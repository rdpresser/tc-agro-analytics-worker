var builder = WebApplication.CreateBuilder(args);

// ========================================
// Configure all Analytics Worker services
// ========================================
builder.Services.AddAnalyticsServices(builder);

var app = builder.Build();

// Apply EF Core migrations automatically (same pattern as Identity-Service)
if (!builder.Environment.IsEnvironment("Testing"))
{
    await app.ApplyMigrations();
}

// ========================================
// Configure HTTP request pipeline
// Pattern: Identity-Service architecture
// ========================================

// 1. CORS (must be early in pipeline)
app.UseCors("DefaultCorsPolicy");

// 2. Ingress PathBase handling (nginx rewrite-target support)
app.UseIngressPathBase(app.Configuration);

// 3. Early-stage middlewares (exception handling, correlation, health checks)
app.UseCustomMiddlewares();

// Authentication/Authorization disabled (anonymous access for now)

// 6. FastEndpoints with Swagger (handles routing + OpenAPI generation)
app.UseCustomFastEndpoints(app.Configuration);

// Health checks are already configured in UseCustomMiddlewares()
// No need for MapAnalyticsHealthChecks() - using middleware approach like Identity Service

await app.RunAsync();
