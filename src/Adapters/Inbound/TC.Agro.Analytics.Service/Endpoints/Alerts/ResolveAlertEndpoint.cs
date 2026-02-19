namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to resolve an alert.
/// PUT /alerts/{alertId}/resolve
/// </summary>
public sealed class ResolveAlertEndpoint : BaseApiEndpoint<ResolveAlertCommand, ResolveAlertResponse>
{
    public override void Configure()
    {
        Put("/alerts/{alertId:guid}/resolve");

        Roles(AppConstants.UserRole, AppConstants.AdminRole, AppConstants.ProducerRole);
        PostProcessor<LoggingCommandPostProcessorBehavior<ResolveAlertCommand, ResolveAlertResponse>>();
        PostProcessor<CacheInvalidationPostProcessorBehavior<ResolveAlertCommand, ResolveAlertResponse>>();


        Description(
            d => d.Produces<ResolveAlertResponse>(200, "application/json")
                  .ProducesProblemDetails()
                  .Produces(400)
                  .Produces(404)
                  .WithTags("Alerts"));

        Summary(s =>
        {
            s.Summary = "Resolve an alert";
            s.Description = "Marks an alert as resolved with optional resolution notes";
            s.Params["alertId"] = "Alert ID (GUID)";

            s.ExampleRequest = new ResolveAlertCommand(
                AlertId: Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                UserId: "user@example.com",
                ResolutionNotes: "Activated irrigation system. Soil moisture normalized after 2 hours.");

            s.ResponseExamples[200] = new ResolveAlertResponse(
                Id: Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                Status: "Resolved",
                ResolvedAt: DateTimeOffset.UtcNow,
                ResolvedBy: "user@example.com",
                ResolutionNotes: "Activated irrigation system. Soil moisture normalized after 2 hours.",
                Message: "Alert resolved successfully");

            s.Responses[200] = "Returned when the alert is successfully resolved.";
            s.Responses[400] = "Returned when the request is invalid or alert cannot be resolved (e.g., already resolved).";
            s.Responses[404] = "Returned when the alert is not found.";
        });
    }

    public override async Task HandleAsync(ResolveAlertCommand req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);
        await MatchResultAsync(response, ct).ConfigureAwait(false);
    }
}
