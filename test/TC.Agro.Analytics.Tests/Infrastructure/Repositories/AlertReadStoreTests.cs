using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using TC.Agro.Analytics.Domain.Aggregates;
using TC.Agro.Analytics.Domain.Snapshots;
using TC.Agro.Analytics.Domain.ValueObjects;
using TC.Agro.Analytics.Infrastructure;
using TC.Agro.Analytics.Infrastructure.Repositories;
using TC.Agro.SharedKernel.Infrastructure.UserClaims;

namespace TC.Agro.Analytics.Tests.Infrastructure.Repositories;

public sealed class AlertReadStoreTests
{
    [Fact]
    public async Task GetPendingAlertsAsync_WhenCallerIsProducer_ShouldReturnOnlyOwnAlerts()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var dbContext = CreateDbContext();
        var seed = await SeedDataAsync(dbContext, ct);

        var sut = new AlertReadStore(dbContext, CreateProducerContext(seed.OwnerAId));

        var result = await sut.GetPendingAlertsAsync(
            ownerId: null,
            search: null,
            severity: null,
            status: null,
            pageNumber: 1,
            pageSize: 20,
            cancellationToken: ct);

        result.TotalCount.ShouldBe(1);
        result.Data.Count.ShouldBe(1);
        result.Data[0].SensorId.ShouldBe(seed.OwnerASensorId);
    }

    [Fact]
    public async Task GetPendingAlertsSummaryAsync_WhenCallerIsAdminWithOwnerFilter_ShouldApplyOwnerScope()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var dbContext = CreateDbContext();
        var seed = await SeedDataAsync(dbContext, ct);

        var sut = new AlertReadStore(dbContext, CreateAdminContext());

        var summary = await sut.GetPendingAlertsSummaryAsync(
            ownerId: seed.OwnerBId,
            windowHours: 24,
            cancellationToken: ct);

        summary.PendingAlertsTotal.ShouldBe(1);
        summary.AffectedSensorsCount.ShouldBe(1);
        summary.HighPendingCount.ShouldBe(1);
        summary.CriticalPendingCount.ShouldBe(0);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"analytics-alert-read-store-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<(Guid OwnerAId, Guid OwnerBId, Guid OwnerASensorId)> SeedDataAsync(
        ApplicationDbContext dbContext,
        CancellationToken ct)
    {
        var ownerAId = Guid.NewGuid();
        var ownerBId = Guid.NewGuid();
        var ownerASensorId = Guid.NewGuid();
        var ownerBSensorId = Guid.NewGuid();

        dbContext.OwnerSnapshots.Add(OwnerSnapshot.Create(ownerAId, "Owner A", "owner-a@tcagro.test"));
        dbContext.OwnerSnapshots.Add(OwnerSnapshot.Create(ownerBId, "Owner B", "owner-b@tcagro.test"));

        dbContext.SensorSnapshots.Add(SensorSnapshot.Create(
            id: ownerASensorId,
            ownerId: ownerAId,
            propertyId: Guid.NewGuid(),
            plotId: Guid.NewGuid(),
            label: "Sensor A",
            plotName: "Plot A",
            propertyName: "Property A",
            status: "Active"));

        dbContext.SensorSnapshots.Add(SensorSnapshot.Create(
            id: ownerBSensorId,
            ownerId: ownerBId,
            propertyId: Guid.NewGuid(),
            plotId: Guid.NewGuid(),
            label: "Sensor B",
            plotName: "Plot B",
            propertyName: "Property B",
            status: "Active"));

        var alertA = AlertAggregate.Create(
            sensorId: ownerASensorId,
            type: AlertType.HighTemperature,
            severity: AlertSeverity.Critical,
            message: "Owner A critical alert",
            value: 45,
            threshold: 35);

        var alertB = AlertAggregate.Create(
            sensorId: ownerBSensorId,
            type: AlertType.LowBattery,
            severity: AlertSeverity.High,
            message: "Owner B high alert",
            value: 10,
            threshold: 20);

        alertA.IsSuccess.ShouldBeTrue();
        alertB.IsSuccess.ShouldBeTrue();

        dbContext.Alerts.Add(alertA.Value);
        dbContext.Alerts.Add(alertB.Value);

        await dbContext.SaveChangesAsync(ct);

        return (ownerAId, ownerBId, ownerASensorId);
    }

    private static IUserContext CreateProducerContext(Guid ownerId)
    {
        var userContext = A.Fake<IUserContext>();

        A.CallTo(() => userContext.IsAdmin).Returns(false);
        A.CallTo(() => userContext.IsAuthenticated).Returns(true);
        A.CallTo(() => userContext.Id).Returns(ownerId);

        return userContext;
    }

    private static IUserContext CreateAdminContext()
    {
        var userContext = A.Fake<IUserContext>();

        A.CallTo(() => userContext.IsAdmin).Returns(true);
        A.CallTo(() => userContext.IsAuthenticated).Returns(true);
        A.CallTo(() => userContext.Id).Returns(Guid.NewGuid());

        return userContext;
    }
}
