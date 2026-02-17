namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

using TC.Agro.Analytics.Application.UseCases.ResolveAlert;

/// <summary>
/// Endpoint to resolve an alert.
/// PATCH /alerts/{alertId}/resolve
/// </summary>
public sealed class ResolveAlertEndpoint : BaseApiEndpoint<ResolveAlertCommand, ResolveAlertResponse>
{
    public override void Configure()
    {
        Patch("/alerts/{alertId}/resolve");

        AllowAnonymous();
        //// Roles(AppConstants.UserRole, AppConstants.AdminRole, AppConstants.ProducerRole);

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

        if (response.IsSuccess)
        {
            await Send.OkAsync(response.Value, cancellation: ct).ConfigureAwait(false);
            return;
        }

        await MatchResultAsync(response, ct).ConfigureAwait(false);
    }
}
