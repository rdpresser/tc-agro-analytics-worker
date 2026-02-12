namespace TC.Agro.Analytics.Domain.Aggregates;

/// <summary>
/// Alert aggregate root with full lifecycle management.
/// Following DDD pattern: alerts have their own lifecycle (Pending → Acknowledged → Resolved).
/// 
/// Ownership (per Domain Map):
/// - Analytics.Worker OWNS Alert (rules evaluation and alert lifecycle)
/// - Sensor.Ingest.Api OWNS SensorReading (time-series data)
/// 
/// This aggregate is necessary because:
/// - Alerts can change state (Pending → Acknowledged → Resolved)
/// - State transitions have business rules (invariants)
/// - Users interact with alerts (acknowledge, resolve)
/// 
/// Pattern matches Farm Service (PlotAggregate with lifecycle) and Identity Service (UserAggregate with activation).
/// </summary>
public sealed class AlertAggregate : BaseAggregateRoot
{
    public string SensorId { get; private set; } = default!;
    public Guid PlotId { get; private set; }
    public AlertType Type { get; private set; } = default!;
    public AlertSeverity Severity { get; private set; } = default!;
    public AlertStatus Status { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public double Value { get; private set; }
    public double Threshold { get; private set; }
    public string? Metadata { get; private set; }
    
    public DateTime? AcknowledgedAt { get; private set; }
    public string? AcknowledgedBy { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public string? ResolvedBy { get; private set; }
    public string? ResolutionNotes { get; private set; }

    private AlertAggregate(Guid id) : base(id) { }

    private AlertAggregate() : base(Guid.Empty) { }

    #region Factory

    public static Result<AlertAggregate> Create(
        string sensorId,
        Guid plotId,
        AlertType type,
        AlertSeverity severity,
        string message,
        double value,
        double threshold,
        string? metadata = null)
    {
        var errors = new List<ValidationError>();
        errors.AddRange(ValidateSensorId(sensorId));
        errors.AddRange(ValidatePlotId(plotId));
        errors.AddRange(ValidateMessage(message));

        if (errors.Count > 0)
            return Result.Invalid(errors.ToArray());

        return CreateAggregate(sensorId, plotId, type, severity, message, value, threshold, metadata);
    }

    private static Result<AlertAggregate> CreateAggregate(
        string sensorId,
        Guid plotId,
        AlertType type,
        AlertSeverity severity,
        string message,
        double value,
        double threshold,
        string? metadata)
    {
        var aggregate = new AlertAggregate(Guid.NewGuid());
        var @event = new AlertCreatedDomainEvent(
            AggregateId: aggregate.Id,
            SensorId: sensorId,
            PlotId: plotId,
            Type: type,
            Severity: severity,
            Message: message,
            Value: value,
            Threshold: threshold,
            Metadata: metadata,
            OccurredOn: DateTimeOffset.UtcNow);

        aggregate.ApplyEvent(@event);
        return Result.Success(aggregate);
    }

    #endregion

    #region Domain Events Apply

    public void Apply(AlertCreatedDomainEvent @event)
    {
        SetId(@event.AggregateId);
        SensorId = @event.SensorId;
        PlotId = @event.PlotId;
        Type = @event.Type;
        Severity = @event.Severity;
        Message = @event.Message;
        Value = @event.Value;
        Threshold = @event.Threshold;
        Metadata = @event.Metadata;
        Status = AlertStatus.Pending;
        SetCreatedAt(@event.OccurredOn);
        SetActivate();
    }

    public void Apply(AlertAcknowledgedDomainEvent @event)
    {
        AcknowledgedAt = @event.AcknowledgedAt;
        AcknowledgedBy = @event.AcknowledgedBy;
        Status = AlertStatus.Acknowledged;
        SetUpdatedAt(@event.OccurredOn);
    }

    public void Apply(AlertResolvedDomainEvent @event)
    {
        ResolvedAt = @event.ResolvedAt;
        ResolvedBy = @event.ResolvedBy;
        ResolutionNotes = @event.ResolutionNotes;
        Status = AlertStatus.Resolved;
        SetUpdatedAt(@event.OccurredOn);
    }

    private void ApplyEvent(BaseDomainEvent @event)
    {
        AddNewEvent(@event);
        switch (@event)
        {
            case AlertCreatedDomainEvent created:
                Apply(created);
                break;
            case AlertAcknowledgedDomainEvent acknowledged:
                Apply(acknowledged);
                break;
            case AlertResolvedDomainEvent resolved:
                Apply(resolved);
                break;
        }
    }

    #endregion

    #region Business Logic

    /// <summary>
    /// Acknowledges the alert (user has seen it).
    /// Business rule: Only pending alerts can be acknowledged.
    /// </summary>
    public Result Acknowledge(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Invalid(new ValidationError(
                "UserId.Required",
                "User ID is required to acknowledge alert"));

        if (Status != AlertStatus.Pending)
            return Result.Invalid(new ValidationError(
                "Alert.NotPending",
                "Only pending alerts can be acknowledged"));

        var @event = new AlertAcknowledgedDomainEvent(
            AggregateId: Id,
            AcknowledgedAt: DateTime.UtcNow,
            AcknowledgedBy: userId,
            OccurredOn: DateTimeOffset.UtcNow);

        ApplyEvent(@event);
        return Result.Success();
    }

    /// <summary>
    /// Resolves the alert (issue has been addressed).
    /// Business rule: Only pending or acknowledged alerts can be resolved.
    /// </summary>
    public Result Resolve(string userId, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Invalid(new ValidationError(
                "UserId.Required",
                "User ID is required to resolve alert"));

        if (Status == AlertStatus.Resolved)
            return Result.Invalid(new ValidationError(
                "Alert.AlreadyResolved",
                "Alert is already resolved"));

        var @event = new AlertResolvedDomainEvent(
            AggregateId: Id,
            ResolvedAt: DateTime.UtcNow,
            ResolvedBy: userId,
            ResolutionNotes: notes,
            OccurredOn: DateTimeOffset.UtcNow);

        ApplyEvent(@event);
        return Result.Success();
    }

    #endregion

    #region Validation

    private static IEnumerable<ValidationError> ValidateSensorId(string sensorId)
    {
        if (string.IsNullOrWhiteSpace(sensorId))
            yield return new ValidationError("SensorId.Required", "SensorId is required");
        else if (sensorId.Length > 100)
            yield return new ValidationError("SensorId.TooLong", "SensorId must be at most 100 characters");
    }

    private static IEnumerable<ValidationError> ValidatePlotId(Guid plotId)
    {
        if (plotId == Guid.Empty)
            yield return new ValidationError("PlotId.Required", "PlotId is required");
    }

    private static IEnumerable<ValidationError> ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            yield return new ValidationError("Message.Required", "Message is required");
        else if (message.Length > 500)
            yield return new ValidationError("Message.TooLong", "Message must be at most 500 characters");
    }

    #endregion

    #region Domain Events

    public record AlertCreatedDomainEvent(
        Guid AggregateId,
        string SensorId,
        Guid PlotId,
        AlertType Type,
        AlertSeverity Severity,
        string Message,
        double Value,
        double Threshold,
        string? Metadata,
        DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

    public record AlertAcknowledgedDomainEvent(
        Guid AggregateId,
        DateTime AcknowledgedAt,
        string AcknowledgedBy,
        DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

    public record AlertResolvedDomainEvent(
        Guid AggregateId,
        DateTime ResolvedAt,
        string ResolvedBy,
        string? ResolutionNotes,
        DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

    #endregion
}
