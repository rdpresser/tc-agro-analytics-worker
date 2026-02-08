using FastEndpoints;
using FastEndpoints.Swagger;
using TC.Agro.Analytics.Service.Extensions;
using TC.Agro.SharedKernel.Infrastructure.Database.EfCore;
using TC.Agro.SharedKernel.Infrastructure.Middleware;

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
// ========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseCors();
app.UseMiddleware<CorrelationMiddleware>();
app.UseFastEndpoints();
app.MapAnalyticsHealthChecks();

await app.RunAsync();
