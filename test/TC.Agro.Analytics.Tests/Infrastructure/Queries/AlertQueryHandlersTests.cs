using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TC.Agro.Analytics.Domain.Entities;
using TC.Agro.Analytics.Infrastructure;
using TC.Agro.Analytics.Infrastructure.Queries;
using TC.Agro.Analytics.Tests.Builders;
using TC.Agro.SharedKernel.Infrastructure.Database;

namespace TC.Agro.Analytics.Tests.Infrastructure.Queries;

public class AlertQueryHandlersTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Guid _testPlotId = Guid.Parse("ae57f8d7-d491-4899-bb39-30124093e683");
    private readonly CultureInfo _originalCulture;

    public AlertQueryHandlersTests()
    {
        // Save original culture and set to InvariantCulture for consistent number formatting
        _originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options, null!);
    }

    #region GetPendingAlertsQueryHandler Tests

    [Fact]
    public async Task GetPendingAlerts_WithPendingAlerts_ShouldReturnOnlyPending()
    {
        // Arrange
        await SeedAlertsAsync();
        var logger = A.Fake<ILogger<GetPendingAlertsQueryHandler>>();
        var handler = new GetPendingAlertsQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.All(a => a.Status == AlertStatus.Pending).ShouldBeTrue();
    }

    [Fact]
    public async Task GetPendingAlerts_ShouldOrderByCreatedAtDescending()
    {
        // Arrange
        await SeedAlertsAsync();
        var logger = A.Fake<ILogger<GetPendingAlertsQueryHandler>>();
        var handler = new GetPendingAlertsQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        for (int i = 0; i < result.Count - 1; i++)
        {
            result[i].CreatedAt.ShouldBeGreaterThanOrEqualTo(result[i + 1].CreatedAt);
        }
    }

    [Fact]
    public async Task GetPendingAlerts_WithNoAlerts_ShouldReturnEmptyList()
    {
        // Arrange
        var logger = A.Fake<ILogger<GetPendingAlertsQueryHandler>>();
        var handler = new GetPendingAlertsQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetPendingAlerts_ShouldLimit100Results()
    {
        // Arrange
        for (int i = 0; i < 150; i++)
        {
            var alert = new AlertBuilder()
                .WithPlotId(_testPlotId)
                .WithHighTemperature(40.0)
                .WithStatus(AlertStatus.Pending)
                .WithCreatedAt(DateTime.UtcNow.AddMinutes(-i))
                .Build();
            await _dbContext.Alerts.AddAsync(alert, TestContext.Current.CancellationToken);
        }
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var logger = A.Fake<ILogger<GetPendingAlertsQueryHandler>>();
        var handler = new GetPendingAlertsQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(TestContext.Current.CancellationToken);

        // Assert
        result.Count.ShouldBe(100);
    }

    #endregion

    #region GetAlertHistoryQueryHandler Tests

    [Fact]
    public async Task GetAlertHistory_WithPlotId_ShouldReturnOnlyForThatPlot()
    {
        // Arrange
        await SeedAlertsAsync();
        var otherPlotId = Guid.NewGuid();
        
        var otherAlert = new AlertBuilder()
            .WithPlotId(otherPlotId)
            .WithHighTemperature(40.0)
            .WithStatus(AlertStatus.Pending)
            .Build();
        await _dbContext.Alerts.AddAsync(otherAlert, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var logger = A.Fake<ILogger<GetAlertHistoryQueryHandler>>();
        var handler = new GetAlertHistoryQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, days: 30, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.All(a => a.PlotId == _testPlotId).ShouldBeTrue();
        result.Any(a => a.PlotId == otherPlotId).ShouldBeFalse();
    }

    [Fact]
    public async Task GetAlertHistory_WithDaysFilter_ShouldReturnOnlyWithinTimeRange()
    {
        // Arrange
        var now = DateTime.UtcNow;
        
        var recentAlert = new AlertBuilder()
            .WithPlotId(_testPlotId)
            .WithHighTemperature(40.0)
            .WithCreatedAt(now.AddDays(-2))
            .Build();
        
        var oldAlert = new AlertBuilder()
            .WithPlotId(_testPlotId)
            .WithLowSoilMoisture(15.0)
            .WithCreatedAt(now.AddDays(-10))
            .Build();

        await _dbContext.Alerts.AddRangeAsync(new[] { recentAlert, oldAlert }, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var logger = A.Fake<ILogger<GetAlertHistoryQueryHandler>>();
        var handler = new GetAlertHistoryQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, days: 7, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(recentAlert.Id);
    }

    [Theory]
    [InlineData(AlertTypes.HighTemperature)]
    [InlineData(AlertTypes.LowSoilMoisture)]
    [InlineData(AlertTypes.LowBattery)]
    public async Task GetAlertHistory_WithAlertTypeFilter_ShouldReturnOnlyThatType(string alertType)
    {
        // Arrange
        await SeedAlertsAsync();
        var logger = A.Fake<ILogger<GetAlertHistoryQueryHandler>>();
        var handler = new GetAlertHistoryQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, days: 30, alertType: alertType, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.All(a => a.AlertType == alertType).ShouldBeTrue();
    }

    [Theory]
    [InlineData(AlertStatus.Pending, 3)]
    [InlineData(AlertStatus.Acknowledged, 1)]
    [InlineData(AlertStatus.Resolved, 1)]
    public async Task GetAlertHistory_WithStatusFilter_ShouldReturnOnlyThatStatus(string status, int expectedCount)
    {
        // Arrange
        await SeedAlertsAsync();
        var logger = A.Fake<ILogger<GetAlertHistoryQueryHandler>>();
        var handler = new GetAlertHistoryQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, days: 30, status: status, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedCount);
        result.All(a => a.Status == status).ShouldBeTrue();
    }

    [Fact]
    public async Task GetAlertHistory_WithCombinedFilters_ShouldApplyAllFilters()
    {
        // Arrange
        await SeedAlertsAsync();
        var logger = A.Fake<ILogger<GetAlertHistoryQueryHandler>>();
        var handler = new GetAlertHistoryQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(
            _testPlotId, 
            days: 30, 
            alertType: AlertTypes.HighTemperature, 
            status: AlertStatus.Pending,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.All(a => a.AlertType == AlertTypes.HighTemperature).ShouldBeTrue();
        result.All(a => a.Status == AlertStatus.Pending).ShouldBeTrue();
    }

    [Fact]
    public async Task GetAlertHistory_ShouldOrderByCreatedAtDescending()
    {
        // Arrange
        await SeedAlertsAsync();
        var logger = A.Fake<ILogger<GetAlertHistoryQueryHandler>>();
        var handler = new GetAlertHistoryQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, days: 30, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        for (int i = 0; i < result.Count - 1; i++)
        {
            result[i].CreatedAt.ShouldBeGreaterThanOrEqualTo(result[i + 1].CreatedAt);
        }
    }

    #endregion

    #region GetPlotStatusQueryHandler Tests

    [Fact]
    public async Task GetPlotStatus_WithAlerts_ShouldReturnCorrectAggregations()
    {
        // Arrange
        await SeedAlertsAsync();
        var logger = A.Fake<ILogger<GetPlotStatusQueryHandler>>();
        var handler = new GetPlotStatusQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.PlotId.ShouldBe(_testPlotId);
        result.PendingAlertsCount.ShouldBe(3);
        result.TotalAlertsLast7Days.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetPlotStatus_ShouldGroupAlertsByType()
    {
        // Arrange
        await SeedAlertsAsync();
        var logger = A.Fake<ILogger<GetPlotStatusQueryHandler>>();
        var handler = new GetPlotStatusQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, TestContext.Current.CancellationToken);

        // Assert
        result.AlertsByType.ShouldNotBeEmpty();
        result.AlertsByType.ContainsKey(AlertTypes.HighTemperature).ShouldBeTrue();
        result.AlertsByType.ContainsKey(AlertTypes.LowSoilMoisture).ShouldBeTrue();
        result.AlertsByType.ContainsKey(AlertTypes.LowBattery).ShouldBeTrue();
    }

    [Fact]
    public async Task GetPlotStatus_ShouldGroupAlertsBySeverity()
    {
        // Arrange
        await SeedAlertsAsync();
        var logger = A.Fake<ILogger<GetPlotStatusQueryHandler>>();
        var handler = new GetPlotStatusQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, TestContext.Current.CancellationToken);

        // Assert
        result.AlertsBySeverity.ShouldNotBeEmpty();
        result.AlertsBySeverity.Values.Sum().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetPlotStatus_WithCriticalAlerts_ShouldReturnCriticalStatus()
    {
        // Arrange
        var criticalAlert = new AlertBuilder()
            .WithPlotId(_testPlotId)
            .WithHighTemperature(50.0)
            .WithSeverity(AlertSeverity.Critical)
            .WithStatus(AlertStatus.Pending)
            .Build();

        await _dbContext.Alerts.AddAsync(criticalAlert, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var logger = A.Fake<ILogger<GetPlotStatusQueryHandler>>();
        var handler = new GetPlotStatusQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, TestContext.Current.CancellationToken);

        // Assert
        result.OverallStatus.ShouldBe("Critical");
    }

    [Fact]
    public async Task GetPlotStatus_WithOnlyHighAlerts_ShouldReturnWarningStatus()
    {
        // Arrange
        var highAlert = new AlertBuilder()
            .WithPlotId(_testPlotId)
            .WithHighTemperature(40.0)
            .WithSeverity(AlertSeverity.High)
            .WithStatus(AlertStatus.Pending)
            .Build();

        await _dbContext.Alerts.AddAsync(highAlert, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var logger = A.Fake<ILogger<GetPlotStatusQueryHandler>>();
        var handler = new GetPlotStatusQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, TestContext.Current.CancellationToken);

        // Assert
        result.OverallStatus.ShouldBe("Warning");
    }

    [Fact]
    public async Task GetPlotStatus_WithNoAlerts_ShouldReturnOKStatus()
    {
        // Arrange
        var logger = A.Fake<ILogger<GetPlotStatusQueryHandler>>();
        var handler = new GetPlotStatusQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, TestContext.Current.CancellationToken);

        // Assert
        result.OverallStatus.ShouldBe("OK");
        result.PendingAlertsCount.ShouldBe(0);
        result.TotalAlertsLast7Days.ShouldBe(0);
    }

    [Fact]
    public async Task GetPlotStatus_ShouldReturnMostRecentAlert()
    {
        // Arrange
        var now = DateTime.UtcNow;
        
        var oldAlert = new AlertBuilder()
            .WithPlotId(_testPlotId)
            .WithCreatedAt(now.AddHours(-2))
            .Build();
        
        var recentAlert = new AlertBuilder()
            .WithPlotId(_testPlotId)
            .WithCreatedAt(now.AddMinutes(-5))
            .Build();

        await _dbContext.Alerts.AddRangeAsync(new[] { oldAlert, recentAlert }, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var logger = A.Fake<ILogger<GetPlotStatusQueryHandler>>();
        var handler = new GetPlotStatusQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, TestContext.Current.CancellationToken);

        // Assert
        result.MostRecentAlert.ShouldNotBeNull();
        result.MostRecentAlert.Id.ShouldBe(recentAlert.Id);
    }

    [Fact]
    public async Task GetPlotStatus_ShouldOnlyIncludeLast7Days()
    {
        // Arrange
        var now = DateTime.UtcNow;
        
        var recentAlert = new AlertBuilder()
            .WithPlotId(_testPlotId)
            .WithCreatedAt(now.AddDays(-3))
            .Build();
        
        var oldAlert = new AlertBuilder()
            .WithPlotId(_testPlotId)
            .WithCreatedAt(now.AddDays(-10))
            .Build();

        await _dbContext.Alerts.AddRangeAsync(new[] { recentAlert, oldAlert }, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var logger = A.Fake<ILogger<GetPlotStatusQueryHandler>>();
        var handler = new GetPlotStatusQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, TestContext.Current.CancellationToken);

        // Assert
        result.TotalAlertsLast7Days.ShouldBe(1);
    }

    [Fact]
    public async Task GetPlotStatus_ShouldCalculateLast24HoursCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        
        var withinLastDay = new AlertBuilder()
            .WithPlotId(_testPlotId)
            .WithCreatedAt(now.AddHours(-12))
            .Build();
        
        var beyondLastDay = new AlertBuilder()
            .WithPlotId(_testPlotId)
            .WithCreatedAt(now.AddHours(-30))
            .Build();

        await _dbContext.Alerts.AddRangeAsync(new[] { withinLastDay, beyondLastDay }, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var logger = A.Fake<ILogger<GetPlotStatusQueryHandler>>();
        var handler = new GetPlotStatusQueryHandler(_dbContext, logger);

        // Act
        var result = await handler.Handle(_testPlotId, TestContext.Current.CancellationToken);

        // Assert
        result.TotalAlertsLast24Hours.ShouldBe(1);
    }

    #endregion

    #region Helper Methods

    private async Task SeedAlertsAsync()
    {
        var now = DateTime.UtcNow;

        var alerts = new List<Alert>
        {
            new AlertBuilder()
                .WithPlotId(_testPlotId)
                .WithHighTemperature(40.0)
                .WithSeverity(AlertSeverity.Critical)
                .WithStatus(AlertStatus.Pending)
                .WithCreatedAt(now.AddHours(-1))
                .Build(),
            
            new AlertBuilder()
                .WithPlotId(_testPlotId)
                .WithLowSoilMoisture(15.0)
                .WithSeverity(AlertSeverity.High)
                .WithStatus(AlertStatus.Pending)
                .WithCreatedAt(now.AddHours(-2))
                .Build(),
            
            new AlertBuilder()
                .WithPlotId(_testPlotId)
                .WithLowBattery(10.0)
                .WithSeverity(AlertSeverity.Medium)
                .WithStatus(AlertStatus.Pending)
                .WithCreatedAt(now.AddHours(-3))
                .Build(),

            new AlertBuilder()
                .WithPlotId(_testPlotId)
                .WithHighTemperature(37.0)
                .WithSeverity(AlertSeverity.Medium)
                .AsAcknowledged("user@example.com", now.AddHours(-4))
                .WithCreatedAt(now.AddHours(-5))
                .Build(),

            new AlertBuilder()
                .WithPlotId(_testPlotId)
                .WithLowSoilMoisture(18.0)
                .WithSeverity(AlertSeverity.Low)
                .AsResolved("admin@example.com", "Irrigation applied", now.AddHours(-6))
                .WithCreatedAt(now.AddHours(-7))
                .Build()
        };

        await _dbContext.Alerts.AddRangeAsync(alerts, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    #endregion

    public void Dispose()
    {
        // Restore original culture
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalCulture;

        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}

