using TC.Agro.Analytics.Application.UseCases.Shared;
using TC.Agro.Analytics.Domain.Entities;

namespace TC.Agro.Analytics.Application.UseCases.GetAlertHistory;

/// <summary>
/// Mapper for GetAlertHistory use case
/// </summary>
internal static class GetAlertHistoryMapper
{
    public static AlertResponse ToResponse(Alert alert)
    {
        return new AlertResponse
        {
            Id = alert.Id,
            SensorReadingId = alert.SensorReadingId,
            SensorId = alert.SensorId,
            PlotId = alert.PlotId,
            AlertType = alert.AlertType,
            Message = alert.Message,
            Status = alert.Status,
            Severity = alert.Severity,
            Value = alert.Value,
            Threshold = alert.Threshold,
            CreatedAt = alert.CreatedAt,
            AcknowledgedAt = alert.AcknowledgedAt,
            AcknowledgedBy = alert.AcknowledgedBy,
            ResolvedAt = alert.ResolvedAt,
            ResolvedBy = alert.ResolvedBy,
            ResolutionNotes = alert.ResolutionNotes
        };
    }

    public static AlertListResponse ToListResponse(
        IEnumerable<Alert> alerts,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        return new AlertListResponse
        {
            Alerts = alerts.Select(ToResponse).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
