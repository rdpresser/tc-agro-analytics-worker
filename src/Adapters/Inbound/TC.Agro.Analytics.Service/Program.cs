var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAnalyticsServices(builder);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ========================================
// Configure Serilog as logging provider (using SharedKernel extension)
// ========================================
builder.Host.UseCustomSerilog(builder.Configuration,
    TelemetryConstants.ServiceName,
    TelemetryConstants.ServiceNamespace,
    TelemetryConstants.Version);

var app = builder.Build();

if (!builder.Environment.IsEnvironment("Testing"))
{
    // Apply EF Core migrations automatically (same pattern as Identity-Service)
    await app.ApplyMigrations().ConfigureAwait(false);
}

// Get logger instance for Program and log telemetry configuration
var logger = app.Services.GetRequiredService<ILogger<TC.Agro.Analytics.Service.Program>>();
TelemetryConstants.LogTelemetryConfiguration(logger, app.Configuration);

// Log APM/exporter configuration (Azure Monitor, OTLP, etc.)
// This info was populated during service configuration in ServiceCollectionExtensions
var exporterInfo = app.Services.GetService<TelemetryExporterInfo>();
TelemetryConstants.LogApmExporterConfiguration(logger, exporterInfo);

// 1. Ingress PathBase handling (nginx rewrite-target support)
app.UseIngressPathBase(app.Configuration);

// 2. CORS (must be early in pipeline)
app.UseCors("DefaultCorsPolicy");

// 3. Early-stage middlewares (exception handling, correlation, health checks)
app.UseCustomMiddlewares();

// CRITICAL: TelemetryMiddleware MUST come AFTER CorrelationMiddleware to access correlationIdGenerator.CorrelationId
app.UseMiddleware<TC.Agro.Analytics.Service.Middleware.TelemetryMiddleware>();

app.UseStaticFiles();

app.UseAuthentication()
   .UseAuthorization();

app.MapHub<AlertHub>("/analytics/alertshub");

app.UseCustomFastEndpoints(app.Configuration);

await app.RunAsync();
