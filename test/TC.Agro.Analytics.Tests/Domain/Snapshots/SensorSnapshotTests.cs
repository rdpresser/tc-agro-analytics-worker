using TC.Agro.Analytics.Domain.Snapshots;

namespace TC.Agro.Analytics.Tests.Domain.Snapshots;

public class SensorSnapshotTests
{
    #region Create - Valid Cases

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var label = "Sensor-001";
        var plotName = "Plot A";
        var propertyName = "Farm XYZ";
        var status = "Active";

        // Act
        var snapshot = SensorSnapshot.Create(
            id,
            ownerId,
            propertyId,
            plotId,
            label,
            plotName,
            propertyName,
            status);

        // Assert
        snapshot.ShouldNotBeNull();
        snapshot.Id.ShouldBe(id);
        snapshot.OwnerId.ShouldBe(ownerId);
        snapshot.PropertyId.ShouldBe(propertyId);
        snapshot.PlotId.ShouldBe(plotId);
        snapshot.Label.ShouldBe(label);
        snapshot.PlotName.ShouldBe(plotName);
        snapshot.PropertyName.ShouldBe(propertyName);
        snapshot.Status.ShouldBe(status);
        snapshot.IsActive.ShouldBeTrue();
        snapshot.CreatedAt.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-2), DateTimeOffset.UtcNow.AddSeconds(2));
        snapshot.UpdatedAt.ShouldBeNull();
        snapshot.Alerts.ShouldBeEmpty();
    }

    [Fact]
    public void Create_WithoutLabel_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var plotName = "Plot B";
        var propertyName = "Farm ABC";

        // Act
        var snapshot = SensorSnapshot.Create(
            id,
            ownerId,
            propertyId,
            plotId,
            null, // No label
            plotName,
            propertyName);

        // Assert
        snapshot.ShouldNotBeNull();
        snapshot.Label.ShouldBeNull();
        snapshot.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Create_WithoutStatus_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var plotName = "Plot C";
        var propertyName = "Farm DEF";

        // Act
        var snapshot = SensorSnapshot.Create(
            id,
            ownerId,
            propertyId,
            plotId,
            "Sensor-002",
            plotName,
            propertyName);

        // Assert
        snapshot.ShouldNotBeNull();
        snapshot.Status.ShouldBeNull();
        snapshot.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Create_WithCreatedAtParameter_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var plotId = Guid.NewGuid();
        var label = "Sensor-003";
        var plotName = "Plot D";
        var propertyName = "Farm GHI";
        var createdAt = DateTimeOffset.UtcNow.AddDays(-30);
        var status = "Active";

        // Act
        var snapshot = SensorSnapshot.Create(
            id,
            ownerId,
            propertyId,
            plotId,
            label,
            plotName,
            propertyName,
            createdAt,
            status);

        // Assert
        snapshot.ShouldNotBeNull();
        snapshot.CreatedAt.ShouldBe(createdAt);
        snapshot.IsActive.ShouldBeTrue();
        snapshot.UpdatedAt.ShouldBeNull();
    }

    [Theory]
    [InlineData("Sensor Norte", "Plot A", "Fazenda Norte")]
    [InlineData("Sensor Sul", "Plot B", "Fazenda Sul")]
    [InlineData("Sensor Leste", "Plot C", "Fazenda Leste")]
    [InlineData("Sensor Oeste", "Plot D", "Fazenda Oeste")]
    public void Create_WithVariousLabelsAndNames_ShouldSucceed(
        string label,
        string plotName,
        string propertyName)
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var plotId = Guid.NewGuid();

        // Act
        var snapshot = SensorSnapshot.Create(
            id,
            ownerId,
            propertyId,
            plotId,
            label,
            plotName,
            propertyName);

        // Assert
        snapshot.ShouldNotBeNull();
        snapshot.Label.ShouldBe(label);
        snapshot.PlotName.ShouldBe(plotName);
        snapshot.PropertyName.ShouldBe(propertyName);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateAllFields()
    {
        // Arrange
        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Original Label",
            "Original Plot",
            "Original Property");

        var newOwnerId = Guid.NewGuid();
        var newPropertyId = Guid.NewGuid();
        var newPlotId = Guid.NewGuid();
        var newLabel = "Updated Label";
        var newPlotName = "Updated Plot";
        var newPropertyName = "Updated Property";
        var newStatus = "Maintenance";

        // Act
        snapshot.Update(
            newOwnerId,
            newPropertyId,
            newPlotId,
            newLabel,
            newPlotName,
            newPropertyName,
            newStatus);

        // Assert
        snapshot.OwnerId.ShouldBe(newOwnerId);
        snapshot.PropertyId.ShouldBe(newPropertyId);
        snapshot.PlotId.ShouldBe(newPlotId);
        snapshot.Label.ShouldBe(newLabel);
        snapshot.PlotName.ShouldBe(newPlotName);
        snapshot.PropertyName.ShouldBe(newPropertyName);
        snapshot.Status.ShouldBe(newStatus);
        snapshot.UpdatedAt.ShouldNotBeNull();
        snapshot.UpdatedAt.Value.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-2), DateTimeOffset.UtcNow.AddSeconds(2));
    }

    [Fact]
    public async Task Update_MultipleTimes_ShouldUpdateTimestampEachTime()
    {
        // Arrange
        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Label",
            "Plot",
            "Property");

        // Act & Assert - First update
        snapshot.Update(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Label 1",
            "Plot 1",
            "Property 1",
            "Active");

        var firstUpdateTime = snapshot.UpdatedAt;
        firstUpdateTime.ShouldNotBeNull();

        await Task.Delay(100, TestContext.Current.CancellationToken); // Ensure time difference

        // Act & Assert - Second update
        snapshot.Update(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Label 2",
            "Plot 2",
            "Property 2",
            "Inactive");

        var secondUpdateTime = snapshot.UpdatedAt;
        secondUpdateTime.ShouldNotBeNull();
        secondUpdateTime.Value.ShouldBeGreaterThan(firstUpdateTime.Value);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("Inactive")]
    [InlineData("Maintenance")]
    [InlineData("Error")]
    public void Update_WithDifferentStatuses_ShouldUpdateStatus(string status)
    {
        // Arrange
        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor",
            "Plot",
            "Property");

        // Act
        snapshot.Update(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor",
            "Plot",
            "Property",
            status);

        // Assert
        snapshot.Status.ShouldBe(status);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_WhenActive_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor",
            "Plot",
            "Property");

        snapshot.IsActive.ShouldBeTrue();

        // Act
        snapshot.Delete();

        // Assert
        snapshot.IsActive.ShouldBeFalse();
        snapshot.UpdatedAt.ShouldNotBeNull();
        snapshot.UpdatedAt.Value.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-2), DateTimeOffset.UtcNow.AddSeconds(2));
    }

    [Fact]
    public async Task Delete_WhenAlreadyInactive_ShouldNotUpdateTimestamp()
    {
        // Arrange
        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor",
            "Plot",
            "Property");

        snapshot.Delete(); // First deletion
        var firstDeleteTime = snapshot.UpdatedAt;

        await Task.Delay(100, TestContext.Current.CancellationToken); // Ensure time difference

        // Act
        snapshot.Delete(); // Second deletion attempt

        // Assert
        snapshot.IsActive.ShouldBeFalse();
        snapshot.UpdatedAt.ShouldBe(firstDeleteTime); // Should not change
    }

    [Fact]
    public void Delete_ShouldBeIdempotent()
    {
        // Arrange
        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor",
            "Plot",
            "Property");

        // Act
        snapshot.Delete();
        var firstState = snapshot.IsActive;
        var firstTimestamp = snapshot.UpdatedAt;

        snapshot.Delete();
        var secondState = snapshot.IsActive;
        var secondTimestamp = snapshot.UpdatedAt;

        snapshot.Delete();
        var thirdState = snapshot.IsActive;
        var thirdTimestamp = snapshot.UpdatedAt;

        // Assert
        firstState.ShouldBeFalse();
        secondState.ShouldBeFalse();
        thirdState.ShouldBeFalse();
        secondTimestamp.ShouldBe(firstTimestamp);
        thirdTimestamp.ShouldBe(firstTimestamp);
    }

    #endregion

    #region State Transitions

    [Fact]
    public async Task StateTransition_CreateToUpdateToDelete_ShouldMaintainConsistency()
    {
        // Arrange & Act - Create
        var id = Guid.NewGuid();
        var snapshot = SensorSnapshot.Create(
            id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Original Sensor",
            "Original Plot",
            "Original Property");

        var originalCreatedAt = snapshot.CreatedAt;
        snapshot.Id.ShouldBe(id);

        // Act - Update
        snapshot.Update(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Updated Sensor",
            "Updated Plot",
            "Updated Property",
            "Maintenance");

        var updateTime = snapshot.UpdatedAt;

        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Act - Delete
        snapshot.Delete();
        var deleteTime = snapshot.UpdatedAt;

        // Assert
        snapshot.Id.ShouldBe(id); // ID never changes
        snapshot.CreatedAt.ShouldBe(originalCreatedAt); // CreatedAt never changes
        snapshot.IsActive.ShouldBeFalse();
        updateTime.ShouldNotBeNull();
        deleteTime.ShouldNotBeNull();
        deleteTime.Value.ShouldBeGreaterThan(updateTime.Value);
    }

    [Fact]
    public async Task StateTransition_MultipleUpdatesAfterCreation_ShouldMaintainHistory()
    {
        // Arrange
        var id = Guid.NewGuid();
        var snapshot = SensorSnapshot.Create(
            id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor V1",
            "Plot V1",
            "Property V1");

        var createdAt = snapshot.CreatedAt;

        // Act - Update 1
        await Task.Delay(50, TestContext.Current.CancellationToken);
        snapshot.Update(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor V2",
            "Plot V2",
            "Property V2",
            "Active");
        var update1Time = snapshot.UpdatedAt;

        // Act - Update 2
        await Task.Delay(50, TestContext.Current.CancellationToken);
        snapshot.Update(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor V3",
            "Plot V3",
            "Property V3",
            "Maintenance");
        var update2Time = snapshot.UpdatedAt;

        // Act - Update 3
        await Task.Delay(50, TestContext.Current.CancellationToken);
        snapshot.Update(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor V4",
            "Plot V4",
            "Property V4",
            "Active");
        var update3Time = snapshot.UpdatedAt;

        // Assert
        snapshot.Id.ShouldBe(id);
        snapshot.CreatedAt.ShouldBe(createdAt);
        snapshot.Label.ShouldBe("Sensor V4");
        update1Time.ShouldNotBeNull();
        update2Time.ShouldNotBeNull();
        update3Time.ShouldNotBeNull();
        update2Time.Value.ShouldBeGreaterThan(update1Time.Value);
        update3Time.Value.ShouldBeGreaterThan(update2Time.Value);
    }

    #endregion

    #region Immutability and Relationships

    [Fact]
    public void Create_ShouldInitializeEmptyAlertsCollection()
    {
        // Arrange & Act
        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor",
            "Plot",
            "Property");

        // Assert
        snapshot.Alerts.ShouldNotBeNull();
        snapshot.Alerts.ShouldBeEmpty();
        snapshot.Alerts.Count.ShouldBe(0);
    }

    [Fact]
    public void Create_ShouldNotAllowDirectModificationOfId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var snapshot = SensorSnapshot.Create(
            id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor",
            "Plot",
            "Property");

        // Assert
        snapshot.Id.ShouldBe(id);
        // Id property has private setter, so it cannot be changed externally
    }

    [Fact]
    public void Update_ShouldNotModifyCreatedAt()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow.AddMonths(-1);
        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor",
            "Plot",
            "Property",
            createdAt);

        // Act
        snapshot.Update(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Updated Sensor",
            "Updated Plot",
            "Updated Property",
            "Active");

        // Assert
        snapshot.CreatedAt.ShouldBe(createdAt); // Should never change
    }

    [Fact]
    public void Delete_ShouldNotModifyCreatedAt()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow.AddMonths(-1);
        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor",
            "Plot",
            "Property",
            createdAt);

        // Act
        snapshot.Delete();

        // Assert
        snapshot.CreatedAt.ShouldBe(createdAt); // Should never change
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_WithEmptyStringsForNames_ShouldSucceed()
    {
        // Arrange & Act
        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            string.Empty, // Empty label
            string.Empty, // Empty plot name
            string.Empty); // Empty property name

        // Assert
        snapshot.ShouldNotBeNull();
        snapshot.Label.ShouldBe(string.Empty);
        snapshot.PlotName.ShouldBe(string.Empty);
        snapshot.PropertyName.ShouldBe(string.Empty);
    }

    [Fact]
    public void Update_ShouldPreserveOwnerRelationship()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var snapshot = SensorSnapshot.Create(
            Guid.NewGuid(),
            ownerId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sensor",
            "Plot",
            "Property");

        var newOwnerId = Guid.NewGuid();

        // Act
        snapshot.Update(
            newOwnerId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Updated Sensor",
            "Updated Plot",
            "Updated Property",
            "Active");

        // Assert
        snapshot.OwnerId.ShouldBe(newOwnerId);
        snapshot.OwnerId.ShouldNotBe(ownerId);
    }

    #endregion
}
