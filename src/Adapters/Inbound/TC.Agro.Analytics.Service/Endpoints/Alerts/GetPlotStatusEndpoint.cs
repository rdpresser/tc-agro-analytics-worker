namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to get plot status summary with aggregated metrics.
/// GET /plots/{plotId}/status
/// </summary>
public sealed class GetPlotStatusEndpoint : BaseApiEndpoint<GetPlotStatusQuery, GetPlotStatusResponse>
{
    public override void Configure()
    {
        Get("/plots/{plotId}/status");

        AllowAnonymous(); // TOD0: Add authentication when ready
        //// Roles(AppConstants.UserRole, AppConstants.AdminRole, AppConstants.ProducerRole);

        // TOD0: Enable caching when ICachedQuery is implemented (good candidate for caching)
        //// PreProcessor<QueryCachingPreProcessorBehavior<GetPlotStatusQuery, GetPlotStatusResponse>>();
        //// PostProcessor<QueryCachingPostProcessorBehavior<GetPlotStatusQuery, GetPlotStatusResponse>>();

        Summary(s =>
        {
            s.Summary = "Get plot status summary";
            s.Description = "Retrieves aggregated alert metrics for a plot (last 7 days)";
            s.Params["plotId"] = "Plot ID (GUID)";

            Description(
                d => d.Produces<GetPlotStatusResponse>(200, "application/json")
                      .ProducesProblemDetails()
                      .Produces(404)
                      .WithTags("Plots"));

            s.ExampleRequest = new GetPlotStatusQuery
            {
                PlotId = Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683")
            };

            s.ResponseExamples[200] = new GetPlotStatusResponse(
                Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683"),
                PendingAlertsCount: 12,
                TotalAlertsLast24Hours: 8,
                TotalAlertsLast7Days: 45,
                MostRecentAlert: new PlotStatusAlertResponse(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "SENSOR-TEST-001",
                    "HighTemperature",
                    "High temperature detected: 42.5°C",
                    "Pending",
                    "Critical",
                    42.5,
                    35.0,
                    DateTime.UtcNow.AddHours(-2)),
                AlertsByType: new Dictionary<string, int>
                {
                    { "HighTemperature", 8 },
                    { "LowSoilMoisture", 4 },
                    { "LowBattery", 0 }
                },
                AlertsBySeverity: new Dictionary<string, int>
                {
                    { "Critical", 3 },
                    { "High", 7 },
                    { "Medium", 2 },
                    { "Low", 0 }
                },
                OverallStatus: "Warning");

            s.Responses[200] = "Returned when the plot status is successfully retrieved.";
            s.Responses[400] = "Returned when the request is invalid (e.g., invalid GUID format).";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
            s.Responses[403] = "Returned when the caller lacks the required role.";
            s.Responses[404] = "Returned when no plot is found with the given ID.";
        });
    }

    public override async Task HandleAsync(GetPlotStatusQuery req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct);
        await MatchResultAsync(response, ct);
    }
}
