using FastEndpoints;
using TC.Agro.Analytics.Infrastructure.Queries;
using TC.Agro.Analytics.Service.Endpoints.Models;

namespace TC.Agro.Analytics.Service.Endpoints.Alerts
{
    /// <summary>
    /// Endpoint to retrieve all pending alerts across all plots
    /// GET /alerts/pending
    /// </summary>
    public class GetPendingAlertsEndpoint : EndpointWithoutRequest<AlertListResponse>
    {
        private readonly GetPendingAlertsQueryHandler _queryHandler;

        public GetPendingAlertsEndpoint(GetPendingAlertsQueryHandler queryHandler)
        {
            _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        }

        public override void Configure()
        {
            Get("/alerts/pending");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get all pending alerts";
                s.Description = "Retrieves all alerts with status 'Pending' across all plots, ordered by creation date (most recent first)";
                s.Response<AlertListResponse>(200, "List of pending alerts");
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var alerts = await _queryHandler.Handle(ct);

            Response = new AlertListResponse
            {
                Alerts = alerts.Select(MapToDto).ToList(),
                TotalCount = alerts.Count,
                PageNumber = 1,
                PageSize = alerts.Count
            };
        }

        private static AlertDto MapToDto(Domain.Entities.Alert alert) => new()
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

    /// <summary>
    /// Endpoint to retrieve alert history for a specific plot
    /// GET /alerts/history/{plotId}?days=30&alertType=HighTemperature&status=Pending
    /// </summary>
    public class GetAlertHistoryEndpoint : Endpoint<GetAlertHistoryRequest, AlertListResponse>
    {
        private readonly GetAlertHistoryQueryHandler _queryHandler;

        public GetAlertHistoryEndpoint(GetAlertHistoryQueryHandler queryHandler)
        {
            _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        }

        public override void Configure()
        {
            Get("/alerts/history/{plotId}");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get alert history for a plot";
                s.Description = "Retrieves historical alerts for a specific plot with optional filters";
                s.Params["plotId"] = "Plot ID (GUID)";
                s.Params["days"] = "Number of days to look back (default: 30)";
                s.Params["alertType"] = "Filter by alert type (optional): HighTemperature, LowSoilMoisture, LowBattery";
                s.Params["status"] = "Filter by status (optional): Pending, Acknowledged, Resolved";
                s.Response<AlertListResponse>(200, "List of alerts");
                s.Response(404, "Plot not found");
            });
        }

        public override async Task HandleAsync(GetAlertHistoryRequest req, CancellationToken ct)
        {
            var alerts = await _queryHandler.Handle(
                req.PlotId,
                req.Days,
                req.AlertType,
                req.Status,
                ct);

            Response = new AlertListResponse
            {
                Alerts = alerts.Select(MapToDto).ToList(),
                TotalCount = alerts.Count,
                PageNumber = 1,
                PageSize = alerts.Count
            };
        }

        private static AlertDto MapToDto(Domain.Entities.Alert alert) => new()
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

    /// <summary>
    /// Endpoint to get plot status summary with aggregated metrics
    /// GET /plots/{plotId}/status
    /// </summary>
    public class GetPlotStatusEndpoint : Endpoint<PlotIdRequest, PlotStatusDto>
    {
        private readonly GetPlotStatusQueryHandler _queryHandler;

        public GetPlotStatusEndpoint(GetPlotStatusQueryHandler queryHandler)
        {
            _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        }

        public override void Configure()
        {
            Get("/plots/{plotId}/status");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get plot status summary";
                s.Description = "Retrieves aggregated alert metrics for a plot (last 7 days)";
                s.Params["plotId"] = "Plot ID (GUID)";
                s.Response<PlotStatusDto>(200, "Plot status summary");
                s.Response(404, "Plot not found");
            });
        }

        public override async Task HandleAsync(PlotIdRequest req, CancellationToken ct)
        {
            var plotStatus = await _queryHandler.Handle(req.PlotId, ct);

            Response = new PlotStatusDto
            {
                PlotId = plotStatus.PlotId,
                PendingAlertsCount = plotStatus.PendingAlertsCount,
                TotalAlertsLast24Hours = plotStatus.TotalAlertsLast24Hours,
                TotalAlertsLast7Days = plotStatus.TotalAlertsLast7Days,
                MostRecentAlert = plotStatus.MostRecentAlert != null ? MapAlertToDto(plotStatus.MostRecentAlert) : null,
                AlertsByType = plotStatus.AlertsByType,
                AlertsBySeverity = plotStatus.AlertsBySeverity,
                OverallStatus = plotStatus.OverallStatus
            };
        }

        private static AlertDto MapAlertToDto(Domain.Entities.Alert alert) => new()
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

    /// <summary>
    /// Request model for plot ID parameter
    /// </summary>
    public record PlotIdRequest
    {
        public Guid PlotId { get; init; }
    }
}
