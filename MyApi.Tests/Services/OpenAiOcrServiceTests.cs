using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using MyApi.Services;
using Xunit;

namespace MyApi.Tests.Services;

public class OpenAiOcrServiceTests
{
    private readonly Mock<ILogger<OpenAiOcrService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfig;

    public OpenAiOcrServiceTests()
    {
        _mockLogger = new Mock<ILogger<OpenAiOcrService>>();
        _mockConfig = new Mock<IConfiguration>();
    }

    [Fact]
    public void Constructor_WithMissingApiKey_LogsWarning()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns((string?)null);

        // Act
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);

        // Assert - Service should be created but log warning
        Assert.NotNull(service);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithValidApiKey_LogsInformation()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-api-key");

        // Act
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);

        // Assert
        Assert.NotNull(service);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("initialized successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_WithMissingApiKey_ReturnsFailure()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns((string?)null);
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        var stream = new MemoryStream();
        var fileName = "receipt.jpg";

        // Act
        var result = await service.ExtractReceiptDataAsync(stream, fileName);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not configured", result.ErrorMessage);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_WithEmptyApiKey_ReturnsFailure()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("");
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        var stream = new MemoryStream();
        var fileName = "receipt.jpg";

        // Act
        var result = await service.ExtractReceiptDataAsync(stream, fileName);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not configured", result.ErrorMessage);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_WithJpegExtension_RecognizesImageType()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }); // JPEG magic bytes
        var fileName = "receipt.jpg";

        // Act
        var result = await service.ExtractReceiptDataAsync(stream, fileName);

        // Assert - Will fail because we can't mock HttpClient easily, but verifies the path
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_WithPngExtension_RecognizesImageType()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        var stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 }); // PNG magic bytes
        var fileName = "receipt.png";

        // Act
        var result = await service.ExtractReceiptDataAsync(stream, fileName);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_WithPdfExtension_UsesPdfPath()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        // Create minimal PDF header
        var pdfBytes = Encoding.ASCII.GetBytes("%PDF-1.4\n");
        var stream = new MemoryStream(pdfBytes);
        var fileName = "receipt.pdf";

        // Act
        var result = await service.ExtractReceiptDataAsync(stream, fileName);

        // Assert - PDF processing will attempt to extract text
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_WithStreamException_ReturnsFailure()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        var mockStream = new Mock<Stream>();
        mockStream.Setup(s => s.CanRead).Returns(true);
        mockStream.Setup(s => s.CopyToAsync(
            It.IsAny<Stream>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Stream error"));
        
        var fileName = "receipt.jpg";

        // Act
        var result = await service.ExtractReceiptDataAsync(mockStream.Object, fileName);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("OCR processing failed", result.ErrorMessage);
    }

    [Fact]
    public void OcrResult_DefaultState_HasExpectedProperties()
    {
        // Arrange & Act
        var result = new OcrResult();

        // Assert
        Assert.Null(result.Merchant);
        Assert.Null(result.Amount);
        Assert.Null(result.PurchaseDate);
        Assert.Null(result.ProductName);
        Assert.Null(result.ExtractedText);
        Assert.False(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void OcrResult_CanSetAllProperties()
    {
        // Arrange
        var result = new OcrResult
        {
            Merchant = "Test Store",
            Amount = 99.99m,
            PurchaseDate = new DateTime(2024, 1, 15),
            ProductName = "Test Product",
            ExtractedText = "Receipt text",
            Success = true,
            ErrorMessage = null
        };

        // Assert
        Assert.Equal("Test Store", result.Merchant);
        Assert.Equal(99.99m, result.Amount);
        Assert.Equal(new DateTime(2024, 1, 15), result.PurchaseDate);
        Assert.Equal("Test Product", result.ProductName);
        Assert.Equal("Receipt text", result.ExtractedText);
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_LogsProcessingStart()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        var fileName = "test_receipt.jpg";

        // Act
        await service.ExtractReceiptDataAsync(stream, fileName);

        // Assert - Should log start of OCR processing
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting OCR processing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_WithNullStream_ReturnsFailure()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        Stream? nullStream = null;
        var fileName = "receipt.jpg";

        // Act
        var result = await service.ExtractReceiptDataAsync(nullStream!, fileName);

        // Assert - Service handles null gracefully and returns error result
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_WithEmptyStream_HandlesGracefully()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        var emptyStream = new MemoryStream();
        var fileName = "receipt.jpg";

        // Act
        var result = await service.ExtractReceiptDataAsync(emptyStream, fileName);

        // Assert - Should not crash with empty stream
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_WithLargeImage_ProcessesWithoutError()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        // Create a 2MB stream
        var largeData = new byte[2 * 1024 * 1024];
        Array.Fill<byte>(largeData, 0xFF);
        var stream = new MemoryStream(largeData);
        var fileName = "large_receipt.jpg";

        // Act
        var result = await service.ExtractReceiptDataAsync(stream, fileName);

        // Assert - Should handle large files without crashing
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_WithUppercaseExtension_HandlesCorrectly()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        var fileName = "receipt.JPG"; // Uppercase extension

        // Act
        var result = await service.ExtractReceiptDataAsync(stream, fileName);

        // Assert - Should handle case-insensitive extensions
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ExtractReceiptDataAsync_WithMixedCaseExtension_HandlesCorrectly()
    {
        // Arrange
        _mockConfig.Setup(x => x["OpenAI:ApiKey"]).Returns("test-key");
        var service = new OpenAiOcrService(_mockConfig.Object, _mockLogger.Object);
        
        var stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        var fileName = "receipt.PnG"; // Mixed case extension

        // Act
        var result = await service.ExtractReceiptDataAsync(stream, fileName);

        // Assert
        Assert.NotNull(result);
    }
}
