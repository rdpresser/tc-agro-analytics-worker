namespace TC.Agro.Analytics.Domain.Aggregates
{
    public sealed class AlertAggregate : BaseAggregateRoot
    {
        public string SensorId { get; private set; } = default!;
        public Guid PlotId { get; private set; }
        public AlertType Type { get; private set; } = default!;
        public AlertSeverity Severity { get; private set; } = default!;
        public AlertStatus Status { get; private set; } = default!;
        public string Message { get; private set; } = default!;
        public DateTime DetectedAt { get; private set; }
        public DateTime? AcknowledgedAt { get; private set; }
        public string? AcknowledgedBy { get; private set; }
        public DateTime? ResolvedAt { get; private set; }
        public string? ResolvedBy { get; private set; }
        public string? ResolutionNotes { get; private set; }

        public double? Temperature { get; private set; }
        public double? Humidity { get; private set; }
        public double? SoilMoisture { get; private set; }
        public double? Rainfall { get; private set; }
        public double? BatteryLevel { get; private set; }

        private AlertAggregate(Guid id) : base(id) { }

        // For EF Core
        private AlertAggregate() { }

        #region Factory

        public static Result<AlertAggregate> Create(
            string sensorId,
            Guid plotId,
            AlertType type,
            AlertSeverity severity,
            string message,
            double? temperature = null,
            double? humidity = null,
            double? soilMoisture = null,
            double? rainfall = null,
            double? batteryLevel = null)
        {
            var errors = new List<ValidationError>();
            errors.AddRange(ValidateSensorId(sensorId));
            errors.AddRange(ValidatePlotId(plotId));
            errors.AddRange(ValidateMessage(message));

            if (errors.Count > 0)
                return Result.Invalid(errors.ToArray());

            var aggregate = new AlertAggregate(Guid.NewGuid());
            var @event = new AlertCreatedDomainEvent(
                aggregate.Id,
                sensorId,
                plotId,
                type,
                severity,
                message,
                temperature,
                humidity,
                soilMoisture,
                rainfall,
                batteryLevel,
                DateTimeOffset.UtcNow);

            aggregate.ApplyEvent(@event);
            return Result.Success(aggregate);
        }

        #endregion

        #region Commands

        public Result Acknowledge(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result.Invalid(new ValidationError
                {
                    Identifier = "UserId.Required",
                    ErrorMessage = "User ID is required to acknowledge an alert."
                });

            if (Status.IsAcknowledged)
                return Result.Invalid(new ValidationError
                {
                    Identifier = "Alert.AlreadyAcknowledged",
                    ErrorMessage = "Alert has already been acknowledged."
                });

            if (Status.IsResolved)
                return Result.Invalid(new ValidationError
                {
                    Identifier = "Alert.AlreadyResolved",
                    ErrorMessage = "Cannot acknowledge a resolved alert."
                });

            var @event = new AlertAcknowledgedDomainEvent(
                Id,
                userId,
                DateTimeOffset.UtcNow);

            ApplyEvent(@event);
            return Result.Success();
        }

        public Result Resolve(string userId, string resolutionNotes)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result.Invalid(new ValidationError
                {
                    Identifier = "UserId.Required",
                    ErrorMessage = "User ID is required to resolve an alert."
                });

            if (string.IsNullOrWhiteSpace(resolutionNotes))
                return Result.Invalid(new ValidationError
                {
                    Identifier = "ResolutionNotes.Required",
                    ErrorMessage = "Resolution notes are required."
                });

            if (Status.IsResolved)
                return Result.Invalid(new ValidationError
                {
                    Identifier = "Alert.AlreadyResolved",
                    ErrorMessage = "Alert has already been resolved."
                });

            var @event = new AlertResolvedDomainEvent(
                Id,
                userId,
                resolutionNotes,
                DateTimeOffset.UtcNow);

            ApplyEvent(@event);
            return Result.Success();
        }

        #endregion

        #region Event Apply

        public void Apply(AlertCreatedDomainEvent @event)
        {
            SetId(@event.AggregateId);
            SensorId = @event.SensorId;
            PlotId = @event.PlotId;
            Type = @event.Type;
            Severity = @event.Severity;
            Message = @event.Message;
            Temperature = @event.Temperature;
            Humidity = @event.Humidity;
            SoilMoisture = @event.SoilMoisture;
            Rainfall = @event.Rainfall;
            BatteryLevel = @event.BatteryLevel;
            Status = AlertStatus.Pending;
            DetectedAt = @event.OccurredOn.DateTime;
            SetCreatedAt(@event.OccurredOn);
            SetActivate();
        }

        public void Apply(AlertAcknowledgedDomainEvent @event)
        {
            Status = AlertStatus.Acknowledged;
            AcknowledgedAt = @event.OccurredOn.DateTime;
            AcknowledgedBy = @event.AcknowledgedBy;
        }

        public void Apply(AlertResolvedDomainEvent @event)
        {
            Status = AlertStatus.Resolved;
            ResolvedAt = @event.OccurredOn.DateTime;
            ResolvedBy = @event.ResolvedBy;
            ResolutionNotes = @event.ResolutionNotes;
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

        #region Validation Helpers

        private static IEnumerable<ValidationError> ValidateSensorId(string sensorId)
        {
            if (string.IsNullOrWhiteSpace(sensorId))
                yield return new ValidationError { Identifier = "SensorId.Required", ErrorMessage = "SensorId is required." };
            else if (sensorId.Length > 100)
                yield return new ValidationError { Identifier = "SensorId.TooLong", ErrorMessage = "SensorId must be at most 100 characters." };
        }

        private static IEnumerable<ValidationError> ValidatePlotId(Guid plotId)
        {
            if (plotId == Guid.Empty)
                yield return new ValidationError { Identifier = "PlotId.Required", ErrorMessage = "PlotId is required." };
        }

        private static IEnumerable<ValidationError> ValidateMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                yield return new ValidationError { Identifier = "Message.Required", ErrorMessage = "Alert message is required." };
            else if (message.Length > 500)
                yield return new ValidationError { Identifier = "Message.TooLong", ErrorMessage = "Alert message must be at most 500 characters." };
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
            double? Temperature,
            double? Humidity,
            double? SoilMoisture,
            double? Rainfall,
            double? BatteryLevel,
            DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record AlertAcknowledgedDomainEvent(
            Guid AggregateId,
            string AcknowledgedBy,
            DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        public record AlertResolvedDomainEvent(
            Guid AggregateId,
            string ResolvedBy,
            string ResolutionNotes,
            DateTimeOffset OccurredOn) : BaseDomainEvent(AggregateId, OccurredOn);

        #endregion
    }
}
