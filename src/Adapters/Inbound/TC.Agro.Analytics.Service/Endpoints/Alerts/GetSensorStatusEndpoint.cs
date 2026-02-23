namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to get sensor status summary with aggregated metrics.
/// GET /sensors/{sensorId}/status
/// </summary>
public sealed class GetSensorStatusEndpoint : BaseApiEndpoint<GetSensorStatusQuery, GetSensorStatusResponse>
{
    public override void Configure()
    {
        Get("/sensors/{sensorId}/status");

        Roles(AppConstants.UserRole, AppConstants.AdminRole, AppConstants.ProducerRole);

        PreProcessor<QueryCachingPreProcessorBehavior<GetSensorStatusQuery, GetSensorStatusResponse>>();
        PostProcessor<QueryCachingPostProcessorBehavior<GetSensorStatusQuery, GetSensorStatusResponse>>();

        Description(
            d => d.Produces<GetSensorStatusResponse>(200, "application/json")
                    .ProducesProblemDetails()
                    .Produces(404)
                    .WithTags("Sensors"));

        Summary(s =>
        {
            s.Summary = "Get sensor status summary";
            s.Description = "Retrieves aggregated alert metrics for a sensor (last 7 days)";
            s.Params["sensorId"] = "Sensor ID (GUID)";

            s.ExampleRequest = new GetSensorStatusQuery
            {
                SensorId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d")
            };

            s.ResponseExamples[200] = new GetSensorStatusResponse(
                Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                PendingAlertsCount: 12,
                TotalAlertsLast24Hours: 8,
                TotalAlertsLast7Days: 45,
                MostRecentAlert: new SensorStatusAlertResponse(
                    Guid.NewGuid(),
                    Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                    "HighTemperature",
                    "High temperature detected: 42.5°C",
                    "Pending",
                    "Critical",
                    42.5,
                    35.0,
                    DateTimeOffset.UtcNow.AddHours(-2)),
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

            s.Responses[200] = "Returned when the sensor status is successfully retrieved.";
            s.Responses[400] = "Returned when the request is invalid (e.g., invalid GUID format).";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
            s.Responses[403] = "Returned when the caller lacks the required role.";
            s.Responses[404] = "Returned when no sensor is found with the given ID.";
        });
    }

    public override async Task HandleAsync(GetSensorStatusQuery req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);
        await MatchResultAsync(response, ct).ConfigureAwait(false);
    }
}
