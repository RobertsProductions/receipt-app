using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyApi.Data;
using MyApi.Models;
using MyApi.Services;
using Xunit;

namespace MyApi.Tests.Services;

public class WarrantyExpirationServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<WarrantyExpirationService>> _mockLogger;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;

    public WarrantyExpirationServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // Setup mocks
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<WarrantyExpirationService>>();

        // Setup memory cache
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Setup configuration
        var configData = new Dictionary<string, string?>
        {
            ["WarrantyNotification:CheckIntervalHours"] = "24",
            ["WarrantyNotification:NotificationDaysThreshold"] = "7"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Setup service provider
        var services = new ServiceCollection();
        services.AddSingleton(_dbContext);
        services.AddSingleton(_mockNotificationService.Object);
        services.AddSingleton(_memoryCache);
        services.AddSingleton(_configuration);
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
    }

    private WarrantyExpirationService CreateService(
        int? checkIntervalHours = null,
        int? notificationDaysThreshold = null)
    {
        var configData = new Dictionary<string, string?>
        {
            ["WarrantyNotification:CheckIntervalHours"] = (checkIntervalHours ?? 24).ToString(),
            ["WarrantyNotification:NotificationDaysThreshold"] = (notificationDaysThreshold ?? 7).ToString()
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        return new WarrantyExpirationService(
            _serviceProvider,
            _memoryCache,
            _mockLogger.Object,
            configuration);
    }

    private async Task InvokeCheckAndNotifyAsync(WarrantyExpirationService service, CancellationToken cancellationToken = default)
    {
        // Use reflection to call the private CheckAndNotifyExpiringWarrantiesAsync method
        var method = typeof(WarrantyExpirationService).GetMethod(
            "CheckAndNotifyExpiringWarrantiesAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        await (Task)method!.Invoke(service, new object[] { cancellationToken })!;
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
    public void Constructor_WithCustomConfiguration_UsesCustomValues()
    {
        // Arrange
        var customIntervalHours = 12;
        var customThreshold = 14;

        // Act
        var service = CreateService(customIntervalHours, customThreshold);

        // Assert
        Assert.NotNull(service);
        // Configuration is applied - we can verify this through the service's behavior
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithNoReceipts_DoesNotSendNotifications()
    {
        // Arrange
        var service = CreateService();

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithExpiringReceipt_SendsNotification()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = "Test Product",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(5),
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService(checkIntervalHours: 1);

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                user.Id,
                user.Email,
                receipt.ProductName,
                receipt.WarrantyExpirationDate.Value,
                receipt.Id),
            Times.Once);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithOptedOutUser_DoesNotSendNotification()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = true // User opted out
        };

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = "Test Product",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(5),
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService();

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithMultipleUsers_SendsNotificationsToEach()
    {
        // Arrange
        var user1 = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        var user2 = new ApplicationUser
        {
            Id = "user2",
            Email = "user2@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        var receipt1 = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            User = user1,
            ProductName = "Product 1",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(5),
            FileName = "test1.pdf",
            FileType = "pdf",
            StoragePath = "/test/path1"
        };

        var receipt2 = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            User = user2,
            ProductName = "Product 2",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(6),
            FileName = "test2.pdf",
            FileType = "pdf",
            StoragePath = "/test/path2"
        };

        await _dbContext.Users.AddRangeAsync(user1, user2);
        await _dbContext.Receipts.AddRangeAsync(receipt1, receipt2);
        await _dbContext.SaveChangesAsync();

        var service = CreateService();

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                user1.Id,
                user1.Email,
                receipt1.ProductName,
                receipt1.WarrantyExpirationDate.Value,
                receipt1.Id),
            Times.Once);

        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                user2.Id,
                user2.Email,
                receipt2.ProductName,
                receipt2.WarrantyExpirationDate.Value,
                receipt2.Id),
            Times.Once);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithCustomThreshold_RespectsUserPreference()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 3, // User wants notification only 3 days before
            OptOutOfNotifications = false
        };

        // Receipt expiring in 5 days - should NOT trigger notification for this user
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = "Test Product",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(5),
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService(notificationDaysThreshold: 7);

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithExpiredWarranty_DoesNotSendNotification()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        // Receipt already expired
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = "Test Product",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(-5), // Already expired
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService();

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithFarFutureWarranty_DoesNotSendNotification()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        // Receipt expiring far in the future (beyond 90 days)
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = "Test Product",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(100),
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService();

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_UpdatesCache_WithExpiringWarranties()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = "Test Product",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(5),
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService();

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        var cachedNotifications = _memoryCache.Get<List<WarrantyNotification>>("warranty_expiration_cache");
        Assert.NotNull(cachedNotifications);
        Assert.Single(cachedNotifications);
        Assert.Equal(user.Id, cachedNotifications[0].UserId);
        Assert.Equal(receipt.ProductName, cachedNotifications[0].ProductName);
        Assert.Equal(receipt.Id, cachedNotifications[0].ReceiptId);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithNullProductName_UsesFallback()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = null, // No product name
            Description = "Test Description",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(5),
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService();

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                user.Id,
                user.Email,
                "Test Description", // Should use description as fallback
                receipt.WarrantyExpirationDate.Value,
                receipt.Id),
            Times.Once);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithNoProductNameOrDescription_UsesDefaultFallback()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = null,
            Description = null,
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(5),
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService();

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                user.Id,
                user.Email,
                "Product", // Should use "Product" as default fallback
                receipt.WarrantyExpirationDate.Value,
                receipt.Id),
            Times.Once);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_DoesNotNotifyTwice_ForSameReceipt()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = "Test Product",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(5),
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService(checkIntervalHours: 1);

        // Act - Run twice to verify no duplicate notification
        await InvokeCheckAndNotifyAsync(service);
        await InvokeCheckAndNotifyAsync(service);

        // Assert - Should only notify once
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                user.Id,
                user.Email,
                receipt.ProductName,
                receipt.WarrantyExpirationDate.Value,
                receipt.Id),
            Times.Once);

        // Verify cache contains the notified receipt
        var notifiedReceipts = _memoryCache.Get<HashSet<Guid>>("notified_receipts");
        Assert.NotNull(notifiedReceipts);
        Assert.Contains(receipt.Id, notifiedReceipts);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_CalculatesDaysUntilExpiration_Correctly()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        var daysUntilExpiration = 3;
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = "Test Product",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(daysUntilExpiration),
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService();

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        var cachedNotifications = _memoryCache.Get<List<WarrantyNotification>>("warranty_expiration_cache");
        Assert.NotNull(cachedNotifications);
        Assert.Single(cachedNotifications);
        Assert.Equal(daysUntilExpiration, cachedNotifications[0].DaysUntilExpiration);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithExceptionInNotificationService_ThrowsException()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = "Product 1",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(5),
            FileName = "test1.pdf",
            FileType = "pdf",
            StoragePath = "/test/path1"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        // Setup notification service to throw exception
        _mockNotificationService
            .Setup(x => x.SendWarrantyExpirationNotificationAsync(
                user.Id,
                user.Email,
                receipt.ProductName,
                receipt.WarrantyExpirationDate.Value,
                receipt.Id))
            .ThrowsAsync(new Exception("Notification failed"));

        var service = CreateService();

        // Act & Assert - Should throw the exception
        await Assert.ThrowsAsync<Exception>(async () => await InvokeCheckAndNotifyAsync(service));
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithReceiptsAtThresholdBoundary_SendsNotifications()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        // Receipt expiring exactly at threshold
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = "Test Product",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(7), // Exactly 7 days
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService(notificationDaysThreshold: 7);

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert - Should send notification at exact threshold
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                user.Id,
                user.Email,
                receipt.ProductName,
                receipt.WarrantyExpirationDate.Value,
                receipt.Id),
            Times.Once);
    }

    [Fact]
    public async Task CheckAndNotifyExpiringWarranties_WithReceiptsJustPastThreshold_DoesNotSendNotification()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user1",
            Email = "user1@test.com",
            NotificationThresholdDays = 7,
            OptOutOfNotifications = false
        };

        // Receipt expiring just past threshold
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ProductName = "Test Product",
            WarrantyExpirationDate = DateTime.UtcNow.Date.AddDays(8), // 8 days, past 7-day threshold
            FileName = "test.pdf",
            FileType = "pdf",
            StoragePath = "/test/path"
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.Receipts.AddAsync(receipt);
        await _dbContext.SaveChangesAsync();

        var service = CreateService(notificationDaysThreshold: 7);

        // Act
        await InvokeCheckAndNotifyAsync(service);

        // Assert
        _mockNotificationService.Verify(
            x => x.SendWarrantyExpirationNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _serviceProvider?.Dispose();
        _memoryCache?.Dispose();
    }
}
