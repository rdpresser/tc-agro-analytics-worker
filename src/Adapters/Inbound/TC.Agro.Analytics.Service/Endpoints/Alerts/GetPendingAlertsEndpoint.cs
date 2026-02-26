namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to retrieve pending alerts across all plots.
/// GET /alerts/pending?pageNumber=1&pageSize=100
/// Returns alerts with status Pending.
/// Uses PaginatedResponse from SharedKernel (standard pattern).
/// </summary>
public sealed class GetPendingAlertsEndpoint : BaseApiEndpoint<GetPendingAlertsQuery, PaginatedResponse<PendingAlertResponse>>
{
    public override void Configure()
    {
        Get("/alerts/pending");

        // Force FastEndpoints to bind query parameters (pagination)
        RequestBinder(new RequestBinder<GetPendingAlertsQuery>(BindingSource.QueryParams));

        Roles(AppConstants.UserRole, AppConstants.AdminRole, AppConstants.ProducerRole);
        PreProcessor<QueryCachingPreProcessorBehavior<GetPendingAlertsQuery, PaginatedResponse<PendingAlertResponse>>>();
        PostProcessor<QueryCachingPostProcessorBehavior<GetPendingAlertsQuery, PaginatedResponse<PendingAlertResponse>>>();

        Description(
            d => d.Produces<PaginatedResponse<PendingAlertResponse>>(200, "application/json")
                  .ProducesProblemDetails()
                  .WithTags("Alerts"));

        Summary(s =>
        {
            s.Summary = "Get pending alerts";
            s.Description = "Retrieves all alerts with status 'Pending', ordered by creation date";
            s.Params["pageNumber"] = "Page number (default: 1)";
            s.Params["pageSize"] = "Page size (default: 100, max: 500)";
            s.Params["ownerId"] = "Optional owner filter for Admin users. Ignored for Producer users (always scoped to authenticated owner).";
            s.Params["search"] = "Optional backend search in alert type/message.";
            s.Params["severity"] = "Optional severity filter: critical, warning, info, high, medium, low.";
            s.Params["status"] = "Optional status filter: pending, acknowledged, resolved, all.";

            s.ExampleRequest = new GetPendingAlertsQuery
            {
                OwnerId = Guid.Parse("d4d4c49a-5c31-4c9d-babf-a2be5148f0a8"),
                PageNumber = 1,
                PageSize = 20
            };

            s.ResponseExamples[200] = new PaginatedResponse<PendingAlertResponse>(
                [
                    new PendingAlertResponse(
                        Guid.NewGuid(),
                        Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                        "HighTemperature",
                        "High temperature detected: 42.5°C",
                        "Pending",
                        "Critical",
                        42.5,
                        35.0,
                        DateTimeOffset.UtcNow.AddMinutes(-15),
                        null,
                        null,
                        null,
                        null,
                        null,
                        "Plot North",
                        "Farm Alpha"),
                    new PendingAlertResponse(
                        Guid.NewGuid(),
                        Guid.Parse("b2c3d4e5-f6a7-4b6c-9d0e-1f2a3b4c5d6e"),
                        "LowBattery",
                        "Low battery warning: 8.0% - Sensor maintenance required",
                        "Pending",
                        "High",
                        8.0,
                        20.0,
                        DateTimeOffset.UtcNow.AddMinutes(-30),
                        null,
                        null,
                        null,
                        null,
                        null,
                        "Plot South",
                        "Farm Beta")
                ],
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
