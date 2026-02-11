namespace TC.Agro.Analytics.Infrastructure.Repositores;

/// <summary>
/// Read-only store implementation for Alert queries.
/// Following Identity Service pattern: returns Response DTOs directly (no mappers needed).
/// Uses PaginatedResponse from SharedKernel for consistency.
/// Uses EF Core with AsNoTracking() for optimized read performance.
/// </summary>
public sealed class AlertReadStore : IAlertReadStore
{
    private readonly ApplicationDbContext _dbContext;

    public AlertReadStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<PaginatedResponse<GetPendingAlerts.PendingAlertResponse>> GetPendingAlertsAsync(
        int pageNumber = 1,
        int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Alerts
            .AsNoTracking()
            .Where(a => a.Status == AlertStatus.Pending)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var alerts = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new GetPendingAlerts.PendingAlertResponse(
                a.Id,
                a.SensorReadingId,
                a.SensorId,
                a.PlotId,
                a.AlertType,
                a.Message,
                a.Status,
                a.Severity,
                a.Value,
                a.Threshold,
                a.CreatedAt,
                a.AcknowledgedAt,
                a.AcknowledgedBy))
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<GetPendingAlerts.PendingAlertResponse>(
            alerts,
            totalCount,
            pageNumber,
            pageSize);
    }

    public async Task<PaginatedResponse<GetAlertHistory.AlertHistoryResponse>> GetAlertHistoryAsync(
        Guid plotId,
        int days = 30,
        string? alertType = null,
        string? status = null,
        int pageNumber = 1,
        int pageSize = 100,
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

        query = query.OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var alerts = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new GetAlertHistory.AlertHistoryResponse(
                a.Id,
                a.SensorReadingId,
                a.SensorId,
                a.PlotId,
                a.AlertType,
                a.Message,
                a.Status,
                a.Severity,
                a.Value,
                a.Threshold,
                a.CreatedAt,
                a.AcknowledgedAt,
                a.AcknowledgedBy,
                a.ResolvedAt,
                a.ResolvedBy,
                a.ResolutionNotes))
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<GetAlertHistory.AlertHistoryResponse>(
            alerts,
            totalCount,
            pageNumber,
            pageSize);
    }

    public async Task<GetPlotStatus.GetPlotStatusResponse> GetPlotStatusAsync(
        Guid plotId,
        CancellationToken cancellationToken = default)
    {
        var last7Days = DateTime.UtcNow.AddDays(-7);
        var last24Hours = DateTime.UtcNow.AddDays(-1);

        var allAlerts = await _dbContext.Alerts
            .AsNoTracking()
            .Where(a => a.PlotId == plotId && a.CreatedAt >= last7Days)
            .ToListAsync(cancellationToken);

        var pendingCount = allAlerts.Count(a => a.Status == AlertStatus.Pending);
        var last24HoursCount = allAlerts.Count(a => a.CreatedAt >= last24Hours);

        var alertsByType = allAlerts
            .GroupBy(a => a.AlertType)
            .ToDictionary(g => g.Key, g => g.Count());

        var alertsBySeverity = allAlerts
            .GroupBy(a => a.Severity)
            .ToDictionary(g => g.Key, g => g.Count());

        var mostRecent = allAlerts
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();

        var mostRecentAlert = mostRecent != null
            ? new GetPlotStatus.PlotStatusAlertResponse(
                mostRecent.Id,
                mostRecent.SensorReadingId,
                mostRecent.SensorId,
                mostRecent.AlertType,
                mostRecent.Message,
                mostRecent.Status,
                mostRecent.Severity,
                mostRecent.Value,
                mostRecent.Threshold,
                mostRecent.CreatedAt)
            : null;

        var overallStatus = pendingCount switch
        {
            0 => "OK",
            <= 5 => "Warning",
            _ => "Critical"
        };

        return new GetPlotStatus.GetPlotStatusResponse(
            plotId,
            pendingCount,
            last24HoursCount,
            allAlerts.Count,
            mostRecentAlert,
            alertsByType,
            alertsBySeverity,
            overallStatus);
    }
}
