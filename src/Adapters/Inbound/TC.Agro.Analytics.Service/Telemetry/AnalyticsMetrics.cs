namespace TC.Agro.Analytics.Service.Telemetry;

/// <summary>
/// Analytics-specific metrics for tracking alert operations and sensor data processing.
/// 
/// This class captures business domain metrics specific to analytics operations:
/// - Alert creation, acknowledgment, and resolution
/// - Sensor data processing and threshold evaluation
/// - Alert severity and type distribution
/// - Processing latencies and error rates
/// 
/// Metrics are automatically exported to configured providers (OTLP, Prometheus, etc.)
/// via OpenTelemetry configuration in ServiceCollectionExtensions.
/// </summary>
public class AnalyticsMetrics
{
    private readonly Counter<long> _analyticsActionsCounter;
    private readonly Counter<long> _alertOperationsCounter;
    private readonly Counter<long> _sensorDataProcessedCounter;
    private readonly Counter<long> _alertsCreatedCounter;
    private readonly Counter<long> _alertsByTypeCounter;
    private readonly Counter<long> _alertsBySeverityCounter;
    private readonly Histogram<double> _operationDurationHistogram;
    private readonly Histogram<double> _alertProcessingDuration;
    private readonly Counter<long> _analyticsErrorsCounter;
    private readonly Counter<long> _thresholdEvaluationsCounter;

    public AnalyticsMetrics()
    {
        var meter = new Meter(TelemetryConstants.AnalyticsMeterName, TelemetryConstants.Version);

        // General analytics action counter
        _analyticsActionsCounter = meter.CreateCounter<long>(
            "analytics_actions_total",
            description: "Total number of analytics-related actions performed");

        // Domain-specific counters
        _alertOperationsCounter = meter.CreateCounter<long>(
            "alert_operations_total",
            description: "Total number of alert operations (create, acknowledge, resolve)");

        _sensorDataProcessedCounter = meter.CreateCounter<long>(
            "sensor_data_processed_total",
            description: "Total number of sensor data events processed");

        _alertsCreatedCounter = meter.CreateCounter<long>(
            "alerts_created_total",
            description: "Total number of alerts created");

        _alertsByTypeCounter = meter.CreateCounter<long>(
            "alerts_by_type_total",
            description: "Total alerts grouped by type (HighTemperature, LowSoilMoisture, etc.)");

        _alertsBySeverityCounter = meter.CreateCounter<long>(
            "alerts_by_severity_total",
            description: "Total alerts grouped by severity (Low, Medium, High, Critical)");

        _thresholdEvaluationsCounter = meter.CreateCounter<long>(
            "threshold_evaluations_total",
            description: "Total number of threshold evaluations performed");

        // Performance metrics
        _operationDurationHistogram = meter.CreateHistogram<double>(
            "analytics_operation_duration_seconds",
            description: "Duration of analytics operations in seconds");

        _alertProcessingDuration = meter.CreateHistogram<double>(
            "alert_processing_duration_seconds",
            description: "Duration of alert processing from sensor data in seconds");

        // Error tracking
        _analyticsErrorsCounter = meter.CreateCounter<long>(
            "analytics_errors_total",
            description: "Total number of errors in analytics operations");
    }

    /// <summary>
    /// Records a general analytics action performed.
    /// </summary>
    public void RecordAnalyticsAction(string action, string userId, string endpoint)
    {
        _analyticsActionsCounter.Add(1,
            new KeyValuePair<string, object?>("action", action.ToLowerInvariant()),
            new KeyValuePair<string, object?>("user_id", userId),
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("service", "analytics"));
    }

    /// <summary>
    /// Records an alert-related operation (create, acknowledge, resolve).
    /// </summary>
    public void RecordAlertOperation(string operation, string userId, string? alertId = null, string? alertType = null)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("operation", operation.ToLowerInvariant()),
            new("user_id", userId),
            new("entity_type", "alert")
        };

        if (!string.IsNullOrWhiteSpace(alertId))
        {
            tags.Add(new("alert_id", alertId));
        }

        if (!string.IsNullOrWhiteSpace(alertType))
        {
            tags.Add(new("alert_type", alertType));
        }

        _alertOperationsCounter.Add(1, tags.ToArray());
    }

    /// <summary>
    /// Records sensor data processing event.
    /// </summary>
    public void RecordSensorDataProcessed(string sensorId, string plotId, bool alertsTriggered)
    {
        _sensorDataProcessedCounter.Add(1,
            new KeyValuePair<string, object?>("sensor_id", sensorId),
            new KeyValuePair<string, object?>("plot_id", plotId),
            new KeyValuePair<string, object?>("alerts_triggered", alertsTriggered));
    }

    /// <summary>
    /// Records alert creation with type and severity.
    /// </summary>
    public void RecordAlertCreated(string alertType, string severity, string sensorId, string plotId)
    {
        _alertsCreatedCounter.Add(1,
            new KeyValuePair<string, object?>("alert_type", alertType),
            new KeyValuePair<string, object?>("severity", severity),
            new KeyValuePair<string, object?>("sensor_id", sensorId),
            new KeyValuePair<string, object?>("plot_id", plotId));

        // Track by type
        _alertsByTypeCounter.Add(1,
            new KeyValuePair<string, object?>("alert_type", alertType));

        // Track by severity
        _alertsBySeverityCounter.Add(1,
            new KeyValuePair<string, object?>("severity", severity));
    }

    /// <summary>
    /// Records threshold evaluation (e.g., temperature > max, soil moisture < min).
    /// </summary>
    public void RecordThresholdEvaluation(string thresholdType, bool exceeded, string sensorId)
    {
        _thresholdEvaluationsCounter.Add(1,
            new KeyValuePair<string, object?>("threshold_type", thresholdType),
            new KeyValuePair<string, object?>("exceeded", exceeded),
            new KeyValuePair<string, object?>("sensor_id", sensorId));
    }

    /// <summary>
    /// Records the duration of an analytics operation.
    /// </summary>
    public void RecordOperationDuration(string operation, string entityType, double durationSeconds, bool success = true)
    {
        _operationDurationHistogram.Record(durationSeconds,
            new KeyValuePair<string, object?>("operation", operation.ToLowerInvariant()),
            new KeyValuePair<string, object?>("entity_type", entityType),
            new KeyValuePair<string, object?>("success", success),
            new KeyValuePair<string, object?>("service", "analytics"));
    }

    /// <summary>
    /// Records alert processing duration (from sensor data ingestion to alert creation).
    /// </summary>
    public void RecordAlertProcessingDuration(string alertType, double durationSeconds, int alertsCreated)
    {
        _alertProcessingDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("alert_type", alertType),
            new KeyValuePair<string, object?>("alerts_created", alertsCreated));
    }

    /// <summary>
    /// Records an analytics error with context.
    /// </summary>
    public void RecordError(string operation, string errorType, string? entityId = null)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("operation", operation.ToLowerInvariant()),
            new("error_type", errorType),
            new("service", "analytics")
        };

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            tags.Add(new("entity_id", entityId));
        }

        _analyticsErrorsCounter.Add(1, tags.ToArray());
    }
}
