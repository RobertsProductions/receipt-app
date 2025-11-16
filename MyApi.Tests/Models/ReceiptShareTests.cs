using FluentAssertions;
using MyApi.Models;
using Xunit;

namespace MyApi.Tests.Models;

public class ReceiptShareTests
{
    [Fact]
    public void ReceiptShare_ShouldHaveDefaultId()
    {
        // Arrange & Act
        var share = new ReceiptShare();

        // Assert
        share.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void ReceiptShare_ShouldHaveDefaultSharedAt()
    {
        // Arrange & Act
        var share = new ReceiptShare();

        // Assert
        share.SharedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ReceiptShare_ShouldStoreAllRequiredProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var receiptId = Guid.NewGuid();
        var ownerId = "owner-user-id";
        var sharedWithUserId = "recipient-user-id";
        var sharedAt = DateTime.UtcNow;

        // Act
        var share = new ReceiptShare
        {
            Id = id,
            ReceiptId = receiptId,
            OwnerId = ownerId,
            SharedWithUserId = sharedWithUserId,
            SharedAt = sharedAt
        };

        // Assert
        share.Id.Should().Be(id);
        share.ReceiptId.Should().Be(receiptId);
        share.OwnerId.Should().Be(ownerId);
        share.SharedWithUserId.Should().Be(sharedWithUserId);
        share.SharedAt.Should().Be(sharedAt);
    }

    [Fact]
    public void ReceiptShare_WithShareNote_ShouldStoreNote()
    {
        // Arrange
        var shareNote = "Sharing warranty info for the laptop we bought together";
        var share = new ReceiptShare
        {
            ReceiptId = Guid.NewGuid(),
            OwnerId = "owner-id",
            SharedWithUserId = "recipient-id",
            ShareNote = shareNote
        };

        // Act & Assert
        share.ShareNote.Should().Be(shareNote);
    }

    [Fact]
    public void ReceiptShare_WithoutShareNote_ShouldBeNull()
    {
        // Arrange & Act
        var share = new ReceiptShare
        {
            ReceiptId = Guid.NewGuid(),
            OwnerId = "owner-id",
            SharedWithUserId = "recipient-id"
        };

        // Assert
        share.ShareNote.Should().BeNull();
    }

    [Fact]
    public void ReceiptShare_ShareNote_ShouldAcceptNull()
    {
        // Arrange & Act
        var share = new ReceiptShare
        {
            ReceiptId = Guid.NewGuid(),
            OwnerId = "owner-id",
            SharedWithUserId = "recipient-id",
            ShareNote = null
        };

        // Assert
        share.ShareNote.Should().BeNull();
    }

    [Fact]
    public void ReceiptShare_ShareNote_ShouldAcceptLongText()
    {
        // Arrange
        var longNote = new string('A', 500); // Max length is 500
        var share = new ReceiptShare
        {
            ReceiptId = Guid.NewGuid(),
            OwnerId = "owner-id",
            SharedWithUserId = "recipient-id",
            ShareNote = longNote
        };

        // Act & Assert
        share.ShareNote.Should().Be(longNote);
        share.ShareNote.Should().HaveLength(500);
    }

    [Fact]
    public void ReceiptShare_ShouldSupportNavigationProperties()
    {
        // Arrange
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = "owner-id",
            FileName = "receipt.jpg",
            FileType = "image/jpeg",
            FileSizeBytes = 1024,
            StoragePath = "/uploads/receipt.jpg"
        };

        var owner = new ApplicationUser
        {
            Id = "owner-id",
            UserName = "owner@example.com",
            Email = "owner@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var recipient = new ApplicationUser
        {
            Id = "recipient-id",
            UserName = "recipient@example.com",
            Email = "recipient@example.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Act
        var share = new ReceiptShare
        {
            ReceiptId = receipt.Id,
            Receipt = receipt,
            OwnerId = owner.Id,
            Owner = owner,
            SharedWithUserId = recipient.Id,
            SharedWithUser = recipient
        };

        // Assert
        share.Receipt.Should().NotBeNull();
        share.Receipt.Id.Should().Be(receipt.Id);
        share.Receipt.FileName.Should().Be("receipt.jpg");

        share.Owner.Should().NotBeNull();
        share.Owner.Id.Should().Be(owner.Id);
        share.Owner.Email.Should().Be("owner@example.com");

        share.SharedWithUser.Should().NotBeNull();
        share.SharedWithUser.Id.Should().Be(recipient.Id);
        share.SharedWithUser.Email.Should().Be("recipient@example.com");
    }

    [Fact]
    public void ReceiptShare_MultipleShares_ShouldHaveUniqueIds()
    {
        // Arrange & Act
        var share1 = new ReceiptShare
        {
            ReceiptId = Guid.NewGuid(),
            OwnerId = "owner-id",
            SharedWithUserId = "recipient1-id"
        };

        var share2 = new ReceiptShare
        {
            ReceiptId = Guid.NewGuid(),
            OwnerId = "owner-id",
            SharedWithUserId = "recipient2-id"
        };

        // Assert
        share1.Id.Should().NotBe(share2.Id);
    }

    [Fact]
    public void ReceiptShare_SameReceiptDifferentRecipients_ShouldBeDistinct()
    {
        // Arrange
        var receiptId = Guid.NewGuid();
        var ownerId = "owner-id";

        // Act
        var share1 = new ReceiptShare
        {
            ReceiptId = receiptId,
            OwnerId = ownerId,
            SharedWithUserId = "recipient1-id"
        };

        var share2 = new ReceiptShare
        {
            ReceiptId = receiptId,
            OwnerId = ownerId,
            SharedWithUserId = "recipient2-id"
        };

        // Assert
        share1.ReceiptId.Should().Be(share2.ReceiptId);
        share1.OwnerId.Should().Be(share2.OwnerId);
        share1.SharedWithUserId.Should().NotBe(share2.SharedWithUserId);
        share1.Id.Should().NotBe(share2.Id);
    }
}
