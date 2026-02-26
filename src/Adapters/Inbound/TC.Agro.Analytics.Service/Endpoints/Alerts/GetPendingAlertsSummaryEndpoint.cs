namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to retrieve aggregated summary for pending alerts.
/// GET /alerts/pending/summary?ownerId={guid}&windowHours=24
/// </summary>
public sealed class GetPendingAlertsSummaryEndpoint : BaseApiEndpoint<GetPendingAlertsSummaryQuery, PendingAlertsSummaryResponse>
{
    public override void Configure()
    {
        Get("/alerts/pending/summary");

        RequestBinder(new RequestBinder<GetPendingAlertsSummaryQuery>(BindingSource.QueryParams));

        Roles(AppConstants.UserRole, AppConstants.AdminRole, AppConstants.ProducerRole);

        PreProcessor<QueryCachingPreProcessorBehavior<GetPendingAlertsSummaryQuery, PendingAlertsSummaryResponse>>();
        PostProcessor<QueryCachingPostProcessorBehavior<GetPendingAlertsSummaryQuery, PendingAlertsSummaryResponse>>();

        Description(d => d.Produces<PendingAlertsSummaryResponse>(200, "application/json")
                          .ProducesProblemDetails()
                          .WithTags("Alerts"));

        Summary(s =>
        {
            s.Summary = "Get pending alerts summary";
            s.Description = "Retrieves aggregated metrics for pending alerts including distinct affected plots/sensors";
            s.Params["ownerId"] = "Optional owner filter for Admin users. Ignored for Producer users (always scoped to authenticated owner).";
            s.Params["windowHours"] = "Window in hours for the new pending alerts metric (default: 24, max: 720).";

            s.ExampleRequest = new GetPendingAlertsSummaryQuery
            {
                OwnerId = Guid.Parse("d4d4c49a-5c31-4c9d-babf-a2be5148f0a8"),
                WindowHours = 24
            };

            s.ResponseExamples[200] = new PendingAlertsSummaryResponse(
                PendingAlertsTotal: 12,
                AffectedPlotsCount: 4,
                AffectedSensorsCount: 6,
                CriticalPendingCount: 3,
                HighPendingCount: 4,
                MediumPendingCount: 3,
                LowPendingCount: 2,
                NewPendingInWindowCount: 5,
                WindowHours: 24);

            s.Responses[200] = "Returned when the pending alerts summary is successfully retrieved.";
            s.Responses[400] = "Returned when the request is invalid.";
            s.Responses[401] = "Returned when the request is made without a valid user token.";
            s.Responses[403] = "Returned when the caller lacks the required role.";
        });
    }

    public override async Task HandleAsync(GetPendingAlertsSummaryQuery req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);
        await MatchResultAsync(response, ct).ConfigureAwait(false);
    }
}
