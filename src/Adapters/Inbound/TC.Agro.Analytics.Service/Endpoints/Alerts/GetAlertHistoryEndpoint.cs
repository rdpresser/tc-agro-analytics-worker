namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to retrieve alert history for a specific plot.
/// GET /alerts/history/{plotId}?days=30&alertType=HighTemperature&status=Pending&pageNumber=1&pageSize=100
/// Uses PaginatedResponse from SharedKernel (standard pattern).
/// </summary>
public sealed class GetAlertHistoryEndpoint : BaseApiEndpoint<GetAlertHistoryQuery, PaginatedResponse<AlertHistoryResponse>>
{
    public override void Configure()
    {
        Get("/alerts/history/{plotId}");

        // Force FastEndpoints to bind query parameters (days, alertType, status, pagination)
        RequestBinder(new RequestBinder<GetAlertHistoryQuery>(BindingSource.QueryParams));

        AllowAnonymous();
        //// TOD0: Add authentication when ready
        //// Roles(AppConstants.UserRole, AppConstants.AdminRole, AppConstants.ProducerRole);

        //// TOD0: Enable caching when ICachedQuery is implemented
        //// PreProcessor<QueryCachingPreProcessorBehavior<GetAlertHistoryQuery, PaginatedResponse<AlertHistoryResponse>>>();
        //// PostProcessor<QueryCachingPostProcessorBehavior<GetAlertHistoryQuery, PaginatedResponse<AlertHistoryResponse>>>();

        Description(
            d => d.Produces<PaginatedResponse<AlertHistoryResponse>>(200, "application/json")
                  .ProducesProblemDetails()
                  .Produces(404)
                  .WithTags("Alerts"));

        Summary(s =>
        {
            s.Summary = "Get alert history for a plot";
            s.Description = "Retrieves historical alerts for a specific plot with optional filters and pagination";
            s.Params["plotId"] = "Plot ID (GUID)";
            s.Params["days"] = "Number of days to look back (default: 30)";
            s.Params["alertType"] = "Filter by alert type (optional): HighTemperature, LowSoilMoisture, LowBattery";
            s.Params["status"] = "Filter by status (optional): Pending, Acknowledged, Resolved";
            s.Params["pageNumber"] = "Page number (default: 1)";
            s.Params["pageSize"] = "Page size (default: 100, max: 500)";

            s.ExampleRequest = new GetAlertHistoryQuery
            {
                PlotId = Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683"),
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
                        Guid.NewGuid(),
                        "SENSOR-TEST-001",
                        Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683"),
                        "HighTemperature",
                        "High temperature detected: 42.5°C",
                        "Pending",
                        "High",
                        42.5,
                        35.0,
                        DateTime.UtcNow.AddHours(-2),
                        null,
                        null,
                        null,
                        null,
                        null),
                    new AlertHistoryResponse(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        "SENSOR-TEST-001",
                        Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683"),
                        "LowSoilMoisture",
                        "Low soil moisture detected: 12.0% - Irrigation may be needed",
                        "Pending",
                        "Medium",
                        12.0,
                        20.0,
                        DateTime.UtcNow.AddHours(-1),
                        null,
                        null,
                        null,
                        null,
                        null)
                },
                totalCount: 2,
                pageNumber: 1,
                pageSize: 20);

            s.Responses[200] = "Returned when alerts are found for the plot.";
            s.Responses[400] = "Returned when the request is invalid (e.g., invalid GUID format).";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
            s.Responses[403] = "Returned when the caller lacks the required role.";
            s.Responses[404] = "Returned when no alerts are found for the given plot ID.";
        });
    }

    public override async Task HandleAsync(GetAlertHistoryQuery req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct);
        await MatchResultAsync(response, ct).ConfigureAwait(false);
    }
}
