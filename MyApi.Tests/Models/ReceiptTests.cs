using FluentAssertions;
using MyApi.Models;
using Xunit;

namespace MyApi.Tests.Models;

public class ReceiptTests
{
    [Fact]
    public void Receipt_ShouldStoreWarrantyExpirationDate()
    {
        // Arrange
        var purchaseDate = new DateTime(2024, 1, 1);
        var warrantyMonths = 12;
        var expectedExpiration = purchaseDate.AddMonths(warrantyMonths);

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            PurchaseDate = purchaseDate,
            WarrantyMonths = warrantyMonths,
            WarrantyExpirationDate = expectedExpiration
        };

        // Act & Assert
        receipt.WarrantyExpirationDate.Should().Be(expectedExpiration);
    }

    [Fact]
    public void Receipt_WithNoWarrantyExpirationDate_ShouldBeNull()
    {
        // Arrange
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            PurchaseDate = DateTime.Now,
            WarrantyMonths = 12
        };

        // Act & Assert
        receipt.WarrantyExpirationDate.Should().BeNull();
    }

    [Fact]
    public void Receipt_ShouldStoreAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = "test-user";
        var fileName = "receipt.jpg";
        var fileType = "image/jpeg";
        var fileSizeBytes = 1024L;
        var storagePath = "/path/to/file";
        var description = "Test receipt";
        var uploadedAt = DateTime.UtcNow;
        var purchaseDate = DateTime.UtcNow.AddDays(-30);
        var merchant = "Test Merchant";
        var amount = 99.99m;
        var productName = "Test Product";
        var warrantyMonths = 24;
        var warrantyExpiration = purchaseDate.AddMonths(warrantyMonths);
        var notes = "Test notes";

        // Act
        var receipt = new Receipt
        {
            Id = id,
            UserId = userId,
            FileName = fileName,
            FileType = fileType,
            FileSizeBytes = fileSizeBytes,
            StoragePath = storagePath,
            Description = description,
            UploadedAt = uploadedAt,
            PurchaseDate = purchaseDate,
            Merchant = merchant,
            Amount = amount,
            ProductName = productName,
            WarrantyMonths = warrantyMonths,
            WarrantyExpirationDate = warrantyExpiration,
            Notes = notes
        };

        // Assert
        receipt.Id.Should().Be(id);
        receipt.UserId.Should().Be(userId);
        receipt.FileName.Should().Be(fileName);
        receipt.FileType.Should().Be(fileType);
        receipt.FileSizeBytes.Should().Be(fileSizeBytes);
        receipt.StoragePath.Should().Be(storagePath);
        receipt.Description.Should().Be(description);
        receipt.UploadedAt.Should().Be(uploadedAt);
        receipt.PurchaseDate.Should().Be(purchaseDate);
        receipt.Merchant.Should().Be(merchant);
        receipt.Amount.Should().Be(amount);
        receipt.ProductName.Should().Be(productName);
        receipt.WarrantyMonths.Should().Be(warrantyMonths);
        receipt.WarrantyExpirationDate.Should().Be(warrantyExpiration);
        receipt.Notes.Should().Be(notes);
    }

    [Fact]
    public void Receipt_ShouldHaveDefaultId()
    {
        // Arrange & Act
        var receipt = new Receipt();

        // Assert
        receipt.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Receipt_ShouldHaveDefaultUploadedAt()
    {
        // Arrange & Act
        var receipt = new Receipt();

        // Assert
        receipt.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Receipt_NullableFields_ShouldAcceptNull()
    {
        // Arrange & Act
        var receipt = new Receipt
        {
            PurchaseDate = null,
            Merchant = null,
            Amount = null,
            ProductName = null,
            WarrantyMonths = null,
            WarrantyExpirationDate = null,
            Description = null,
            Notes = null
        };

        // Assert
        receipt.PurchaseDate.Should().BeNull();
        receipt.Merchant.Should().BeNull();
        receipt.Amount.Should().BeNull();
        receipt.ProductName.Should().BeNull();
        receipt.WarrantyMonths.Should().BeNull();
        receipt.WarrantyExpirationDate.Should().BeNull();
        receipt.Description.Should().BeNull();
        receipt.Notes.Should().BeNull();
    }
}
