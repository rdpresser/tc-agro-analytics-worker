using TC.Agro.Analytics.Application;
using TC.Agro.Analytics.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Add Application layer (with configuration)
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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
