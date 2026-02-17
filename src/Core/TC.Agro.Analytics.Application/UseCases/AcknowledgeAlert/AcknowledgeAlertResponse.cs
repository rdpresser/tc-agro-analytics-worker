namespace TC.Agro.Analytics.Application.UseCases.AcknowledgeAlert;

/// <summary>
/// Response for the AcknowledgeAlert command.
/// Contains audit trail information.
/// </summary>
public sealed record AcknowledgeAlertResponse(
    Guid Id,
    string Status,
    DateTimeOffset AcknowledgedAt,
    string AcknowledgedBy,
    string Message = "Alert acknowledged successfully");
