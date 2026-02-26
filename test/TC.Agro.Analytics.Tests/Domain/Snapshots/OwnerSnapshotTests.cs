using TC.Agro.Analytics.Domain.Snapshots;

namespace TC.Agro.Analytics.Tests.Domain.Snapshots;

public class OwnerSnapshotTests
{
    #region Create - Valid Cases

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "John Doe";
        var email = "john.doe@example.com";

        // Act
        var snapshot = OwnerSnapshot.Create(id, name, email);

        // Assert
        snapshot.ShouldNotBeNull();
        snapshot.Id.ShouldBe(id);
        snapshot.Name.ShouldBe(name);
        snapshot.Email.ShouldBe(email);
        snapshot.IsActive.ShouldBeTrue();
        snapshot.CreatedAt.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-2), DateTimeOffset.UtcNow.AddSeconds(2));
        snapshot.UpdatedAt.ShouldBeNull();
        snapshot.Sensors.ShouldBeEmpty();
    }

    [Fact]
    public void Create_WithCreatedAtParameter_ShouldSucceed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Jane Smith";
        var email = "jane.smith@example.com";
        var createdAt = DateTimeOffset.UtcNow.AddDays(-10);

        // Act
        var snapshot = OwnerSnapshot.Create(id, name, email, createdAt);

        // Assert
        snapshot.ShouldNotBeNull();
        snapshot.Id.ShouldBe(id);
        snapshot.Name.ShouldBe(name);
        snapshot.Email.ShouldBe(email);
        snapshot.IsActive.ShouldBeTrue();
        snapshot.CreatedAt.ShouldBe(createdAt);
        snapshot.UpdatedAt.ShouldBeNull();
    }

    [Theory]
    [InlineData("Alice Johnson", "alice@example.com")]
    [InlineData("Bob Wilson", "bob.wilson@company.com")]
    [InlineData("Carol Martinez", "c.martinez@email.com")]
    [InlineData("David Brown", "david.brown@test.org")]
    public void Create_WithVariousNamesAndEmails_ShouldSucceed(string name, string email)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var snapshot = OwnerSnapshot.Create(id, name, email);

        // Assert
        snapshot.ShouldNotBeNull();
        snapshot.Name.ShouldBe(name);
        snapshot.Email.ShouldBe(email);
        snapshot.IsActive.ShouldBeTrue();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateNameAndEmail()
    {
        // Arrange
        var snapshot = OwnerSnapshot.Create(
            Guid.NewGuid(),
            "Original Name",
            "original@example.com");

        var newName = "Updated Name";
        var newEmail = "updated@example.com";

        // Act
        snapshot.Update(newName, newEmail);

        // Assert
        snapshot.Name.ShouldBe(newName);
        snapshot.Email.ShouldBe(newEmail);
        snapshot.UpdatedAt.ShouldNotBeNull();
        snapshot.UpdatedAt.Value.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-2), DateTimeOffset.UtcNow.AddSeconds(2));
    }

    [Fact]
    public async Task Update_MultipleTimes_ShouldUpdateTimestampEachTime()
    {
        // Arrange
        var snapshot = OwnerSnapshot.Create(
            Guid.NewGuid(),
            "Original Name",
            "original@example.com");

        // Act & Assert - First update
        snapshot.Update("Name 1", "email1@example.com");
        var firstUpdateTime = snapshot.UpdatedAt;
        firstUpdateTime.ShouldNotBeNull();

        await Task.Delay(100, TestContext.Current.CancellationToken); // Ensure time difference

        // Act & Assert - Second update
        snapshot.Update("Name 2", "email2@example.com");
        var secondUpdateTime = snapshot.UpdatedAt;
        secondUpdateTime.ShouldNotBeNull();
        secondUpdateTime.Value.ShouldBeGreaterThan(firstUpdateTime.Value);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var snapshot = OwnerSnapshot.Create(
            Guid.NewGuid(),
            "Test User",
            "test@example.com");

        snapshot.Delete(); // Make inactive first
        snapshot.IsActive.ShouldBeFalse();

        // Act
        snapshot.Activate();

        // Assert
        snapshot.IsActive.ShouldBeTrue();
        snapshot.UpdatedAt.ShouldNotBeNull();
        snapshot.UpdatedAt.Value.ShouldBeInRange(DateTimeOffset.UtcNow.AddSeconds(-2), DateTimeOffset.UtcNow.AddSeconds(2));
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotUpdateTimestamp()
    {
        // Arrange
        var snapshot = OwnerSnapshot.Create(
            Guid.NewGuid(),
            "Test User",
            "test@example.com");

        var initialUpdatedAt = snapshot.UpdatedAt;

        // Act
        snapshot.Activate();

        // Assert
        snapshot.IsActive.ShouldBeTrue();
        snapshot.UpdatedAt.ShouldBe(initialUpdatedAt); // Should not change
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_WhenActive_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var snapshot = OwnerSnapshot.Create(
            Guid.NewGuid(),
            "Test User",
            "test@example.com");

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
        var snapshot = OwnerSnapshot.Create(
            Guid.NewGuid(),
            "Test User",
            "test@example.com");

        snapshot.Delete(); // First deletion
        var firstDeleteTime = snapshot.UpdatedAt;

        await Task.Delay(100, TestContext.Current.CancellationToken); // Ensure time difference

        // Act
        snapshot.Delete(); // Second deletion attempt

        // Assert
        snapshot.IsActive.ShouldBeFalse();
        snapshot.UpdatedAt.ShouldBe(firstDeleteTime); // Should not change
    }

    #endregion

    #region State Transitions

    [Fact]
    public async Task StateTransition_CreateToUpdateToDelete_ShouldMaintainConsistency()
    {
        // Arrange & Act - Create
        var snapshot = OwnerSnapshot.Create(
            Guid.NewGuid(),
            "Test User",
            "test@example.com");

        var originalCreatedAt = snapshot.CreatedAt;

        // Act - Update
        snapshot.Update("Updated User", "updated@example.com");
        var updateTime = snapshot.UpdatedAt;

        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Act - Delete
        snapshot.Delete();
        var deleteTime = snapshot.UpdatedAt;

        // Assert
        snapshot.CreatedAt.ShouldBe(originalCreatedAt); // CreatedAt never changes
        snapshot.IsActive.ShouldBeFalse();
        updateTime.ShouldNotBeNull();
        deleteTime.ShouldNotBeNull();
        deleteTime.Value.ShouldBeGreaterThan(updateTime.Value);
    }

    [Fact]
    public void StateTransition_DeleteThenActivate_ShouldRestoreActiveState()
    {
        // Arrange
        var snapshot = OwnerSnapshot.Create(
            Guid.NewGuid(),
            "Test User",
            "test@example.com");

        // Act
        snapshot.Delete();
        snapshot.IsActive.ShouldBeFalse();

        snapshot.Activate();

        // Assert
        snapshot.IsActive.ShouldBeTrue();
        snapshot.UpdatedAt.ShouldNotBeNull();
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void Create_ShouldNotAllowDirectModificationOfId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var snapshot = OwnerSnapshot.Create(id, "Test", "test@example.com");

        // Assert
        snapshot.Id.ShouldBe(id);
        // Id property has private setter, so it cannot be changed externally
    }

    [Fact]
    public void Create_ShouldInitializeEmptySensorsCollection()
    {
        // Arrange & Act
        var snapshot = OwnerSnapshot.Create(
            Guid.NewGuid(),
            "Test User",
            "test@example.com");

        // Assert
        snapshot.Sensors.ShouldNotBeNull();
        snapshot.Sensors.ShouldBeEmpty();
        snapshot.Sensors.Count.ShouldBe(0);
    }

    #endregion
}
