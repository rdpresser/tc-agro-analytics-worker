namespace TC.Agro.Analytics.Service.Telemetry;

/// <summary>
/// Factory for creating ActivitySource instances for different domains.
/// These activity sources are used to emit traces in handlers and business logic.
/// </summary>
internal static class ActivitySourceFactory
{
    /// <summary>
    /// Activity source for handlers (CQRS Commands/Queries and Message Handlers)
    /// Usage: StartActivity for processing commands/queries/messages
    /// </summary>
    public static readonly ActivitySource Handlers =
        new(TelemetryConstants.HandlersActivitySource, TelemetryConstants.Version);

    /// <summary>
    /// Activity source for FastEndpoints operations
    /// Usage: StartActivity for FastEndpoint pipeline events
    /// </summary>
    public static readonly ActivitySource FastEndpoints =
        new(TelemetryConstants.FastEndpointsActivitySource, TelemetryConstants.Version);

    /// <summary>
    /// Activity source for database operations (beyond ORM instrumentation)
    /// Usage: custom spans for complex database operations
    /// </summary>
    public static readonly ActivitySource Database =
        new(TelemetryConstants.DatabaseActivitySource, TelemetryConstants.Version);

    /// <summary>
    /// Activity source for cache operations (beyond automatic instrumentation)
    /// Usage: custom spans for cache strategies or key-value patterns
    /// </summary>
    public static readonly ActivitySource Cache =
        new(TelemetryConstants.CacheActivitySource, TelemetryConstants.Version);

    /// <summary>
    /// Activity source for messaging operations (RabbitMQ/Wolverine)
    /// Usage: custom spans for message processing, publishing, and routing
    /// </summary>
    public static readonly ActivitySource Messaging =
        new(TelemetryConstants.MessagingActivitySource, TelemetryConstants.Version);

    // ============================================================
    // Helper Methods for Common Patterns
    // ============================================================

    /// <summary>
    /// Start a handler operation span (Command/Query processing)
    /// </summary>
    /// <param name="operationName">Handler name (e.g., "ProcessSensorAlertsCommand")</param>
    /// <param name="userId">User ID performing the operation</param>
    public static Activity? StartHandlerOperation(string operationName, string userId = TelemetryConstants.SystemUser)
    {
        var activity = Handlers.StartActivity(operationName);
        activity?.SetTag("handler.name", operationName);
        activity?.SetTag("user.id", userId);
        return activity;
    }

    /// <summary>
    /// Start a database operation span (beyond automatic instrumentation)
    /// </summary>
    /// <param name="operation">Operation name (e.g., "GetAlertById")</param>
    /// <param name="tableName">Table name involved</param>
    public static Activity? StartDatabaseOperation(string operation, string tableName)
    {
        var activity = Database.StartActivity(operation);
        activity?.SetTag("db.operation", operation);
        activity?.SetTag("db.table", tableName);
        return activity;
    }

    /// <summary>
    /// Start a cache operation span
    /// </summary>
    /// <param name="operation">Operation (get, set, delete)</param>
    /// <param name="cacheKey">Cache key being accessed</param>
    public static Activity? StartCacheOperation(string operation, string cacheKey)
    {
        var activity = Cache.StartActivity(operation);
        activity?.SetTag("cache.operation", operation);
        activity?.SetTag("cache.key", cacheKey);
        return activity;
    }

    /// <summary>
    /// Start a messaging operation span (message publishing or consumption)
    /// </summary>
    /// <param name="operation">Operation name (e.g., "PublishAlert", "ConsumeSensorEvent")</param>
    /// <param name="messageType">Message type being processed</param>
    /// <param name="source">Message source (queue/exchange name)</param>
    public static Activity? StartMessagingOperation(string operation, string messageType, string? source = null)
    {
        var activity = Messaging.StartActivity(operation);
        activity?.SetTag("messaging.operation", operation);
        activity?.SetTag("messaging.message_type", messageType);

        if (!string.IsNullOrWhiteSpace(source))
        {
            activity?.SetTag("messaging.source", source);
        }

        return activity;
    }

    /// <summary>
    /// Start an analytics-specific operation span
    /// </summary>
    /// <param name="operationName">Operation name (e.g., "CreateAlert", "AcknowledgeAlert", "ProcessSensorData")</param>
    /// <param name="entityType">Entity type (alert, sensor_data, threshold)</param>
    /// <param name="entityId">Entity ID if available</param>
    /// <param name="userId">User ID performing the operation</param>
    public static Activity? StartAnalyticsOperation(
        string operationName, 
        string entityType, 
        string? entityId = null, 
        string userId = TelemetryConstants.SystemUser)
    {
        var activity = Handlers.StartActivity(operationName);
        activity?.SetTag("analytics.operation", operationName);
        activity?.SetTag("analytics.entity_type", entityType);
        activity?.SetTag("user.id", userId);

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            activity?.SetTag("analytics.entity_id", entityId);
        }

        return activity;
    }

    /// <summary>
    /// Start an alert operation span with alert-specific context
    /// </summary>
    /// <param name="operation">Operation (create, acknowledge, resolve)</param>
    /// <param name="alertId">Alert ID</param>
    /// <param name="alertType">Alert type (HighTemperature, LowSoilMoisture, etc.)</param>
    /// <param name="severity">Alert severity (Low, Medium, High, Critical)</param>
    /// <param name="userId">User ID performing the operation</param>
    public static Activity? StartAlertOperation(
        string operation,
        string alertId,
        string? alertType = null,
        string? severity = null,
        string userId = TelemetryConstants.SystemUser)
    {
        var activity = Handlers.StartActivity($"Alert.{operation}");
        activity?.SetTag("alert.operation", operation);
        activity?.SetTag("alert.id", alertId);
        activity?.SetTag("user.id", userId);

        if (!string.IsNullOrWhiteSpace(alertType))
        {
            activity?.SetTag("alert.type", alertType);
        }

        if (!string.IsNullOrWhiteSpace(severity))
        {
            activity?.SetTag("alert.severity", severity);
        }

        return activity;
    }

    /// <summary>
    /// Start a sensor data processing span
    /// </summary>
    /// <param name="sensorId">Sensor ID</param>
    /// <param name="plotId">Plot ID</param>
    /// <param name="eventId">Event ID</param>
    public static Activity? StartSensorProcessing(string sensorId, string plotId, string? eventId = null)
    {
        var activity = Handlers.StartActivity("SensorData.Process");
        activity?.SetTag("sensor.id", sensorId);
        activity?.SetTag("plot.id", plotId);

        if (!string.IsNullOrWhiteSpace(eventId))
        {
            activity?.SetTag("event.id", eventId);
        }

        return activity;
    }

    /// <summary>
    /// Start a threshold evaluation span
    /// </summary>
    /// <param name="thresholdType">Threshold type (temperature, soil_moisture, battery)</param>
    /// <param name="sensorId">Sensor ID</param>
    public static Activity? StartThresholdEvaluation(string thresholdType, string sensorId)
    {
        var activity = Handlers.StartActivity("Threshold.Evaluate");
        activity?.SetTag("threshold.type", thresholdType);
        activity?.SetTag("sensor.id", sensorId);
        return activity;
    }
}
