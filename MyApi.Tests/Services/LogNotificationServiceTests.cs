using Microsoft.Extensions.Logging;
using Moq;
using MyApi.Services;
using Xunit;

namespace MyApi.Tests.Services;

public class LogNotificationServiceTests
{
    private readonly Mock<ILogger<LogNotificationService>> _mockLogger;
    private readonly LogNotificationService _service;

    public LogNotificationServiceTests()
    {
        _mockLogger = new Mock<ILogger<LogNotificationService>>();
        _service = new LogNotificationService(_mockLogger.Object);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_LogsWarningWithAllDetails()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(7);
        var receiptId = Guid.NewGuid();

        // Act
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("WARRANTY EXPIRATION NOTIFICATION")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_IncludesUserId()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(7);
        var receiptId = Guid.NewGuid();

        // Act
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_IncludesUserEmail()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(7);
        var receiptId = Guid.NewGuid();

        // Act
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userEmail)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_IncludesProductName()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(7);
        var receiptId = Guid.NewGuid();

        // Act
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(productName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithNullProductName_UsesUnknownProduct()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        string? productName = null;
        var expirationDate = DateTime.UtcNow.Date.AddDays(7);
        var receiptId = Guid.NewGuid();

        // Act
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName!, expirationDate, receiptId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unknown Product")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_IncludesReceiptId()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(7);
        var receiptId = Guid.NewGuid();

        // Act
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(receiptId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_CalculatesDaysCorrectly()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var daysUntilExpiration = 5;
        var expirationDate = DateTime.UtcNow.Date.AddDays(daysUntilExpiration);
        var receiptId = Guid.NewGuid();

        // Act
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"{daysUntilExpiration} days")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_FormatsDateCorrectly()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var expirationDate = new DateTime(2025, 12, 31);
        var receiptId = Guid.NewGuid();
        var expectedDateFormat = "2025-12-31";

        // Act
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedDateFormat)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_CompletesSuccessfully()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(7);
        var receiptId = Guid.NewGuid();

        // Act
        var task = _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);

        // Assert
        Assert.True(task.IsCompleted);
        await task; // Should not throw
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithExpiredWarranty_ShowsNegativeDays()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(-3); // Expired 3 days ago
        var receiptId = Guid.NewGuid();

        // Act
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("-3 days")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_UsesWarningLogLevel()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(7);
        var receiptId = Guid.NewGuid();

        // Act
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);

        // Assert - Verify it's using Warning level, not Info or Error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithEmptyStrings_StillLogs()
    {
        // Arrange
        var userId = "";
        var userEmail = "";
        var productName = "";
        var expirationDate = DateTime.UtcNow.Date.AddDays(7);
        var receiptId = Guid.Empty;

        // Act
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);

        // Assert - Should still log even with empty values
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("WARRANTY EXPIRATION NOTIFICATION")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
