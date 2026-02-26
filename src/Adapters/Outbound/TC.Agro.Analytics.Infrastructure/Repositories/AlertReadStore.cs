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
    private readonly IUserContext _userContext;

    public AlertReadStore(ApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }

    public async Task<PaginatedResponse<PendingAlertResponse>> GetPendingAlertsAsync(
        Guid? ownerId = null,
        string? search = null,
        string? severity = null,
        string? status = null,
        int pageNumber = 1,
        int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Alerts
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt);

        if (string.IsNullOrWhiteSpace(status) || string.Equals(status.Trim(), "pending", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(a => a.Status == AlertStatus.Pending)
                         .OrderByDescending(a => a.CreatedAt);
        }
        else if (!string.Equals(status.Trim(), "all", StringComparison.OrdinalIgnoreCase))
        {
            var statusResult = AlertStatus.Create(status.Trim());
            if (statusResult.IsSuccess)
            {
                var statusValue = statusResult.Value;
                query = query.Where(a => a.Status == statusValue)
                             .OrderByDescending(a => a.CreatedAt);
            }
            else
            {
                query = query.Where(_ => false)
                             .OrderByDescending(a => a.CreatedAt);
            }
        }

        if (_userContext.IsAdmin)
        {
            if (ownerId.HasValue && ownerId.Value != Guid.Empty)
            {
                query = query.Where(a => a.Sensor.OwnerId == ownerId.Value)
                             .OrderByDescending(a => a.CreatedAt);
            }
        }
        else
        {
            query = query.Where(a => a.Sensor.OwnerId == _userContext.Id)
                         .OrderByDescending(a => a.CreatedAt);
        }

        if (!string.IsNullOrWhiteSpace(severity))
        {
            var normalizedSeverity = severity.Trim().ToLowerInvariant();

            if (normalizedSeverity == "warning")
            {
                query = query.Where(a => a.Severity == AlertSeverity.High || a.Severity == AlertSeverity.Medium)
                             .OrderByDescending(a => a.CreatedAt);
            }
            else if (normalizedSeverity == "info")
            {
                query = query.Where(a => a.Severity == AlertSeverity.Low)
                             .OrderByDescending(a => a.CreatedAt);
            }
            else
            {
                var severityResult = AlertSeverity.Create(normalizedSeverity);
                if (severityResult.IsSuccess)
                {
                    var severityValue = severityResult.Value;
                    query = query.Where(a => a.Severity == severityValue)
                                 .OrderByDescending(a => a.CreatedAt);
                }
                else
                {
                    query = query.Where(_ => false)
                                 .OrderByDescending(a => a.CreatedAt);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query
                .Where(a => a.Message.Contains(normalizedSearch) || a.Type.Value.Contains(normalizedSearch))
                .OrderByDescending(a => a.CreatedAt);
        }

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
                a.CreatedAt,
                a.AcknowledgedAt,
                a.AcknowledgedBy))
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<PendingAlertResponse>(
            alerts,
            totalCount,
            pageNumber,
            pageSize);
    }

    public async Task<PendingAlertsSummaryResponse> GetPendingAlertsSummaryAsync(
        Guid? ownerId = null,
        int windowHours = 24,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var windowStart = now.AddHours(-windowHours);

        var query = _dbContext.Alerts
            .AsNoTracking()
            .Where(a => a.Status == AlertStatus.Pending);

        if (_userContext.IsAdmin)
        {
            if (ownerId.HasValue && ownerId.Value != Guid.Empty)
            {
                query = query.Where(a => a.Sensor.OwnerId == ownerId.Value);
            }
        }
        else
        {
            query = query.Where(a => a.Sensor.OwnerId == _userContext.Id);
        }

        var summary = await query
            .GroupBy(_ => 1)
            .Select(group => new
            {
                PendingAlertsTotal = group.Count(),
                AffectedPlotsCount = group.Select(a => a.Sensor.PlotId).Distinct().Count(),
                AffectedSensorsCount = group.Select(a => a.SensorId).Distinct().Count(),
                CriticalPendingCount = group.Count(a => a.Severity == AlertSeverity.Critical),
                HighPendingCount = group.Count(a => a.Severity == AlertSeverity.High),
                MediumPendingCount = group.Count(a => a.Severity == AlertSeverity.Medium),
                LowPendingCount = group.Count(a => a.Severity == AlertSeverity.Low),
                NewPendingInWindowCount = group.Count(a => a.CreatedAt >= windowStart)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (summary is null)
        {
            return new PendingAlertsSummaryResponse(
                PendingAlertsTotal: 0,
                AffectedPlotsCount: 0,
                AffectedSensorsCount: 0,
                CriticalPendingCount: 0,
                HighPendingCount: 0,
                MediumPendingCount: 0,
                LowPendingCount: 0,
                NewPendingInWindowCount: 0,
                WindowHours: windowHours);
        }

        return new PendingAlertsSummaryResponse(
            summary.PendingAlertsTotal,
            summary.AffectedPlotsCount,
            summary.AffectedSensorsCount,
            summary.CriticalPendingCount,
            summary.HighPendingCount,
            summary.MediumPendingCount,
            summary.LowPendingCount,
            summary.NewPendingInWindowCount,
            windowHours);
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
                a.CreatedAt,
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
                mostRecent.CreatedAt)
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

    public async Task<IReadOnlyList<PendingAlertResponse>> GetPendingAlertsBySensorIdsAsync(
        IEnumerable<Guid> sensorIds,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var sensorIdsList = sensorIds.ToList();

        if (!sensorIdsList.Any())
            return Array.Empty<PendingAlertResponse>();

        var alerts = await _dbContext.Alerts
            .AsNoTracking()
            .Where(a => sensorIdsList.Contains(a.SensorId))
            .Where(a => a.Status == AlertStatus.Pending || a.Status == AlertStatus.Acknowledged)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .Select(a => new PendingAlertResponse(
                a.Id,
                a.SensorId,
                a.Type.Value,
                a.Message,
                a.Status.Value,
                a.Severity.Value,
                a.Value,
                a.Threshold,
                a.CreatedAt,
                a.AcknowledgedAt,
                a.AcknowledgedBy))
            .ToListAsync(cancellationToken);

        return alerts;
    }
}
