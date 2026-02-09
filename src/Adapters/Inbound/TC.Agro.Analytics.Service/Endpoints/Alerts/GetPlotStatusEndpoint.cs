using TC.Agro.Analytics.Application.UseCases.Shared;
using TC.Agro.Analytics.Application.UseCases.GetPlotStatus;
using TC.Agro.SharedKernel.Api.Endpoints;

namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to get plot status summary with aggregated metrics.
/// GET /plots/{plotId}/status
/// </summary>
public sealed class GetPlotStatusEndpoint : BaseApiEndpoint<GetPlotStatusQuery, PlotStatusResponse>
{
    public override void Configure()
    {
        Get("/plots/{plotId}/status");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get plot status summary";
            s.Description = "Retrieves aggregated alert metrics for a plot (last 7 days)";
            s.Params["plotId"] = "Plot ID (GUID)";

            s.ExampleRequest = new GetPlotStatusQuery
            {
                PlotId = Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683")
            };

            s.ResponseExamples[200] = new PlotStatusResponse
            {
                PlotId = Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683"),
                PendingAlertsCount = 12,
                TotalAlertsLast24Hours = 8,
                TotalAlertsLast7Days = 45,
                OverallStatus = "Warning",
                AlertsByType = new Dictionary<string, int>
                {
                    { "HighTemperature", 8 },
                    { "LowSoilMoisture", 4 },
                    { "LowBattery", 0 }
                },
                AlertsBySeverity = new Dictionary<string, int>
                {
                    { "Critical", 3 },
                    { "High", 7 },
                    { "Medium", 2 },
                    { "Low", 0 }
                },
                MostRecentAlert = new AlertResponse
                {
                    Id = Guid.NewGuid(),
                    SensorReadingId = Guid.NewGuid(),
                    SensorId = "SENSOR-TEST-001",
                    PlotId = Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683"),
                    AlertType = "HighTemperature",
                    Message = "High temperature detected: 42.5°C",
                    Status = "Pending",
                    Severity = "Critical",
                    Value = 42.5,
                    Threshold = 35.0,
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                }
            };

            s.Responses[200] = "Returned when the plot status is successfully retrieved.";
            s.Responses[400] = "Returned when the request is invalid (e.g., invalid GUID format).";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
            s.Responses[403] = "Returned when the caller lacks the required role.";
            s.Responses[404] = "Returned when no plot is found with the given ID.";
        });

        Description(d => d
            .Produces<PlotStatusResponse>(200, "application/json")
            .ProducesProblemDetails()
            .Produces(404)
            .WithTags("Plots"));
    }

    public override async Task HandleAsync(GetPlotStatusQuery req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct);
        await MatchResultAsync(response, ct);
    }
}
