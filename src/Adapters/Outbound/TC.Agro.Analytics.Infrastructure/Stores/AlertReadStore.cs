using Microsoft.EntityFrameworkCore;
using TC.Agro.Analytics.Application.Abstractions.Ports;
using TC.Agro.Analytics.Domain.Abstractions.Constants;
using TC.Agro.Analytics.Domain.Entities;

namespace TC.Agro.Analytics.Infrastructure.Stores;

/// <summary>
/// Read-only store implementation for Alert queries.
/// Uses EF Core with AsNoTracking() for optimized read performance.
/// </summary>
public sealed class AlertReadStore : IAlertReadStore
{
    private readonly ApplicationDbContext _dbContext;

    public AlertReadStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<List<Alert>> GetPendingAlertsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Alerts
            .AsNoTracking()
            .Where(a => a.Status == AlertStatus.Pending)
            .OrderByDescending(a => a.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Alert>> GetAlertHistoryAsync(
        Guid plotId,
        int days = 30,
        string? alertType = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var query = _dbContext.Alerts
            .AsNoTracking()
            .Where(a => a.PlotId == plotId)
            .Where(a => a.CreatedAt >= cutoffDate);

        if (!string.IsNullOrEmpty(alertType))
        {
            query = query.Where(a => a.AlertType == alertType);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status == status);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(500)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlotStatusResult> GetPlotStatusAsync(
        Guid plotId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var last24Hours = now.AddHours(-24);
        var last7Days = now.AddDays(-7);

        var alerts = await _dbContext.Alerts
            .AsNoTracking()
            .Where(a => a.PlotId == plotId)
            .Where(a => a.CreatedAt >= last7Days)
            .ToListAsync(cancellationToken);

        var pendingCount = alerts.Count(a => a.Status == AlertStatus.Pending);
        var last24HoursCount = alerts.Count(a => a.CreatedAt >= last24Hours);
        
        var alertsByType = alerts
            .GroupBy(a => a.AlertType)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var alertsBySeverity = alerts
            .GroupBy(a => a.Severity)
            .ToDictionary(g => g.Key, g => g.Count());

        var mostRecentAlert = alerts
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();

        var overallStatus = DetermineOverallStatus(pendingCount, alertsBySeverity);

        return new PlotStatusResult
        {
            PlotId = plotId,
            PendingAlertsCount = pendingCount,
            TotalAlertsLast24Hours = last24HoursCount,
            TotalAlertsLast7Days = alerts.Count,
            MostRecentAlert = mostRecentAlert,
            AlertsByType = alertsByType,
            AlertsBySeverity = alertsBySeverity,
            OverallStatus = overallStatus
        };
    }

    private static string DetermineOverallStatus(int pendingCount, Dictionary<string, int> alertsBySeverity)
    {
        if (alertsBySeverity.TryGetValue(AlertSeverity.Critical, out var criticalCount) && criticalCount > 0)
            return "Critical";

        if (alertsBySeverity.TryGetValue(AlertSeverity.High, out var highCount) && highCount > 0)
            return "Warning";

        if (pendingCount > 0)
            return "Warning";

        return "OK";
    }
}
