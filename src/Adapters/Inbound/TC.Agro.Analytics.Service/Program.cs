using FastEndpoints;
using FastEndpoints.Swagger;
using TC.Agro.Analytics.Application;
using TC.Agro.Analytics.Infrastructure;
using TC.Agro.SharedKernel.Infrastructure.Middleware;
using ZiggyCreatures.Caching.Fusion;
using Wolverine;
using Wolverine.RabbitMQ;
using Wolverine.Marten;
using Marten;

var builder = WebApplication.CreateBuilder(args);

// Configure Marten (Event Store + Document DB)
var postgresConfig = builder.Configuration.GetSection("Database:Postgres");
var connectionString = $"Host={postgresConfig["Host"]};Port={postgresConfig["Port"]};Database={postgresConfig["Database"]};Username={postgresConfig["UserName"]};Password={postgresConfig["Password"]};SearchPath={postgresConfig["Schema"]};";

builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionString);

    // Configure schema
    opts.DatabaseSchemaName = postgresConfig["Schema"] ?? "analytics";

    // Configure event store
    opts.Events.DatabaseSchemaName = postgresConfig["Schema"] ?? "analytics";
})
.IntegrateWithWolverine(); // Integração com Wolverine para Outbox pattern

// Configure Wolverine for message handling
builder.Host.UseWolverine(opts =>
{
    // Configure RabbitMQ transport
    var rabbitMqConfig = builder.Configuration.GetSection("Messaging:RabbitMQ");
    var host = rabbitMqConfig["Host"] ?? "localhost";
    var port = int.Parse(rabbitMqConfig["Port"] ?? "5672");
    var username = rabbitMqConfig["UserName"] ?? "guest";
    var password = rabbitMqConfig["Password"] ?? "guest";

    // Configure RabbitMQ and declare queue in one step
    var rabbitMq = opts.UseRabbitMq(rabbit =>
    {
        rabbit.HostName = host;
        rabbit.Port = port;
        rabbit.UserName = username;
        rabbit.Password = password;
    });

    // Declare queue with proper configuration
    rabbitMq.DeclareQueue("analytics.sensor.ingested.queue", queue =>
    {
        queue.IsDurable = true;
        queue.IsExclusive = false;
        queue.AutoDelete = false;
    });

    // Listen to the queue (Wolverine will create it if doesn't exist)
    opts.ListenToRabbitQueue("analytics.sensor.ingested.queue");

    // Auto-discover handlers in Application AND Infrastructure layers
    opts.Discovery.IncludeAssembly(typeof(TC.Agro.Analytics.Application.DependencyInjection).Assembly);
    opts.Discovery.IncludeAssembly(typeof(TC.Agro.Analytics.Infrastructure.DependencyInjection).Assembly);
});

// Add services to the container.
builder.Services.AddOpenApi();

// Add CORS (for development/testing)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add HttpContextAccessor (required by UserContext)
builder.Services.AddHttpContextAccessor();

// Add Correlation ID Generator (required by SharedKernel)
builder.Services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();

// Add FusionCache (in-memory for now, can be upgraded to Redis later)
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(options =>
    {
        options.Duration = TimeSpan.FromMinutes(5);
        options.IsFailSafeEnabled = true;
        options.FailSafeMaxDuration = TimeSpan.FromHours(1);
    });

// Add FastEndpoints
builder.Services.AddFastEndpoints();

// Add Swagger for FastEndpoints
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "TC.Agro Analytics API";
        s.Version = "v1";
        s.Description = "Analytics Worker API for sensor data processing and alert management";
    };
});

// Add Application layer (with configuration)
builder.Services.AddApplication(builder.Configuration);

// Add Infrastructure layer (with database and projections)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerGen(); // FastEndpoints Swagger UI
}

// Use CORS
app.UseCors();

// Use Correlation Middleware (from SharedKernel)
app.UseMiddleware<CorrelationMiddleware>();

// Use FastEndpoints
app.UseFastEndpoints();

// Health Check Endpoint
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow,
        service = "Analytics Worker Service"
    });
})
    .Produces(StatusCodes.Status200OK)
    .WithName("Health Check")
    .WithDescription("Verifica a saúde da aplicação");

await app.RunAsync();
