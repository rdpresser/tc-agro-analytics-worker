namespace TC.Agro.Analytics.Service.Endpoints.Alerts;

/// <summary>
/// Endpoint to acknowledge an alert.
/// PUT /alerts/{alertId}/acknowledge
/// </summary>
public sealed class AcknowledgeAlertEndpoint : BaseApiEndpoint<AcknowledgeAlertCommand, AcknowledgeAlertResponse>
{
    public override void Configure()
    {
        Put("/alerts/{alertId:guid}/acknowledge");

        PostProcessor<LoggingCommandPostProcessorBehavior<AcknowledgeAlertCommand, AcknowledgeAlertResponse>>();
        PostProcessor<CacheInvalidationPostProcessorBehavior<AcknowledgeAlertCommand, AcknowledgeAlertResponse>>();

        Roles(AppConstants.UserRole, AppConstants.AdminRole, AppConstants.ProducerRole);

        Description(
            d => d.Produces<AcknowledgeAlertResponse>(200, "application/json")
                    .ProducesProblemDetails(400)
                    .ProducesProblemDetails(404)
                    .Produces((int)HttpStatusCode.Unauthorized)
                    .Produces((int)HttpStatusCode.Forbidden)
                    .WithTags("Alerts"));

        Summary(s =>
        {
            s.Summary = "Acknowledge an alert";
            s.Description = "Marks an alert as acknowledged (user has seen it and will take action)";
            s.Params["alertId"] = "Alert ID (GUID)";

            s.ExampleRequest = new AcknowledgeAlertCommand(
                AlertId: Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d")
                );

            s.ResponseExamples[200] = new AcknowledgeAlertResponse(
                Id: Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
                Status: "Acknowledged",
                AcknowledgedAt: DateTimeOffset.UtcNow,
                AcknowledgedBy: Guid.Parse("9823af89-c34e-4da1-b680-79ebd06cc35f"),
                Message: "Alert acknowledged successfully");

            s.Responses[200] = "Returned when the alert is successfully acknowledged.";
            s.Responses[400] = "Returned when the request is invalid or alert cannot be acknowledged (e.g., not pending).";
            s.Responses[404] = "Returned when the alert is not found.";
        });
    }

    public override async Task HandleAsync(AcknowledgeAlertCommand req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct: ct).ConfigureAwait(false);
        await MatchResultAsync(response, ct).ConfigureAwait(false);
    }
}
