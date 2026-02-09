namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to retrieve all pending alerts across all plots.
/// GET /alerts/pending?pageNumber=1&pageSize=100
/// </summary>
public sealed class GetPendingAlertsEndpoint : BaseApiEndpoint<GetPendingAlertsQuery, AlertListResponse>
{
    public override void Configure()
    {
        Get("/alerts/pending");

        // Force FastEndpoints to bind query parameters (pagination)
        RequestBinder(new RequestBinder<GetPendingAlertsQuery>(BindingSource.QueryParams));

        AllowAnonymous(); // TOD0: Add authentication when ready
        //// Roles(AppConstants.UserRole, AppConstants.AdminRole, AppConstants.ProducerRole);

        // TOD0: Enable caching when ICachedQuery is implemented (short TTL for real-time data)
        //// PreProcessor<QueryCachingPreProcessorBehavior<GetPendingAlertsQuery, AlertListResponse>>();
        //// PostProcessor<QueryCachingPostProcessorBehavior<GetPendingAlertsQuery, AlertListResponse>>();

        Summary(s =>
        {
            s.Summary = "Get all pending alerts";
            s.Description = "Retrieves all alerts with status 'Pending' across all plots, ordered by creation date (most recent first)";
            s.Params["pageNumber"] = "Page number (default: 1)";
            s.Params["pageSize"] = "Page size (default: 100, max: 500)";

            Description(
                d => d.Produces<AlertListResponse>(200, "application/json")
                      .ProducesProblemDetails()
                      .WithTags("Alerts"));

            s.ExampleRequest = new GetPendingAlertsQuery
            {
                PageNumber = 1,
                PageSize = 20
            };

            s.ResponseExamples[200] = new AlertListResponse
            {
                Alerts = new List<AlertResponse>
                {
                    new AlertResponse
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
                        CreatedAt = DateTime.UtcNow.AddMinutes(-15)
                    },
                    new AlertResponse
                    {
                        Id = Guid.NewGuid(),
                        SensorReadingId = Guid.NewGuid(),
                        SensorId = "SENSOR-TEST-002",
                        PlotId = Guid.Parse("7e2b8c3f-9a4d-4f1e-b6c5-8d7f2a1e3c4b"),
                        AlertType = "LowBattery",
                        Message = "Low battery warning: 8.0% - Sensor maintenance required",
                        Status = "Pending",
                        Severity = "High",
                        Value = 8.0,
                        Threshold = 20.0,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                    }
                },
                TotalCount = 15,
                PageNumber = 1,
                PageSize = 20
            };

            s.Responses[200] = "Returned when pending alerts are found.";
            s.Responses[400] = "Returned when the request is invalid.";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
            s.Responses[403] = "Returned when the caller lacks the required role.";
        });


    }

    public override async Task HandleAsync(GetPendingAlertsQuery req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct);
        await MatchResultAsync(response, ct);
    }
}
