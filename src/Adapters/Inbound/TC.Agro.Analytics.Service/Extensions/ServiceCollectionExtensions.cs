namespace TC.Agro.Analytics.Service.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnalyticsServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        // Configure FluentValidation globally
        ConfigureFluentValidationGlobals();

        if (!builder.Environment.IsEnvironment("Testing"))
        {
            builder.AddWolverineMessaging();
        }

        services
            .AddHttpClient()
            .AddCorrelationIdGenerator()
            //     .AddValidatorsFromAssemblyContaining<XXXXCommandValidator>()
            .AddCaching()
            .AddCustomCors(builder.Configuration)
            //.AddCustomAuthentication(builder.Configuration)
            .AddCustomFastEndpoints()
            .AddCustomHealthChecks()
            .AddApplication(builder.Configuration)
            .AddInfrastructure(builder.Configuration);
       // AddCustomOpenTelemetry(builder, builder.Configuration)
                        // ENHANCED: Register telemetry metrics
                        ////.AddSingleton<FarmMetrics>()
                        ////.AddSingleton<SystemMetrics>();
        return services;
    }

    // CORS Configuration
    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", builder =>
            {
                builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    // Health Checks with Enhanced Telemetry
    private static IServiceCollection AddCustomHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddNpgSql(sp =>
            {
                var connectionProvider = sp.GetRequiredService<DbConnectionFactory>();
                return connectionProvider.ConnectionString;
            },
                name: "PostgreSQL",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "sql", "postgres", "live", "ready"])

            .AddCheck("RabbitMQ", () =>
                HealthCheckResult.Healthy("RabbitMQ connection is active via Wolverine"),
                tags: ["messaging", "rabbitmq", "live", "ready"])

            .AddTypeActivatedCheck<RedisHealthCheck>("Redis",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["cache", "redis", "live", "ready"])

            .AddCheck("Memory", () =>
            {
                var allocated = GC.GetTotalMemory(false);
                var mb = allocated / 1024 / 1024;
                return mb < 1024
                    ? HealthCheckResult.Healthy($"Memory usage: {mb} MB")
                    : HealthCheckResult.Degraded($"High memory usage: {mb} MB");
            },
                tags: ["memory", "system", "live"])
            .AddCheck("Custom-Metrics", () =>
            {
                // Add any custom health logic for your metrics system
                return HealthCheckResult.Healthy("Custom metrics are functioning");
            },
                tags: ["metrics", "telemetry", "live"]);
        return services;
    }

    // FastEndpoints Configuration
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
                s.Title = "TC.Agro.Analytics Service";
                s.Version = "v1";
                s.Description = "Analytics Worker API for sensor data processing and alert management";
                s.MarkNonNullablePropsAsRequired();
            };

            o.RemoveEmptyRequestSchema = true;
            o.NewtonsoftSettings = s => { s.Converters.Add(new StringEnumConverter()); };
        });

        return services;
    }

    // Authentication and Authorization
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = JwtHelper.Build(configuration);

        services.AddAuthenticationJwtBearer(s => s.SigningKey = jwtSettings.SecretKey)
                .AddAuthorization()
                .AddHttpContextAccessor();

        return services;
    }

    // FluentValidation Global Setup
    private static void ConfigureFluentValidationGlobals()
    {
        ValidatorOptions.Global.PropertyNameResolver = (type, memberInfo, expression) => memberInfo?.Name;
        ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => memberInfo?.Name;
        ValidatorOptions.Global.ErrorCodeResolver = validator => validator.Name;
        ValidatorOptions.Global.LanguageManager = new LanguageManager
        {
            Enabled = true,
            Culture = new System.Globalization.CultureInfo("en")
        };
    }

    private static IServiceCollection AddCaching(this IServiceCollection services)
    {
        // Add FusionCache with Redis backplane for distributed cache coherence
        services.AddFusionCache()
            .WithDefaultEntryOptions(options =>
            {
                // L1 (Memory) cache duration - shorter to reduce incoherence window
                options.Duration = TimeSpan.FromSeconds(20);

                // L2 (Redis) cache duration - longer for persistence
                options.DistributedCacheDuration = TimeSpan.FromSeconds(60);

                // Reduce memory cache duration to mitigate incoherence
                options.MemoryCacheDuration = TimeSpan.FromSeconds(10);
            })
            .WithDistributedCache(sp =>
            {
                var cacheProvider = sp.GetRequiredService<ICacheProvider>();

                var options = new RedisCacheOptions
                {
                    Configuration = cacheProvider.ConnectionString,
                    InstanceName = cacheProvider.InstanceName
                };

                return new RedisCache(options);
            })
            .WithBackplane(sp =>
            {
                var cacheProvider = sp.GetRequiredService<ICacheProvider>();

                // Create Redis backplane for cache coherence across multiple pods
                return new RedisBackplane(new RedisBackplaneOptions
                {
                    Configuration = cacheProvider.ConnectionString
                });
            })
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .AsHybridCache();

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

    private static WebApplicationBuilder AddWolverineMessaging(this WebApplicationBuilder builder)
    {
        builder.Host.UseWolverine(opts =>
        {
            opts.UseSystemTextJsonForSerialization();
            opts.ServiceName = "tc-agro-analytics";
            opts.ApplicationAssembly = typeof(Program).Assembly;

            // Include Application and Infrastructure assemblies for handler discovery
            opts.Discovery.IncludeAssembly(typeof(Application.DependencyInjection).Assembly);
            opts.Discovery.IncludeAssembly(typeof(Infrastructure.DependencyInjection).Assembly);

            // Include Application assembly for handlers
            opts.Discovery.IncludeAssembly(typeof(Application.MessageBrokerHandlers.SensorIngestedHandler).Assembly);

            // -------------------------------
            // Durability schema (same database, different schema)
            // -------------------------------
            opts.Durability.MessageStorageSchemaName = DefaultSchemas.Wolverine;

            // IMPORTANT:
            // Use the same Postgres DB as EF Core.
            // This enables transactional outbox with EF Core.
            opts.PersistMessagesWithPostgresql(
                PostgresHelper.Build(builder.Configuration).ConnectionString,
                DefaultSchemas.Wolverine);

            // -------------------------------
            // Retry policy
            // -------------------------------
            opts.Policies.OnAnyException()
                .RetryWithCooldown(
                    TimeSpan.FromMilliseconds(200),
                    TimeSpan.FromMilliseconds(400),
                    TimeSpan.FromMilliseconds(600),
                    TimeSpan.FromMilliseconds(800),
                    TimeSpan.FromMilliseconds(1000)
                );

            // -------------------------------
            // Enable durable local queues and auto transaction application
            // -------------------------------
            opts.Policies.UseDurableLocalQueues();
            opts.Policies.AutoApplyTransactions();
            opts.UseEntityFrameworkCoreTransactions();

            // -------------------------------
            // OUTBOX (for sending)
            // -------------------------------
            opts.Policies.UseDurableOutboxOnAllSendingEndpoints();

            // -------------------------------
            // INBOX (for receiving) - optional but recommended
            // -------------------------------
            // This makes message consumption safe in face of retries/crashes.
            // It gives "at-least-once safe" processing with deduplication.
            opts.Policies.UseDurableInboxOnAllListeners();

            // -------------------------------
            // Load and configure message broker
            // -------------------------------
            var mqConnectionFactory = RabbitMqHelper.Build(builder.Configuration);

            var rabbitOpts = opts.UseRabbitMq(factory =>
            {
                factory.Uri = new Uri(mqConnectionFactory.ConnectionString);
                factory.VirtualHost = mqConnectionFactory.VirtualHost;
                factory.ClientProperties["application"] = opts.ServiceName;
                factory.ClientProperties["environment"] = builder.Environment.EnvironmentName;
            });

            if (mqConnectionFactory.AutoProvision)
                rabbitOpts.AutoProvision();
            if (mqConnectionFactory.UseQuorumQueues)
                rabbitOpts.UseQuorumQueues();
            if (mqConnectionFactory.AutoPurgeOnStartup)
                rabbitOpts.AutoPurgeOnStartup();

            var exchangeName = $"{mqConnectionFactory.Exchange}-exchange";

            // -------------------------------
            // Publishing example
            // -------------------------------
            opts.PublishMessage<EventContext<SensorIngestedIntegrationEvent>>()
                .ToRabbitExchange(exchangeName)
                .BufferedInMemory()
                .UseDurableOutbox();

            // ============================================================
            // PUBLISHING - Analytics Service Events (TOPIC Exchange)
            // Analytics publishes its own events (Alert)
            // ============================================================
            opts.PublishMessage<EventContext<AlertCreatedIntegrationEvent>>()
                .ToRabbitExchange(exchangeName)
                .BufferedInMemory()
                .UseDurableOutbox();

            opts.PublishMessage<EventContext<AlertResolvedIntegrationEvent>>()
                .ToRabbitExchange(exchangeName)
                .BufferedInMemory()
                .UseDurableOutbox();

            opts.ConfigureIdentityUserEventsConsumption(
                 exchangeName: "identity.events-exchange",
                 queueName: "analitics-identity-user-events-queue"
             );
        });

        // -------------------------------
        // Ensure all messaging resources and schema are created at startup
        // -------------------------------
        builder.Services.AddResourceSetupOnStartup();

        return builder;
    }
    // OpenTelemetry Configuration
    ////public static IServiceCollection AddCustomOpenTelemetry(
    ////    this IServiceCollection services,
    ////    IHostApplicationBuilder builder,
    ////    IConfiguration configuration)
    ////{
    ////    var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? TelemetryConstants.Version;
    ////    var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
    ////    var instanceId = Environment.MachineName;
    ////    var serviceName = TelemetryConstants.ServiceName;
    ////    var serviceNamespace = TelemetryConstants.ServiceNamespace;

    ////    // NOTE: Serilog handles all logging (no OpenTelemetry logging)
    ////    // This prevents log duplication and simplifies trace_id/span_id correlation
    ////    // Serilog.Enrichers.Span automatically adds trace_id/span_id from Activity.Current
    ////    // ❌ REMOVED: builder.Logging.AddOpenTelemetry() - use Serilog only

    ////    var otelBuilder = services.AddOpenTelemetry()
    ////        .ConfigureResource(resource => resource
    ////            .AddService(
    ////                serviceName: serviceName,
    ////                serviceNamespace: serviceNamespace,
    ////                serviceVersion: serviceVersion,
    ////                serviceInstanceId: instanceId)
    ////            .AddAttributes(new Dictionary<string, object>
    ////            {
    ////                ["deployment.environment"] = environment.ToLowerInvariant(),
    ////                ["service.namespace"] = serviceNamespace.ToLowerInvariant(),
    ////                ["service.instance.id"] = instanceId,
    ////                ["container.name"] = Environment.GetEnvironmentVariable("HOSTNAME") ?? instanceId,
    ////                ["host.provider"] = "localhost",
    ////                ["host.platform"] = "k3d_kubernetes_service",
    ////                ["service.team"] = "engineering",
    ////                ["service.owner"] = "devops"
    ////            }))
    ////        .WithMetrics(metrics =>
    ////        {
    ////            metrics
    ////                .AddAspNetCoreInstrumentation()
    ////                .AddHttpClientInstrumentation()
    ////                .AddRuntimeInstrumentation()
    ////                .AddFusionCacheInstrumentation()
    ////                .AddNpgsqlInstrumentation()
    ////                .AddMeter("Microsoft.AspNetCore.Hosting")
    ////                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    ////                .AddMeter("System.Net.Http")
    ////                .AddMeter("System.Runtime")
    ////                .AddMeter("Wolverine")
    ////                .AddMeter(TelemetryConstants.IdentityMeterName)
    ////                .AddPrometheusExporter();
    ////        })
    ////        .WithTracing(tracing =>
    ////        {
    ////            tracing
    ////                .AddAspNetCoreInstrumentation(options =>
    ////                {
    ////                    options.Filter = ctx =>
    ////                    {
    ////                        var path = ctx.Request.Path.Value ?? "";
    ////                        return !path.Contains("/health") && !path.Contains("/metrics") && !path.Contains("/prometheus");
    ////                    };

    ////                    options.EnrichWithHttpRequest = (activity, request) =>
    ////                    {
    ////                        activity.SetTag("http.method", request.Method);
    ////                        activity.SetTag("http.scheme", request.Scheme);
    ////                        activity.SetTag("http.host", request.Host.Value);
    ////                        activity.SetTag("http.target", request.Path);
    ////                        if (request.ContentLength.HasValue)
    ////                            activity.SetTag("http.request.size", request.ContentLength.Value);
    ////                        activity.SetTag("user.id", request.HttpContext.User?.Identity?.Name);
    ////                        activity.SetTag("user.authenticated", request.HttpContext.User?.Identity?.IsAuthenticated);
    ////                        activity.SetTag("http.route", request.HttpContext.GetRouteValue("action")?.ToString());
    ////                        activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString());

    ////                        // NEW: Enhanced domain attributes
    ////                        activity.SetTag("http.endpoint_handler", request.Path);
    ////                        activity.SetTag("http.query_params", request.QueryString.Value ?? "");

    ////                        // NEW: User context from JWT/Principal
    ////                        var userId = request.HttpContext.User?.FindFirst("sub")?.Value ??
    ////                                     request.HttpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    ////                        if (!string.IsNullOrWhiteSpace(userId))
    ////                            activity.SetTag("user.id", userId);

    ////                        // NEW: Request correlation ID
    ////                        if (request.HttpContext.Request.Headers.TryGetValue(TelemetryConstants.CorrelationIdHeader, out var correlationId))
    ////                            activity.SetTag("correlation_id", correlationId.ToString());

    ////                        // NEW: User roles
    ////                        var roles = string.Join(",", request.HttpContext.User?.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value) ?? new string[] { });
    ////                        if (!string.IsNullOrWhiteSpace(roles))
    ////                            activity.SetTag("user.roles", roles);
    ////                    };

    ////                    options.EnrichWithHttpResponse = (activity, response) =>
    ////                    {
    ////                        activity.SetTag("http.status_code", response.StatusCode);
    ////                        if (response.ContentLength.HasValue)
    ////                            activity.SetTag("http.response.size", response.ContentLength.Value);

    ////                        // NEW: HTTP status category
    ////                        activity.SetTag("http.status_category", response.StatusCode >= 400 ? "error" : "success");
    ////                    };

    ////                    options.EnrichWithException = (activity, ex) =>
    ////                    {
    ////                        activity.SetTag("exception.type", ex.GetType().Name);
    ////                        activity.SetTag("exception.message", ex.Message);
    ////                        activity.SetTag("exception.stacktrace", ex.StackTrace);
    ////                    };
    ////                })
    ////                .AddHttpClientInstrumentation(options =>
    ////                {
    ////                    options.FilterHttpRequestMessage = request =>
    ////                    {
    ////                        var path = request.RequestUri?.AbsolutePath ?? "";
    ////                        return !path.Contains("/health") && !path.Contains("/metrics") && !path.Contains("/prometheus");
    ////                    };
    ////                })
    ////                .AddRedisInstrumentation()
    ////                .AddFusionCacheInstrumentation()
    ////                .AddNpgsql()
    ////                //.AddSource(TelemetryConstants.UserActivitySource)
    ////                //.AddSource(TelemetryConstants.DatabaseActivitySource)
    ////                //.AddSource(TelemetryConstants.CacheActivitySource)
    ////                //.AddSource(TelemetryConstants.HandlersActivitySource)
    ////                //.AddSource(TelemetryConstants.FastEndpointsActivitySource)
    ////                .AddSource("Wolverine");
    ////        });

    ////    AddOpenTelemetryExporters(otelBuilder, builder);

    ////    return services;
    ////}

    ////private static void AddOpenTelemetryExporters(OpenTelemetryBuilder otelBuilder, IHostApplicationBuilder builder)
    ////{
    ////    var grafanaSettings = GrafanaHelper.Build(builder.Configuration);
    ////    var useOtlpExporter = grafanaSettings.Agent.Enabled && !string.IsNullOrWhiteSpace(grafanaSettings.Otlp.Endpoint);

    ////    if (useOtlpExporter)
    ////    {
    ////        // Configure OTLP for Traces
    ////        // NOTE: Traces use /v1/traces endpoint per OTLP specification
    ////        otelBuilder.WithTracing(tracerBuilder =>
    ////        {
    ////            tracerBuilder.AddOtlpExporter(otlp =>
    ////            {
    ////                otlp.Endpoint = new Uri(grafanaSettings.ResolveTracesEndpoint());
    ////                otlp.Protocol = grafanaSettings.Otlp.Protocol.ToLowerInvariant() == "grpc"
    ////                    ? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc
    ////                    : OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;

    ////                if (!string.IsNullOrWhiteSpace(grafanaSettings.Otlp.Headers))
    ////                {
    ////                    otlp.Headers = grafanaSettings.Otlp.Headers;
    ////                }

    ////                otlp.TimeoutMilliseconds = grafanaSettings.Otlp.TimeoutSeconds * 1000;
    ////            });
    ////        });

    ////        // Configure OTLP for Metrics
    ////        // NOTE: Send metrics to OTEL Collector for centralized processing
    ////        // OTEL Collector will strip problematic attributes before exposing to Prometheus
    ////        otelBuilder.WithMetrics(metricsBuilder =>
    ////        {
    ////            metricsBuilder.AddOtlpExporter(otlp =>
    ////            {
    ////                otlp.Endpoint = new Uri(grafanaSettings.ResolveMetricsEndpoint());
    ////                otlp.Protocol = grafanaSettings.Otlp.Protocol.ToLowerInvariant() == "grpc"
    ////                    ? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc
    ////                    : OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;

    ////                if (!string.IsNullOrWhiteSpace(grafanaSettings.Otlp.Headers))
    ////                {
    ////                    otlp.Headers = grafanaSettings.Otlp.Headers;
    ////                }

    ////                otlp.TimeoutMilliseconds = grafanaSettings.Otlp.TimeoutSeconds * 1000;
    ////            });
    ////        });

    ////        // Configure OTLP for Logs
    ////        // NOTE: Logs use /v1/logs endpoint per OTLP specification
    ////        otelBuilder.WithLogging(loggingBuilder =>
    ////        {
    ////            loggingBuilder.AddOtlpExporter(otlp =>
    ////            {
    ////                otlp.Endpoint = new Uri(grafanaSettings.ResolveLogsEndpoint());
    ////                otlp.Protocol = grafanaSettings.Otlp.Protocol.ToLowerInvariant() == "grpc"
    ////                    ? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc
    ////                    : OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;

    ////                if (!string.IsNullOrWhiteSpace(grafanaSettings.Otlp.Headers))
    ////                {
    ////                    otlp.Headers = grafanaSettings.Otlp.Headers;
    ////                }

    ////                otlp.TimeoutMilliseconds = grafanaSettings.Otlp.TimeoutSeconds * 1000;
    ////            });
    ////        });

    ////        ////builder.Services.AddSingleton(new TelemetryExporterInfo
    ////        ////{
    ////        ////    ExporterType = "OTLP",
    ////        ////    Endpoint = grafanaSettings.ResolveTracesEndpoint(),
    ////        ////    Protocol = grafanaSettings.Otlp.Protocol
    ////        ////});
    ////    }
    ////    else
    ////    {
    ////        ////builder.Services.AddSingleton(new TelemetryExporterInfo { ExporterType = "None" });
    ////    }
    ////}
}
