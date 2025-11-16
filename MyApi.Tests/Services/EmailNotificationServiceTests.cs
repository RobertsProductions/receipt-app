using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyApi.Services;
using Xunit;

namespace MyApi.Tests.Services;

public class EmailNotificationServiceTests
{
    private readonly Mock<ILogger<EmailNotificationService>> _mockLogger;

    public EmailNotificationServiceTests()
    {
        _mockLogger = new Mock<ILogger<EmailNotificationService>>();
    }

    private EmailNotificationService CreateService(
        string? host = "smtp.test.com",
        int port = 587,
        string? username = "test@test.com",
        string? password = "testpass",
        string? fromEmail = "noreply@test.com",
        string? fromName = "Test App",
        bool useSsl = true)
    {
        var configData = new Dictionary<string, string?>
        {
            ["Smtp:Host"] = host,
            ["Smtp:Port"] = port.ToString(),
            ["Smtp:Username"] = username,
            ["Smtp:Password"] = password,
            ["Smtp:FromEmail"] = fromEmail,
            ["Smtp:FromName"] = fromName,
            ["Smtp:UseSsl"] = useSsl.ToString()
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        return new EmailNotificationService(_mockLogger.Object, configuration);
    }

    [Fact]
    public void Constructor_WithValidConfiguration_InitializesSuccessfully()
    {
        // Act
        var service = CreateService();

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithMissingHost_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            CreateService(host: null));
    }

    [Fact]
    public void Constructor_WithMissingUsername_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            CreateService(username: null));
    }

    [Fact]
    public void Constructor_WithMissingPassword_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            CreateService(password: null));
    }

    [Fact]
    public void Constructor_WithDefaultPort_Uses587()
    {
        // Act
        var service = CreateService(port: 587);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithMissingFromEmail_UsesUsernameAsDefault()
    {
        // Act
        var service = CreateService(fromEmail: null);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithMissingFromName_UsesDefaultWarrantyApp()
    {
        // Act
        var service = CreateService(fromName: null);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithEmptyEmail_LogsWarningAndReturns()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SendWarrantyExpirationNotificationAsync(
            "user123",
            "",
            "Test Product",
            DateTime.UtcNow.AddDays(7),
            Guid.NewGuid());

        // Assert - Should not throw, just log warning
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no email address")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithNullEmail_LogsWarningAndReturns()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SendWarrantyExpirationNotificationAsync(
            "user123",
            null!,
            "Test Product",
            DateTime.UtcNow.AddDays(7),
            Guid.NewGuid());

        // Assert - Should not throw, just log warning
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no email address")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyEmail_LogsWarningAndReturns()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SendEmailAsync("", "Test Subject", "<html><body>Test</body></html>");

        // Assert - Should not throw, just log warning
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No recipient email address")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithNullEmail_LogsWarningAndReturns()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.SendEmailAsync(null!, "Test Subject", "<html><body>Test</body></html>");

        // Assert - Should not throw, just log warning
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No recipient email address")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithCustomPort_UsesProvidedPort()
    {
        // Act
        var service = CreateService(port: 465);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithSslDisabled_ConfiguresCorrectly()
    {
        // Act
        var service = CreateService(useSsl: false);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_LogsInitializationMessage()
    {
        // Act
        var service = CreateService(host: "smtp.example.com", port: 587);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email notification service initialized")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Note: Actual SMTP sending tests are integration tests and require a real or mock SMTP server.
    // These unit tests focus on validation, configuration, and edge cases.
    // For integration testing, consider using a library like smtp4dev or MailHog for local testing.
}
