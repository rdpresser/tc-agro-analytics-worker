using Microsoft.Extensions.Logging;
using TC.Agro.Analytics.Domain.Aggregates;
using TC.Agro.Analytics.Domain.Entities;
using TC.Agro.Analytics.Domain.ValueObjects;
using static TC.Agro.Analytics.Domain.Aggregates.SensorReadingAggregate;

namespace TC.Agro.Analytics.Infrastructure.Projections
{
    /// <summary>
    /// Projection handler that transforms Domain Events into Alert read models.
    /// This is the CQRS Query Side - it projects write-side events into optimized read models.
    /// </summary>
    public class AlertProjectionHandler
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AlertProjectionHandler> _logger;

        public AlertProjectionHandler(
            ApplicationDbContext dbContext,
            ILogger<AlertProjectionHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Projects HighTemperatureDetected domain event into alerts table
        /// </summary>
        public async Task Handle(HighTemperatureDetectedDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Projecting HighTemperatureDetected event for Sensor {SensorId}, Temperature: {Temperature}°C",
                domainEvent.SensorId,
                domainEvent.Temperature);

            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                SensorReadingId = domainEvent.AggregateId,
                SensorId = domainEvent.SensorId,
                PlotId = domainEvent.PlotId,
                AlertType = AlertType.HighTemperature,
                Message = $"High temperature detected: {domainEvent.Temperature:F1}°C",
                Status = AlertStatus.Pending,
                Severity = DetermineSeverity(35.0, domainEvent.Temperature),
                Value = domainEvent.Temperature,
                Threshold = 35.0,
                CreatedAt = domainEvent.OccurredOn.UtcDateTime, // PostgreSQL requer UTC
                Metadata = System.Text.Json.JsonSerializer.Serialize(new
                {
                    domainEvent.Humidity,
                    domainEvent.SoilMoisture,
                    domainEvent.Rainfall,
                    domainEvent.BatteryLevel
                })
            };

            await _dbContext.Alerts.AddAsync(alert, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Alert {AlertId} created for HighTemperature event from Sensor {SensorId}",
                alert.Id,
                domainEvent.SensorId);
        }

        /// <summary>
        /// Projects LowSoilMoistureDetected domain event into alerts table
        /// </summary>
        public async Task Handle(LowSoilMoistureDetectedDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Projecting LowSoilMoistureDetected event for Sensor {SensorId}, Soil Moisture: {SoilMoisture}%",
                domainEvent.SensorId,
                domainEvent.SoilMoisture);

            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                SensorReadingId = domainEvent.AggregateId,
                SensorId = domainEvent.SensorId,
                PlotId = domainEvent.PlotId,
                AlertType = AlertType.LowSoilMoisture,
                Message = $"Low soil moisture detected: {domainEvent.SoilMoisture:F1}% - Irrigation may be needed",
                Status = AlertStatus.Pending,
                Severity = DetermineSeverity(20.0, domainEvent.SoilMoisture),
                Value = domainEvent.SoilMoisture,
                Threshold = 20.0,
                CreatedAt = domainEvent.OccurredOn.UtcDateTime, // PostgreSQL requer UTC
                Metadata = System.Text.Json.JsonSerializer.Serialize(new
                {
                    domainEvent.Temperature,
                    domainEvent.Humidity,
                    domainEvent.Rainfall,
                    domainEvent.BatteryLevel
                })
            };

            await _dbContext.Alerts.AddAsync(alert, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Alert {AlertId} created for LowSoilMoisture event from Sensor {SensorId}",
                alert.Id,
                domainEvent.SensorId);
        }

        /// <summary>
        /// Projects BatteryLowWarning domain event into alerts table
        /// </summary>
        public async Task Handle(BatteryLowWarningDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Projecting BatteryLowWarning event for Sensor {SensorId}, Battery: {BatteryLevel}%",
                domainEvent.SensorId,
                domainEvent.BatteryLevel);

            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                SensorReadingId = domainEvent.AggregateId,
                SensorId = domainEvent.SensorId,
                PlotId = domainEvent.PlotId,
                AlertType = AlertType.LowBattery,
                Message = $"Low battery warning: {domainEvent.BatteryLevel:F1}% - Sensor maintenance required",
                Status = AlertStatus.Pending,
                Severity = DetermineSeverity(domainEvent.Threshold, domainEvent.BatteryLevel),
                Value = domainEvent.BatteryLevel,
                Threshold = domainEvent.Threshold,
                CreatedAt = domainEvent.OccurredOn.UtcDateTime, // PostgreSQL requer UTC
                Metadata = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Threshold = domainEvent.Threshold
                })
            };

            await _dbContext.Alerts.AddAsync(alert, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Alert {AlertId} created for BatteryLowWarning event from Sensor {SensorId}",
                alert.Id,
                domainEvent.SensorId);
        }

        /// <summary>
        /// Determines alert severity based on how far the value is from the threshold
        /// </summary>
        private static string DetermineSeverity(double threshold, double actualValue)
        {
            var difference = Math.Abs(actualValue - threshold);
            var percentDifference = (difference / threshold) * 100;

            return percentDifference switch
            {
                >= 50 => AlertSeverity.Critical,
                >= 25 => AlertSeverity.High,
                >= 10 => AlertSeverity.Medium,
                _ => AlertSeverity.Low
            };
        }
    }
}
