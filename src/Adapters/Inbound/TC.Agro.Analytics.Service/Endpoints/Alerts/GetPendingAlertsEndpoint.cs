namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to retrieve all pending alerts across all plots.
/// GET /alerts/pending?pageNumber=1&pageSize=100
/// Uses PaginatedResponse from SharedKernel (standard pattern).
/// </summary>
public sealed class GetPendingAlertsEndpoint : BaseApiEndpoint<GetPendingAlertsQuery, PaginatedResponse<PendingAlertResponse>>
{
    public override void Configure()
    {
        Get("/alerts/pending");

        // Force FastEndpoints to bind query parameters (pagination)
        RequestBinder(new RequestBinder<GetPendingAlertsQuery>(BindingSource.QueryParams));

        AllowAnonymous(); // TOD0: Add authentication when ready
        //// Roles(AppConstants.UserRole, AppConstants.AdminRole, AppConstants.ProducerRole);

        // TOD0: Enable caching when ICachedQuery is implemented (short TTL for real-time data)
        //// PreProcessor<QueryCachingPreProcessorBehavior<GetPendingAlertsQuery, PaginatedResponse<PendingAlertResponse>>>();
        //// PostProcessor<QueryCachingPostProcessorBehavior<GetPendingAlertsQuery, PaginatedResponse<PendingAlertResponse>>>();

        Summary(s =>
        {
            s.Summary = "Get all pending alerts";
            s.Description = "Retrieves all alerts with status 'Pending' across all plots, ordered by creation date (most recent first)";
            s.Params["pageNumber"] = "Page number (default: 1)";
            s.Params["pageSize"] = "Page size (default: 100, max: 500)";

            Description(
                d => d.Produces<PaginatedResponse<PendingAlertResponse>>(200, "application/json")
                      .ProducesProblemDetails()
                      .WithTags("Alerts"));

            s.ExampleRequest = new GetPendingAlertsQuery
            {
                PageNumber = 1,
                PageSize = 20
            };

            s.ResponseExamples[200] = new PaginatedResponse<PendingAlertResponse>(
                new List<PendingAlertResponse>
                {
                    new PendingAlertResponse(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        "SENSOR-TEST-001",
                        Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683"),
                        "HighTemperature",
                        "High temperature detected: 42.5°C",
                        "Pending",
                        "Critical",
                        42.5,
                        35.0,
                        DateTime.UtcNow.AddMinutes(-15),
                        null,
                        null),
                    new PendingAlertResponse(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        "SENSOR-TEST-002",
                        Guid.Parse("7e2b8c3f-9a4d-4f1e-b6c5-8d7f2a1e3c4b"),
                        "LowBattery",
                        "Low battery warning: 8.0% - Sensor maintenance required",
                        "Pending",
                        "High",
                        8.0,
                        20.0,
                        DateTime.UtcNow.AddMinutes(-30),
                        null,
                        null)
                },
                totalCount: 15,
                pageNumber: 1,
                pageSize: 20);

            s.Responses[200] = "Returned when pending alerts are found.";
            s.Responses[400] = "Returned when the request is invalid.";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
            s.Responses[403] = "Returned when the caller lacks the required role.";
        });


    }

    public override async Task HandleAsync(GetPendingAlertsQuery req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);
        await MatchResultAsync(response, ct).ConfigureAwait(false);
    }
}
