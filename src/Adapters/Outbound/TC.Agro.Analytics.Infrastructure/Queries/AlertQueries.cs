using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TC.Agro.Analytics.Domain.Entities;

namespace TC.Agro.Analytics.Infrastructure.Queries
{
    /// <summary>
    /// Query handler for retrieving pending alerts
    /// CQRS Query Side - optimized for reads
    /// </summary>
    public class GetPendingAlertsQueryHandler
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<GetPendingAlertsQueryHandler> _logger;

        public GetPendingAlertsQueryHandler(
            ApplicationDbContext dbContext,
            ILogger<GetPendingAlertsQueryHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Alert>> Handle(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Querying pending alerts");

            var alerts = await _dbContext.Alerts
                .AsNoTracking()
                .Where(a => a.Status == AlertStatus.Pending)
                .OrderByDescending(a => a.CreatedAt)
                .Take(100)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} pending alerts", alerts.Count);

            return alerts;
        }
    }

    /// <summary>
    /// Query handler for retrieving alert history by plot
    /// </summary>
    public class GetAlertHistoryQueryHandler
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<GetAlertHistoryQueryHandler> _logger;

        public GetAlertHistoryQueryHandler(
            ApplicationDbContext dbContext,
            ILogger<GetAlertHistoryQueryHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Alert>> Handle(
            Guid plotId,
            int days = 30,
            string? alertType = null,
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Querying alert history for Plot {PlotId}, Days: {Days}, Type: {AlertType}, Status: {Status}",
                plotId,
                days,
                alertType ?? "All",
                status ?? "All");

            var query = _dbContext.Alerts
                .AsNoTracking()
                .Where(a => a.PlotId == plotId)
                .Where(a => a.CreatedAt >= DateTime.UtcNow.AddDays(-days));

            if (!string.IsNullOrEmpty(alertType))
            {
                query = query.Where(a => a.AlertType == alertType);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            var alerts = await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(500)
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} alerts for Plot {PlotId}",
                alerts.Count,
                plotId);

            return alerts;
        }
    }

    /// <summary>
    /// Query handler for retrieving plot status summary
    /// </summary>
    public class GetPlotStatusQueryHandler
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<GetPlotStatusQueryHandler> _logger;

        public GetPlotStatusQueryHandler(
            ApplicationDbContext dbContext,
            ILogger<GetPlotStatusQueryHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PlotStatus> Handle(Guid plotId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Querying status for Plot {PlotId}", plotId);

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

            _logger.LogInformation(
                "Plot {PlotId} status: {Status}, Pending: {Pending}, Last24h: {Last24h}",
                plotId,
                overallStatus,
                pendingCount,
                last24HoursCount);

            return new PlotStatus
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

    /// <summary>
    /// DTO for plot status response
    /// </summary>
    public record PlotStatus
    {
        public Guid PlotId { get; init; }
        public int PendingAlertsCount { get; init; }
        public int TotalAlertsLast24Hours { get; init; }
        public int TotalAlertsLast7Days { get; init; }
        public Alert? MostRecentAlert { get; init; }
        public Dictionary<string, int> AlertsByType { get; init; } = new();
        public Dictionary<string, int> AlertsBySeverity { get; init; } = new();
        public string OverallStatus { get; init; } = "OK";
    }
}
