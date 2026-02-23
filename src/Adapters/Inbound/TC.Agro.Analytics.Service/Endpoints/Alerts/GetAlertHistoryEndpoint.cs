namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to retrieve alert history for a specific sensor.
/// GET /alerts/history/{sensorId}?days=30&alertType=HighTemperature&status=Pending&pageNumber=1&pageSize=100
/// Uses PaginatedResponse from SharedKernel (standard pattern).
/// </summary>
public sealed class GetAlertHistoryEndpoint : BaseApiEndpoint<GetAlertHistoryQuery, PaginatedResponse<AlertHistoryResponse>>
{
    public override void Configure()
    {
        Get("/alerts/history/{sensorId}");

        Roles(AppConstants.UserRole, AppConstants.AdminRole, AppConstants.ProducerRole);

        PreProcessor<QueryCachingPreProcessorBehavior<GetAlertHistoryQuery, PaginatedResponse<AlertHistoryResponse>>>();
        PostProcessor<QueryCachingPostProcessorBehavior<GetAlertHistoryQuery, PaginatedResponse<AlertHistoryResponse>>>();

        Description(
            d => d.Produces<PaginatedResponse<AlertHistoryResponse>>(200, "application/json")
                  .ProducesProblemDetails()
                  .Produces(404)
                  .WithTags("Alerts"));

        Summary(s =>
        {
            s.Summary = "Get alert history for a sensor";
            s.Description = "Retrieves historical alerts for a specific sensor with optional filters and pagination";
            s.Params["sensorId"] = "Sensor ID (GUID)";
            s.Params["pageNumber"] = "Page number (default: 1)";
            s.Params["pageSize"] = "Page size (default: 100, max: 500)";
            s.Params["days"] = "Number of days to look back (default: 30)";
            s.Params["alertType"] = "Filter by alert type (optional): HighTemperature, LowSoilMoisture, LowBattery";
            s.Params["status"] = "Filter by status (optional): Pending, Acknowledged, Resolved";

            s.ExampleRequest = new GetAlertHistoryQuery
            {
                SensorId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                Days = 30,
                AlertType = "HighTemperature",
                Status = "Pending",
                PageNumber = 1,
                PageSize = 20
            };

            s.ResponseExamples[200] = new PaginatedResponse<AlertHistoryResponse>(
                new List<AlertHistoryResponse>
                {
                    new AlertHistoryResponse(
                        Guid.NewGuid(),
                        Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                        "HighTemperature",
                        "High temperature detected: 42.5°C",
                        "Pending",
                        "High",
                        42.5,
                        35.0,
                        DateTimeOffset.UtcNow.AddHours(-2),
                        null,
                        null,
                        null,
                        null,
                        null),
                    new AlertHistoryResponse(
                        Guid.NewGuid(),
                        Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                        "LowSoilMoisture",
                        "Low soil moisture detected: 12.0% - Irrigation may be needed",
                        "Pending",
                        "Medium",
                        12.0,
                        20.0,
                        DateTimeOffset.UtcNow.AddHours(-1),
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                totalCount: 2,
                pageNumber: 1,
                pageSize: 20);

            s.Responses[200] = "Returned when alerts are found for the sensor.";
            s.Responses[400] = "Returned when the request is invalid (e.g., invalid GUID format).";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
            s.Responses[403] = "Returned when the caller lacks the required role.";
            s.Responses[404] = "Returned when no alerts are found for the given sensor ID.";
        });
    }

    public override async Task HandleAsync(GetAlertHistoryQuery req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);
        await MatchResultAsync(response, ct).ConfigureAwait(false);
    }
}
