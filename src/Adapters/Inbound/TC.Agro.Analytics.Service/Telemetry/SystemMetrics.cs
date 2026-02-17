namespace TC.Agro.Analytics.Service.Telemetry;

/// <summary>
/// System-level metrics for Analytics Worker.
/// Tracks HTTP requests, errors, latencies, database and messaging performance.
/// </summary>
public class SystemMetrics
{
    // HTTP Request metrics
    private readonly Counter<long> _httpRequestsTotal;
    private readonly Counter<long> _httpErrorsTotal;
    private readonly Histogram<double> _httpRequestDuration;

    // Database metrics
    private readonly Histogram<double> _databaseQueryDuration;
    private readonly Counter<long> _databaseErrorsTotal;

    // Cache metrics
    private readonly Counter<long> _cacheHits;
    private readonly Counter<long> _cacheMisses;

    // Connection metrics
    private readonly UpDownCounter<long> _activeConnections;

    // Messaging metrics (specific for Analytics Worker)
    private readonly Counter<long> _messagesReceived;
    private readonly Counter<long> _messagesProcessed;
    private readonly Counter<long> _messagesFailed;
    private readonly Histogram<double> _messageProcessingDuration;

    public SystemMetrics()
    {
        var meter = new Meter(TelemetryConstants.AnalyticsMeterName, TelemetryConstants.Version);

        // HTTP metrics
        _httpRequestsTotal = meter.CreateCounter<long>(
            "analytics.http.requests_total",
            description: "Total number of HTTP requests processed");

        _httpErrorsTotal = meter.CreateCounter<long>(
            "analytics.http.errors_total",
            description: "Total number of HTTP errors (4xx, 5xx)");

        _httpRequestDuration = meter.CreateHistogram<double>(
            "analytics.http.request_duration_seconds",
            unit: "s",
            description: "Duration of HTTP requests in seconds");

        // Database metrics
        _databaseQueryDuration = meter.CreateHistogram<double>(
            "analytics.database.query_duration_seconds",
            unit: "s",
            description: "Duration of database queries in seconds");

        _databaseErrorsTotal = meter.CreateCounter<long>(
            "analytics.database.errors_total",
            description: "Total number of database errors");

        // Cache metrics
        _cacheHits = meter.CreateCounter<long>(
            "analytics.cache.hits_total",
            description: "Total number of cache hits");

        _cacheMisses = meter.CreateCounter<long>(
            "analytics.cache.misses_total",
            description: "Total number of cache misses");

        // Connection metrics
        _activeConnections = meter.CreateUpDownCounter<long>(
            "analytics.connections.active",
            description: "Number of currently active connections");

        // Messaging metrics
        _messagesReceived = meter.CreateCounter<long>(
            "analytics.messages.received_total",
            description: "Total number of messages received from RabbitMQ");

        _messagesProcessed = meter.CreateCounter<long>(
            "analytics.messages.processed_total",
            description: "Total number of messages successfully processed");

        _messagesFailed = meter.CreateCounter<long>(
            "analytics.messages.failed_total",
            description: "Total number of messages that failed processing");

        _messageProcessingDuration = meter.CreateHistogram<double>(
            "analytics.messages.processing_duration_seconds",
            unit: "s",
            description: "Duration of message processing in seconds");
    }

    /// <summary>
    /// Records an HTTP request
    /// </summary>
    public void RecordHttpRequest(string method, string path, int statusCode, double durationSeconds)
    {
        _httpRequestsTotal.Add(1,
            new KeyValuePair<string, object?>("http.method", method),
            new KeyValuePair<string, object?>("http.path", path),
            new KeyValuePair<string, object?>("http.status_code", statusCode.ToString()));

        _httpRequestDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("http.method", method),
            new KeyValuePair<string, object?>("http.status_code", statusCode.ToString()));

        // Track errors separately
        if (statusCode >= 400)
        {
            var errorCategory = statusCode >= 500 ? "server_error" : "client_error";
            _httpErrorsTotal.Add(1,
                new KeyValuePair<string, object?>("http.method", method),
                new KeyValuePair<string, object?>("http.status_code", statusCode.ToString()),
                new KeyValuePair<string, object?>("error.category", errorCategory));
        }
    }

    /// <summary>
    /// Records a database query operation
    /// </summary>
    public void RecordDatabaseQuery(string operation, double durationSeconds, bool isError = false)
    {
        _databaseQueryDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("db.operation", operation));

        if (isError)
        {
            _databaseErrorsTotal.Add(1,
                new KeyValuePair<string, object?>("db.operation", operation));
        }
    }

    /// <summary>
    /// Records a cache hit
    /// </summary>
    public void RecordCacheHit(string cacheKey)
    {
        _cacheHits.Add(1,
            new KeyValuePair<string, object?>("cache.key", cacheKey));
    }

    /// <summary>
    /// Records a cache miss
    /// </summary>
    public void RecordCacheMiss(string cacheKey)
    {
        _cacheMisses.Add(1,
            new KeyValuePair<string, object?>("cache.key", cacheKey));
    }

    /// <summary>
    /// Increments active connection counter (connection opened)
    /// </summary>
    public void ConnectionOpened() => _activeConnections.Add(1);

    /// <summary>
    /// Decrements active connection counter (connection closed)
    /// </summary>
    public void ConnectionClosed() => _activeConnections.Add(-1);

    /// <summary>
    /// Records a message received from RabbitMQ
    /// </summary>
    public void RecordMessageReceived(string messageType, string source)
    {
        _messagesReceived.Add(1,
            new KeyValuePair<string, object?>("message.type", messageType),
            new KeyValuePair<string, object?>("message.source", source));
    }

    /// <summary>
    /// Records a successfully processed message
    /// </summary>
    public void RecordMessageProcessed(string messageType, double durationSeconds)
    {
        _messagesProcessed.Add(1,
            new KeyValuePair<string, object?>("message.type", messageType));

        _messageProcessingDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("message.type", messageType),
            new KeyValuePair<string, object?>("status", "success"));
    }

    /// <summary>
    /// Records a failed message processing
    /// </summary>
    public void RecordMessageFailed(string messageType, string errorType, double durationSeconds)
    {
        _messagesFailed.Add(1,
            new KeyValuePair<string, object?>("message.type", messageType),
            new KeyValuePair<string, object?>("error.type", errorType));

        _messageProcessingDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("message.type", messageType),
            new KeyValuePair<string, object?>("status", "failed"),
            new KeyValuePair<string, object?>("error.type", errorType));
    }
}
