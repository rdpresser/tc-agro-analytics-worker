using FastEndpoints;
using FastEndpoints.Swagger;
using TC.Agro.Analytics.Service.Extensions;
using TC.Agro.SharedKernel.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// Configure all Analytics Worker services
// ========================================
builder.Services.AddAnalyticsServices(builder);
builder.Services.AddOpenApi();

var app = builder.Build();

// ========================================
// Configure HTTP request pipeline
// ========================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerGen();
}

app.UseCors();
app.UseMiddleware<CorrelationMiddleware>();
app.UseFastEndpoints();
app.MapAnalyticsHealthChecks();

await app.RunAsync();
