namespace TC.Agro.Analytics.Tests.Application.MessageBrokerHandlers
{
    /// <summary>
    /// Unit tests for SensorIngestedHandler - Application Layer
    /// Tests orchestration, mapping, and integration with dependencies
    /// </summary>
    public class SensorIngestedHandlerTests
    {
        private readonly ISensorReadingRepository _repository;
        private readonly ITransactionalOutbox _outbox;
        private readonly ILogger<SensorIngestedHandler> _logger;
        private readonly IOptions<AlertThresholdsOptions> _options;
        private readonly SensorIngestedHandler _handler;

        public SensorIngestedHandlerTests()
        {
            // Arrange - Setup test doubles (mocks)
            _repository = A.Fake<ISensorReadingRepository>();
            _outbox = A.Fake<ITransactionalOutbox>();
            _logger = A.Fake<ILogger<SensorIngestedHandler>>();

            _options = Options.Create(new AlertThresholdsOptions
            {
                MaxTemperature = 35.0,
                MinSoilMoisture = 20.0,
                MinBatteryLevel = 15.0
            });

            _handler = new SensorIngestedHandler(_repository, _outbox, _logger, _options);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                new SensorIngestedHandler(null!, _outbox, _logger, _options));
        }

        [Fact]
        public void Constructor_WithNullOutbox_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                new SensorIngestedHandler(_repository, null!, _logger, _options));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                new SensorIngestedHandler(_repository, _outbox, null!, _options));
        }

        [Fact]
        public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                new SensorIngestedHandler(_repository, _outbox, _logger, null!));
        }

        #endregion

        #region HandleAsync - Idempotency Tests

        [Fact]
        public async Task HandleAsync_WithDuplicateEvent_ShouldReturnEarlyAndLogWarning()
        {
            // Arrange
            var aggregateId = Guid.NewGuid();
            var existingAggregate = new SensorReadingAggregateBuilder().Build().Value;

            A.CallTo(() => _repository.GetByIdAsync(aggregateId, A<CancellationToken>._))
                .Returns(existingAggregate);

            var @event = CreateSampleEvent(aggregateId);

            // Act
            await _handler.Handle(@event.EventData, CancellationToken.None);

            // Assert - Should NOT persist or commit (idempotency)
            A.CallTo(() => _repository.Add(A<SensorReadingAggregate>._))
                .MustNotHaveHappened();

            A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._))
                .MustNotHaveHappened();

            // Verify warning was logged
            A.CallTo(_logger).MustHaveHappened();
        }

        [Fact]
        public async Task HandleAsync_WithNewEvent_ShouldProcessSuccessfully()
        {
            // Arrange
            var aggregateId = Guid.NewGuid();

            A.CallTo(() => _repository.GetByIdAsync(aggregateId, A<CancellationToken>._))
                .Returns<SensorReadingAggregate?>(null);

            var @event = CreateSampleEvent(aggregateId);

            // Act
            await _handler.Handle(@event.EventData, CancellationToken.None);

            // Assert - Should persist and commit
            A.CallTo(() => _repository.Add(A<SensorReadingAggregate>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        #endregion

        #region HandleAsync - Error Handling Tests

        [Fact]
        public async Task HandleAsync_WithInvalidData_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var aggregateId = Guid.NewGuid();
            A.CallTo(() => _repository.GetByIdAsync(aggregateId, A<CancellationToken>._))
                .Returns<SensorReadingAggregate?>(null);

            var @event = CreateInvalidEvent(aggregateId);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () =>
                await _handler.Handle(@event.EventData, CancellationToken.None));
        }

        [Fact]
        public async Task HandleAsync_WithOutboxFailure_ShouldLogErrorAndRethrow()
        {
            // Arrange
            var aggregateId = Guid.NewGuid();
            A.CallTo(() => _repository.GetByIdAsync(aggregateId, A<CancellationToken>._))
                .Returns<SensorReadingAggregate?>(null);

            A.CallTo(() => _outbox.SaveChangesAsync(A<CancellationToken>._))
                .Throws<InvalidOperationException>();

            var @event = CreateSampleEvent(aggregateId);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () =>
                await _handler.Handle(@event.EventData, CancellationToken.None));

            // Verify error was logged
            A.CallTo(_logger).MustHaveHappened();
        }

        #endregion

        #region Helper Methods

        private static EventContext<SensorIngestedIntegrationEvent> CreateSampleEvent(Guid aggregateId)
        {
            var eventData = new SensorIngestedIntegrationEvent(
                EventId: Guid.NewGuid(),
                AggregateId: aggregateId,
                OccurredOn: DateTimeOffset.UtcNow,
                EventName: nameof(SensorIngestedIntegrationEvent),
                RelatedIds: null,
                SensorId: "SENSOR-001",
                PlotId: Guid.NewGuid(),
                Time: DateTime.UtcNow,
                Temperature: 25.0,
                Humidity: 60.0,
                SoilMoisture: 40.0,
                Rainfall: 5.0,
                BatteryLevel: 85.0
            );

            return new EventContext<SensorIngestedIntegrationEvent>(
                eventData: eventData,
                messageId: Guid.NewGuid(),
                occurredAt: DateTime.UtcNow,
                aggregateId: aggregateId,
                userId: "system",
                isAuthenticated: true,
                correlationId: Guid.NewGuid().ToString(),
                source: "test",
                eventType: nameof(SensorIngestedIntegrationEvent),
                aggregateType: nameof(SensorReadingAggregate),
                version: 1,
                metadata: null
            );
        }

        public static EventContext<SensorIngestedIntegrationEvent> CreateHighTemperatureEvent(Guid aggregateId)
        {
            var eventData = new SensorIngestedIntegrationEvent(
                EventId: Guid.NewGuid(),
                AggregateId: aggregateId,
                OccurredOn: DateTimeOffset.UtcNow,
                EventName: nameof(SensorIngestedIntegrationEvent),
                RelatedIds: null,
                SensorId: "SENSOR-HOT",
                PlotId: Guid.NewGuid(),
                Time: DateTime.UtcNow,
                Temperature: 38.0, // Above threshold
                Humidity: 60.0,
                SoilMoisture: 40.0,
                Rainfall: 5.0,
                BatteryLevel: 85.0
            );

            return new EventContext<SensorIngestedIntegrationEvent>(
                eventData: eventData,
                messageId: Guid.NewGuid(),
                occurredAt: DateTime.UtcNow,
                aggregateId: aggregateId,
                userId: "system",
                isAuthenticated: true,
                correlationId: Guid.NewGuid().ToString(),
                source: "test",
                eventType: nameof(SensorIngestedIntegrationEvent),
                aggregateType: nameof(SensorReadingAggregate),
                version: 1,
                metadata: null
            );
        }

        public static EventContext<SensorIngestedIntegrationEvent> CreateEventWithMultipleAlerts(Guid aggregateId)
        {
            var eventData = new SensorIngestedIntegrationEvent(
                EventId: Guid.NewGuid(),
                AggregateId: aggregateId,
                OccurredOn: DateTimeOffset.UtcNow,
                EventName: nameof(SensorIngestedIntegrationEvent),
                RelatedIds: null,
                SensorId: "SENSOR-CRITICAL",
                PlotId: Guid.NewGuid(),
                Time: DateTime.UtcNow,
                Temperature: 38.0,  // High temp
                Humidity: 60.0,
                SoilMoisture: 15.0, // Low moisture
                Rainfall: 5.0,
                BatteryLevel: 10.0  // Low battery
            );

            return new EventContext<SensorIngestedIntegrationEvent>(
                eventData: eventData,
                messageId: Guid.NewGuid(),
                occurredAt: DateTime.UtcNow,
                aggregateId: aggregateId,
                userId: "system",
                isAuthenticated: true,
                correlationId: Guid.NewGuid().ToString(),
                source: "test",
                eventType: nameof(SensorIngestedIntegrationEvent),
                aggregateType: nameof(SensorReadingAggregate),
                version: 1,
                metadata: null
            );
        }

        private static EventContext<SensorIngestedIntegrationEvent> CreateInvalidEvent(Guid aggregateId)
        {
            var eventData = new SensorIngestedIntegrationEvent(
                EventId: Guid.NewGuid(),
                AggregateId: aggregateId,
                OccurredOn: DateTimeOffset.UtcNow,
                EventName: nameof(SensorIngestedIntegrationEvent),
                RelatedIds: null,
                SensorId: "", // Invalid!
                PlotId: Guid.NewGuid(),
                Time: DateTime.UtcNow,
                Temperature: null,
                Humidity: null,
                SoilMoisture: null, // No metrics!
                Rainfall: null,
                BatteryLevel: null
            );

            return new EventContext<SensorIngestedIntegrationEvent>(
                eventData: eventData,
                messageId: Guid.NewGuid(),
                occurredAt: DateTime.UtcNow,
                aggregateId: aggregateId,
                userId: "system",
                isAuthenticated: true,
                correlationId: Guid.NewGuid().ToString(),
                source: "test",
                eventType: nameof(SensorIngestedIntegrationEvent),
                aggregateType: nameof(SensorReadingAggregate),
                version: 1,
                metadata: null
            );
        }

        #endregion
    }
}
