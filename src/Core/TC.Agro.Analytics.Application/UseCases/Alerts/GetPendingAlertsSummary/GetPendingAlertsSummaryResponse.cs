namespace TC.Agro.Analytics.Application.UseCases.Alerts.GetPendingAlertsSummary;

/// <summary>
/// Aggregated summary for pending alerts, optimized for dashboard KPIs.
/// </summary>
public sealed record PendingAlertsSummaryResponse(
    int PendingAlertsTotal,
    int AffectedPlotsCount,
    int AffectedSensorsCount,
    int CriticalPendingCount,
    int HighPendingCount,
    int MediumPendingCount,
    int LowPendingCount,
    int NewPendingInWindowCount,
    int WindowHours);
