namespace TC.Agro.Analytics.Infrastructure.Repositories;

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

    public async Task<PaginatedResponse<PendingAlertResponse>> GetPendingAlertsAsync(
        int pageNumber = 1,
        int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Alerts
            .AsNoTracking()
            .Where(a => a.Status == AlertStatus.Pending || a.Status == AlertStatus.Acknowledged)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var alerts = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new PendingAlertResponse(
                a.Id,
                a.SensorId,
                a.Type.Value,
                a.Message,
                a.Status.Value,
                a.Severity.Value,
                a.Value,
                a.Threshold,
                a.CreatedAt.DateTime,
                a.AcknowledgedAt,
                a.AcknowledgedBy))
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<PendingAlertResponse>(
            alerts,
            totalCount,
            pageNumber,
            pageSize);
    }

    public async Task<PaginatedResponse<AlertHistoryResponse>> GetAlertHistoryAsync(
        GetAlertHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-query.Days);

        var alertsQuery = _dbContext.Alerts
            .AsNoTracking()
            .Where(a => a.SensorId == query.SensorId)
            .Where(a => a.CreatedAt >= cutoffDate);

        if (!string.IsNullOrEmpty(query.AlertType))
        {
            var typeResult = AlertType.Create(query.AlertType.Trim());
            if (typeResult.IsSuccess)
            {
                var alertTypeVO = typeResult.Value;
                alertsQuery = alertsQuery.Where(a => a.Type == alertTypeVO);
            }
        }

        if (!string.IsNullOrEmpty(query.Status))
        {
            var statusResult = AlertStatus.Create(query.Status.Trim());
            if (statusResult.IsSuccess)
            {
                var statusVO = statusResult.Value;
                alertsQuery = alertsQuery.Where(a => a.Status == statusVO);
            }
        }

        alertsQuery = alertsQuery.OrderByDescending(a => a.CreatedAt);

        var totalCount = await alertsQuery.CountAsync(cancellationToken);

        var alerts = await alertsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AlertHistoryResponse(
                a.Id,
                a.SensorId,
                a.Type.Value,
                a.Message,
                a.Status.Value,
                a.Severity.Value,
                a.Value,
                a.Threshold,
                a.CreatedAt.DateTime,
                a.AcknowledgedAt,
                a.AcknowledgedBy,
                a.ResolvedAt,
                a.ResolvedBy,
                a.ResolutionNotes))
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<AlertHistoryResponse>(
            alerts,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }

    public async Task<GetSensorStatusResponse> GetSensorStatusAsync(
        Guid sensorId,
        CancellationToken cancellationToken = default)
    {
        var last7Days = DateTimeOffset.UtcNow.AddDays(-7);
        var last24Hours = DateTimeOffset.UtcNow.AddDays(-1);

        var allAlerts = await _dbContext.Alerts
            .AsNoTracking()
            .Where(a => a.SensorId == sensorId && a.CreatedAt >= last7Days)
            .ToListAsync(cancellationToken);

        var pendingCount = allAlerts.Count(a => a.Status == AlertStatus.Pending);
        var last24HoursCount = allAlerts.Count(a => a.CreatedAt >= last24Hours);

        var alertsByType = allAlerts
            .GroupBy(a => a.Type.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var alertsBySeverity = allAlerts
            .GroupBy(a => a.Severity.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var mostRecent = allAlerts
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();

        var mostRecentAlert = mostRecent != null
            ? new SensorStatusAlertResponse(
                mostRecent.Id,
                mostRecent.SensorId,
                mostRecent.Type.Value,
                mostRecent.Message,
                mostRecent.Status.Value,
                mostRecent.Severity.Value,
                mostRecent.Value,
                mostRecent.Threshold,
                mostRecent.CreatedAt.DateTime)
            : null;

        var overallStatus = pendingCount switch
        {
            0 => "OK",
            <= 5 => "Warning",
            _ => "Critical"
        };

        return new GetSensorStatusResponse(
            sensorId,
            pendingCount,
            last24HoursCount,
            allAlerts.Count,
            mostRecentAlert,
            alertsByType,
            alertsBySeverity,
            overallStatus);
    }
}
