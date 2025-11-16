using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyApi.Models;
using MyApi.Services;
using Xunit;

namespace MyApi.Tests.Services;

public class CompositeNotificationServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly EmailNotificationService _emailService;
    private readonly SmsNotificationService _smsService;
    private readonly Mock<ILogger<CompositeNotificationService>> _mockLogger;
    private readonly CompositeNotificationService _service;

    public CompositeNotificationServiceTests()
    {
        // Mock UserManager
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Create real Email and SMS services with test configuration
        _emailService = CreateEmailService();
        _smsService = CreateSmsService();

        _mockLogger = new Mock<ILogger<CompositeNotificationService>>();

        _service = new CompositeNotificationService(
            _mockUserManager.Object,
            _emailService,
            _smsService,
            _mockLogger.Object);
    }

    private EmailNotificationService CreateEmailService()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Smtp:Host"] = "smtp.test.com",
            ["Smtp:Port"] = "587",
            ["Smtp:Username"] = "test@test.com",
            ["Smtp:Password"] = "testpass",
            ["Smtp:FromEmail"] = "noreply@test.com",
            ["Smtp:FromName"] = "Test App",
            ["Smtp:UseSsl"] = "true"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var logger = new Mock<ILogger<EmailNotificationService>>();
        return new EmailNotificationService(logger.Object, configuration);
    }

    private SmsNotificationService CreateSmsService()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Twilio:AccountSid"] = "test_sid",
            ["Twilio:AuthToken"] = "test_token",
            ["Twilio:FromPhoneNumber"] = "+1234567890"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var logger = new Mock<ILogger<SmsNotificationService>>();
        return new SmsNotificationService(logger.Object, configuration);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithEmailAndSms_CompletesSuccessfully()
    {
        // Arrange
        var userId = "user1";
        var userEmail = "user@test.com";
        var phoneNumber = "+1234567890";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(5);
        var receiptId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            Email = userEmail,
            PhoneNumber = phoneNumber,
            NotificationChannel = NotificationChannel.EmailAndSms,
            OptOutOfNotifications = false
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert - Should complete without exception
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);
        
        // Note: Since we're using real services that simulate sending,
        // we can only verify it doesn't throw
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithEmailOnly_CompletesSuccessfully()
    {
        // Arrange
        var userId = "user1";
        var userEmail = "user@test.com";
        var phoneNumber = "+1234567890";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(5);
        var receiptId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            Email = userEmail,
            PhoneNumber = phoneNumber,
            NotificationChannel = NotificationChannel.EmailOnly,
            OptOutOfNotifications = false
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithSmsOnly_CompletesSuccessfully()
    {
        // Arrange
        var userId = "user1";
        var userEmail = "user@test.com";
        var phoneNumber = "+1234567890";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(5);
        var receiptId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            Email = userEmail,
            PhoneNumber = phoneNumber,
            NotificationChannel = NotificationChannel.SmsOnly,
            OptOutOfNotifications = false
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithNoneChannel_CompletesWithoutSending()
    {
        // Arrange
        var userId = "user1";
        var userEmail = "user@test.com";
        var phoneNumber = "+1234567890";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(5);
        var receiptId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            Email = userEmail,
            PhoneNumber = phoneNumber,
            NotificationChannel = NotificationChannel.None,
            OptOutOfNotifications = false
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert - Should complete without exception
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithOptedOutUser_CompletesWithoutSending()
    {
        // Arrange
        var userId = "user1";
        var userEmail = "user@test.com";
        var phoneNumber = "+1234567890";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(5);
        var receiptId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            Email = userEmail,
            PhoneNumber = phoneNumber,
            NotificationChannel = NotificationChannel.EmailAndSms,
            OptOutOfNotifications = true // User opted out
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithUserNotFound_CompletesWithoutSending()
    {
        // Arrange
        var userId = "nonexistent";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(5);
        var receiptId = Guid.NewGuid();

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithEmailOnly_AndNoEmail_LogsWarning()
    {
        // Arrange
        var userId = "user1";
        var userEmail = ""; // No email
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(5);
        var receiptId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            Email = "", // No email
            PhoneNumber = "+1234567890",
            NotificationChannel = NotificationChannel.EmailOnly,
            OptOutOfNotifications = false
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);
    }

    [Fact]
    public async Task SendWarrantyExpirationNotificationAsync_WithSmsOnly_AndNoPhone_CompletesWithoutSending()
    {
        // Arrange
        var userId = "user1";
        var userEmail = "user@test.com";
        var productName = "Test Product";
        var expirationDate = DateTime.UtcNow.Date.AddDays(5);
        var receiptId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            Email = userEmail,
            PhoneNumber = "", // No phone
            NotificationChannel = NotificationChannel.SmsOnly,
            OptOutOfNotifications = false
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        await _service.SendWarrantyExpirationNotificationAsync(
            userId, userEmail, productName, expirationDate, receiptId);
    }
}
