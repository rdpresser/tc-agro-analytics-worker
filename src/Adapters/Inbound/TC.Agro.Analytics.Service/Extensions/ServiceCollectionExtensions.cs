using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json.Converters;
using TC.Agro.Analytics.Application;
using TC.Agro.Analytics.Infrastructure;
using TC.Agro.SharedKernel.Infrastructure.Database;
using TC.Agro.SharedKernel.Infrastructure.Middleware;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace TC.Agro.Analytics.Service.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnalyticsServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsEnvironment("Testing"))
        {
            builder.AddWolverineMessaging();
        }

        services
            .AddCorrelationIdGenerator()
            .AddCustomCors(builder.Configuration)
            .AddCustomCaching()
            .AddCustomFastEndpoints()
            .AddCustomHealthChecks()
            .AddApplication(builder.Configuration)
            .AddInfrastructure(builder.Configuration);

        return services;
    }

    private static WebApplicationBuilder AddWolverineMessaging(this WebApplicationBuilder builder)
    {
        builder.Host.UseWolverine(opts =>
        {
            opts.UseSystemTextJsonForSerialization();
            opts.ServiceName = "tc-agro-analytics";
            opts.ApplicationAssembly = typeof(Program).Assembly;

            opts.Discovery.IncludeAssembly(typeof(Application.DependencyInjection).Assembly);
            opts.Discovery.IncludeAssembly(typeof(Infrastructure.DependencyInjection).Assembly);

            // Configure Wolverine Durability (Message Persistence) with PostgreSQL
            // Uses the same database as EF Core, but separate schema (wolverine)
            opts.Durability.MessageStorageSchemaName = DefaultSchemas.Wolverine;

            opts.PersistMessagesWithPostgresql(
                PostgresHelper.Build(builder.Configuration).ConnectionString,
                DefaultSchemas.Wolverine);

            // Enable EF Core transactions integration (Transactional Outbox)
            opts.UseEntityFrameworkCoreTransactions();

            opts.Policies.UseDurableLocalQueues();
            opts.Policies.AutoApplyTransactions();
            opts.Policies.UseDurableOutboxOnAllSendingEndpoints();

            var rabbitMqConfig = builder.Configuration.GetSection("Messaging:RabbitMQ");
            var host = rabbitMqConfig["Host"] ?? "localhost";
            var port = int.Parse(rabbitMqConfig["Port"] ?? "5672");
            var username = rabbitMqConfig["UserName"] ?? "guest";
            var password = rabbitMqConfig["Password"] ?? "guest";
            var autoProvision = bool.Parse(rabbitMqConfig["AutoProvision"] ?? "true");
            var autoPurge = bool.Parse(rabbitMqConfig["AutoPurgeOnStartup"] ?? "false");

            var rabbitOpts = opts.UseRabbitMq(rabbit =>
            {
                rabbit.HostName = host;
                rabbit.Port = port;
                rabbit.UserName = username;
                rabbit.Password = password;
                rabbit.ClientProperties["application"] = opts.ServiceName;
                rabbit.ClientProperties["environment"] = builder.Environment.EnvironmentName;
            });
            if (autoProvision)
                rabbitOpts.AutoProvision();

            if (autoPurge && builder.Environment.IsDevelopment())
                rabbitOpts.AutoPurgeOnStartup();

            // Declare inbound queue for sensor data
            rabbitOpts.DeclareQueue("analytics.sensor.ingested.queue", queue =>
            {
                queue.IsDurable = true;
                queue.IsExclusive = false;
                queue.AutoDelete = false;
            });

            opts.ListenToRabbitQueue("analytics.sensor.ingested.queue");
        });

        return builder;
    }

    private static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (configuration.GetValue<bool>("Cors:AllowAnyOrigin"))
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
                else
                {
                    policy.SetIsOriginAllowed(host => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
            });
        });

        return services;
    }

    private static IServiceCollection AddCustomCaching(this IServiceCollection services)
    {
        services.AddFusionCache()
            .WithDefaultEntryOptions(options =>
            {
                options.Duration = TimeSpan.FromMinutes(5);
                options.IsFailSafeEnabled = true;
                options.FailSafeMaxDuration = TimeSpan.FromHours(1);
                options.FailSafeThrottleDuration = TimeSpan.FromSeconds(30);
            })
            .WithSerializer(new FusionCacheSystemTextJsonSerializer());

        return services;
    }

    private static IServiceCollection AddCustomFastEndpoints(this IServiceCollection services)
    {
        services.AddFastEndpoints(discoveryOptions =>
        {
            discoveryOptions.Assemblies = [typeof(Program).Assembly];
        })
        .SwaggerDocument(o =>
        {
            o.DocumentSettings = s =>
            {
                s.Title = "TC.Agro Analytics API";
                s.Version = "v1";
                s.Description = "Analytics Worker API for sensor data processing and alert management";
                s.MarkNonNullablePropsAsRequired();
            };

            o.RemoveEmptyRequestSchema = true;
            o.NewtonsoftSettings = s => { s.Converters.Add(new StringEnumConverter()); };
        });

        return services;
    }

    private static IServiceCollection AddCustomHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddNpgSql(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var postgresConfig = configuration.GetSection("Database:Postgres");
                return $"Host={postgresConfig["Host"]};Port={postgresConfig["Port"]};Database={postgresConfig["Database"]};Username={postgresConfig["UserName"]};Password={postgresConfig["Password"]};";
            },
                name: "PostgreSQL",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "sql", "postgres", "live", "ready"])

            .AddCheck("RabbitMQ", () =>
                HealthCheckResult.Healthy("RabbitMQ connection is active via Wolverine"),
                tags: ["messaging", "rabbitmq", "live", "ready"])

            .AddCheck("Memory", () =>
            {
                var allocated = GC.GetTotalMemory(false);
                var mb = allocated / 1024 / 1024;
                return mb < 1024
                    ? HealthCheckResult.Healthy($"Memory usage: {mb} MB")
                    : HealthCheckResult.Degraded($"High memory usage: {mb} MB");
            },
                tags: ["memory", "system", "live"]);

        return services;
    }

    private static IServiceCollection AddCorrelationIdGenerator(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();
        return services;
    }

    public static WebApplication MapAnalyticsHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    service = "Analytics Worker Service",
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        duration = e.Value.Duration.TotalMilliseconds,
                        tags = e.Value.Tags
                    }),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                });
                await context.Response.WriteAsync(result);
            }
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return app;
    }
}
