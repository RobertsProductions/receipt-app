using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MyApi.Models;
using Xunit;

namespace MyApi.Tests.Models;

public class ApplicationUserTests
{
    [Fact]
    public void ApplicationUser_ShouldInheritFromIdentityUser()
    {
        // Arrange
        var user = new ApplicationUser();

        // Assert
        user.Should().BeAssignableTo<IdentityUser>();
    }

    [Fact]
    public void ApplicationUser_ShouldHaveNotificationProperties()
    {
        // Arrange & Act
        var user = new ApplicationUser
        {
            Id = "test-id",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            NotificationChannel = NotificationChannel.EmailOnly,
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        // Assert
        user.Id.Should().Be("test-id");
        user.UserName.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.NotificationChannel.Should().Be(NotificationChannel.EmailOnly);
        user.NotificationThresholdDays.Should().Be(7);
        user.OptOutOfNotifications.Should().BeFalse();
    }

    [Fact]
    public void ApplicationUser_ShouldHave2FAProperties()
    {
        // Arrange
        var user = new ApplicationUser();

        // Act
        user.TwoFactorEnabled = true;

        // Assert
        user.TwoFactorEnabled.Should().BeTrue();
    }

    [Fact]
    public void ApplicationUser_ShouldStoreRefreshToken()
    {
        // Arrange & Act
        var refreshToken = "test-refresh-token";
        var expiry = DateTime.UtcNow.AddDays(7);
        var user = new ApplicationUser
        {
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = expiry
        };

        // Assert
        user.RefreshToken.Should().Be(refreshToken);
        user.RefreshTokenExpiryTime.Should().BeCloseTo(expiry, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ApplicationUser_DefaultNotificationThreshold_ShouldBe7Days()
    {
        // Arrange & Act
        var user = new ApplicationUser();

        // Assert
        user.NotificationThresholdDays.Should().Be(7);
    }

    [Fact]
    public void ApplicationUser_DefaultNotificationChannel_ShouldBeEmailAndSms()
    {
        // Arrange & Act
        var user = new ApplicationUser();

        // Assert
        user.NotificationChannel.Should().Be(NotificationChannel.EmailAndSms);
    }

    [Fact]
    public void ApplicationUser_DefaultOptOut_ShouldBeFalse()
    {
        // Arrange & Act
        var user = new ApplicationUser();

        // Assert
        user.OptOutOfNotifications.Should().BeFalse();
    }

    [Fact]
    public void ApplicationUser_ShouldHaveCreatedAtTimestamp()
    {
        // Arrange & Act
        var user = new ApplicationUser();

        // Assert
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ApplicationUser_ShouldStoreLastLoginAt()
    {
        // Arrange & Act
        var lastLogin = DateTime.UtcNow.AddMinutes(-30);
        var user = new ApplicationUser
        {
            LastLoginAt = lastLogin
        };

        // Assert
        user.LastLoginAt.Should().BeCloseTo(lastLogin, TimeSpan.FromSeconds(1));
    }
}

public class NotificationChannelTests
{
    [Theory]
    [InlineData(NotificationChannel.None, 0)]
    [InlineData(NotificationChannel.EmailOnly, 1)]
    [InlineData(NotificationChannel.SmsOnly, 2)]
    [InlineData(NotificationChannel.EmailAndSms, 3)]
    public void NotificationChannel_ShouldHaveCorrectValue(NotificationChannel channel, int expectedValue)
    {
        // Assert
        ((int)channel).Should().Be(expectedValue);
    }
}
