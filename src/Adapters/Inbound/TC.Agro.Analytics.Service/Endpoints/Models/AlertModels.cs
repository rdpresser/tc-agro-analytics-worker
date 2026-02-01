namespace TC.Agro.Analytics.Service.Endpoints.Models
{
    /// <summary>
    /// DTO for Alert read model
    /// </summary>
    public record AlertDto
    {
        public Guid Id { get; init; }
        public Guid SensorReadingId { get; init; }
        public string SensorId { get; init; } = default!;
        public Guid PlotId { get; init; }
        public string AlertType { get; init; } = default!;
        public string Message { get; init; } = default!;
        public string Status { get; init; } = default!;
        public string Severity { get; init; } = default!;
        public double? Value { get; init; }
        public double? Threshold { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? AcknowledgedAt { get; init; }
        public string? AcknowledgedBy { get; init; }
        public DateTime? ResolvedAt { get; init; }
        public string? ResolvedBy { get; init; }
        public string? ResolutionNotes { get; init; }
    }

    /// <summary>
    /// Request model for alert history queries
    /// </summary>
    public record GetAlertHistoryRequest
    {
        public Guid PlotId { get; init; }
        public int Days { get; init; } = 30; // Default: last 30 days
        public string? AlertType { get; init; } // Optional filter by type
        public string? Status { get; init; } // Optional filter by status
    }

    /// <summary>
    /// Plot status summary with aggregated alert information
    /// </summary>
    public record PlotStatusDto
    {
        public Guid PlotId { get; init; }
        public int PendingAlertsCount { get; init; }
        public int TotalAlertsLast24Hours { get; init; }
        public int TotalAlertsLast7Days { get; init; }
        public AlertDto? MostRecentAlert { get; init; }
        public Dictionary<string, int> AlertsByType { get; init; } = new();
        public Dictionary<string, int> AlertsBySeverity { get; init; } = new();
        public string OverallStatus { get; init; } = "OK"; // OK, Warning, Critical
    }

    /// <summary>
    /// Paginated response for alert lists
    /// </summary>
    public record AlertListResponse
    {
        public List<AlertDto> Alerts { get; init; } = new();
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public bool HasNextPage => PageNumber * PageSize < TotalCount;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
