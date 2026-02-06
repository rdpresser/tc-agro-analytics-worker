namespace TC.Agro.Analytics.Application.UseCases.Shared;

/// <summary>
/// Alert response model for read operations
/// </summary>
public record AlertResponse
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
/// Plot status summary response with aggregated alert information
/// </summary>
public record PlotStatusResponse
{
    public Guid PlotId { get; init; }
    public int PendingAlertsCount { get; init; }
    public int TotalAlertsLast24Hours { get; init; }
    public int TotalAlertsLast7Days { get; init; }
    public AlertResponse? MostRecentAlert { get; init; }
    public Dictionary<string, int> AlertsByType { get; init; } = new();
    public Dictionary<string, int> AlertsBySeverity { get; init; } = new();
    public string OverallStatus { get; init; } = "OK";
}

/// <summary>
/// Paginated response for alert lists
/// </summary>
public record AlertListResponse
{
    public List<AlertResponse> Alerts { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public bool HasNextPage => PageNumber * PageSize < TotalCount;
    public bool HasPreviousPage => PageNumber > 1;
}
