using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Application.UseCases.Shared;
using TC.Agro.Analytics.Domain.Entities;

namespace TC.Agro.Analytics.Application.UseCases.GetPlotStatus;

/// <summary>
/// Mapper for GetPlotStatus use case
/// </summary>
internal static class GetPlotStatusMapper
{
    public static AlertResponse? ToAlertResponse(Alert? alert)
    {
        if (alert == null)
            return null;

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

    public static PlotStatusResponse ToResponse(PlotStatusResult plotStatus)
    {
        return new PlotStatusResponse
        {
            PlotId = plotStatus.PlotId,
            PendingAlertsCount = plotStatus.PendingAlertsCount,
            TotalAlertsLast24Hours = plotStatus.TotalAlertsLast24Hours,
            TotalAlertsLast7Days = plotStatus.TotalAlertsLast7Days,
            MostRecentAlert = ToAlertResponse(plotStatus.MostRecentAlert),
            AlertsByType = plotStatus.AlertsByType,
            AlertsBySeverity = plotStatus.AlertsBySeverity,
            OverallStatus = plotStatus.OverallStatus
        };
    }
}
