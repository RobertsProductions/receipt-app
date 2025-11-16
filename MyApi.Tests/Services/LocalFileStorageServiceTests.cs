using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyApi.Services;
using Xunit;

namespace MyApi.Tests.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly LocalFileStorageService _storageService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<LocalFileStorageService>> _mockLogger;
    private readonly string _testStoragePath;

    public LocalFileStorageServiceTests()
    {
        // Create a temporary test directory
        _testStoragePath = Path.Combine(Path.GetTempPath(), "WarrantyAppTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testStoragePath);

        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["FileStorage:BasePath"]).Returns(_testStoragePath);

        _mockLogger = new Mock<ILogger<LocalFileStorageService>>();

        _storageService = new LocalFileStorageService(_mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SaveFileAsync_WithValidFile_SavesFileAndReturnsPath()
    {
        // Arrange
        var userId = "test-user-id";
        var fileName = "test-receipt.jpg";
        var fileContent = "Test file content"u8.ToArray();

        var mockFile = CreateMockFormFile(fileName, fileContent);

        // Act
        var storagePath = await _storageService.SaveFileAsync(mockFile, userId);

        // Assert
        storagePath.Should().NotBeNullOrEmpty();
        storagePath.Should().StartWith(userId);
        storagePath.Should().EndWith(".jpg");

        var fullPath = Path.Combine(_testStoragePath, storagePath);
        File.Exists(fullPath).Should().BeTrue();

        var savedContent = await File.ReadAllBytesAsync(fullPath);
        savedContent.Should().BeEquivalentTo(fileContent);
    }

    [Fact]
    public async Task SaveFileAsync_CreatesUserDirectoryIfNotExists()
    {
        // Arrange
        var userId = "new-user-id";
        var mockFile = CreateMockFormFile("test.jpg", "content"u8.ToArray());

        var userDirectory = Path.Combine(_testStoragePath, userId);
        Directory.Exists(userDirectory).Should().BeFalse();

        // Act
        await _storageService.SaveFileAsync(mockFile, userId);

        // Assert
        Directory.Exists(userDirectory).Should().BeTrue();
    }

    [Fact]
    public async Task SaveFileAsync_GeneratesUniqueFileNames()
    {
        // Arrange
        var userId = "test-user-id";
        var mockFile1 = CreateMockFormFile("receipt.jpg", "content1"u8.ToArray());
        var mockFile2 = CreateMockFormFile("receipt.jpg", "content2"u8.ToArray());

        // Act
        var path1 = await _storageService.SaveFileAsync(mockFile1, userId);
        var path2 = await _storageService.SaveFileAsync(mockFile2, userId);

        // Assert
        path1.Should().NotBe(path2);

        File.Exists(Path.Combine(_testStoragePath, path1)).Should().BeTrue();
        File.Exists(Path.Combine(_testStoragePath, path2)).Should().BeTrue();
    }

    [Fact]
    public async Task SaveFileAsync_PreservesFileExtension()
    {
        // Arrange
        var userId = "test-user-id";
        var mockPdfFile = CreateMockFormFile("document.pdf", "pdf content"u8.ToArray());
        var mockJpgFile = CreateMockFormFile("image.jpg", "jpg content"u8.ToArray());

        // Act
        var pdfPath = await _storageService.SaveFileAsync(mockPdfFile, userId);
        var jpgPath = await _storageService.SaveFileAsync(mockJpgFile, userId);

        // Assert
        pdfPath.Should().EndWith(".pdf");
        jpgPath.Should().EndWith(".jpg");
    }

    [Fact]
    public async Task GetFileAsync_WithExistingFile_ReturnsFileStream()
    {
        // Arrange
        var userId = "test-user-id";
        var fileContent = "Test file content"u8.ToArray();
        var mockFile = CreateMockFormFile("test.jpg", fileContent);
        var storagePath = await _storageService.SaveFileAsync(mockFile, userId);

        // Act
        var stream = await _storageService.GetFileAsync(storagePath);

        // Assert
        stream.Should().NotBeNull();
        stream.Position.Should().Be(0);

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.ToArray().Should().BeEquivalentTo(fileContent);
    }

    [Fact]
    public async Task GetFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "user-id/non-existent-file.jpg";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _storageService.GetFileAsync(nonExistentPath));
    }

    [Fact]
    public async Task DeleteFileAsync_WithExistingFile_DeletesFile()
    {
        // Arrange
        var userId = "test-user-id";
        var mockFile = CreateMockFormFile("test.jpg", "content"u8.ToArray());
        var storagePath = await _storageService.SaveFileAsync(mockFile, userId);
        var fullPath = Path.Combine(_testStoragePath, storagePath);

        File.Exists(fullPath).Should().BeTrue();

        // Act
        await _storageService.DeleteFileAsync(storagePath);

        // Assert
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_WithNonExistentFile_DoesNotThrow()
    {
        // Arrange
        var nonExistentPath = "user-id/non-existent-file.jpg";

        // Act & Assert - should not throw
        await _storageService.DeleteFileAsync(nonExistentPath);
    }

    [Fact]
    public async Task FileExistsAsync_WithExistingFile_ReturnsTrue()
    {
        // Arrange
        var userId = "test-user-id";
        var mockFile = CreateMockFormFile("test.jpg", "content"u8.ToArray());
        var storagePath = await _storageService.SaveFileAsync(mockFile, userId);

        // Act
        var exists = await _storageService.FileExistsAsync(storagePath);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task FileExistsAsync_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var nonExistentPath = "user-id/non-existent-file.jpg";

        // Act
        var exists = await _storageService.FileExistsAsync(nonExistentPath);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task SaveFileAsync_IsolatesUserFiles()
    {
        // Arrange
        var user1 = "user-1";
        var user2 = "user-2";
        var mockFile1 = CreateMockFormFile("receipt.jpg", "user1 content"u8.ToArray());
        var mockFile2 = CreateMockFormFile("receipt.jpg", "user2 content"u8.ToArray());

        // Act
        var path1 = await _storageService.SaveFileAsync(mockFile1, user1);
        var path2 = await _storageService.SaveFileAsync(mockFile2, user2);

        // Assert
        path1.Should().StartWith(user1);
        path2.Should().StartWith(user2);

        var fullPath1 = Path.Combine(_testStoragePath, path1);
        var fullPath2 = Path.Combine(_testStoragePath, path2);

        Directory.GetParent(fullPath1)!.Name.Should().Be(user1);
        Directory.GetParent(fullPath2)!.Name.Should().Be(user2);
    }

    private IFormFile CreateMockFormFile(string fileName, byte[] content)
    {
        var stream = new MemoryStream(content);
        var mockFile = new Mock<IFormFile>();

        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(content.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream target, CancellationToken token) =>
            {
                stream.Position = 0;
                return stream.CopyToAsync(target, token);
            });

        return mockFile.Object;
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testStoragePath))
        {
            Directory.Delete(_testStoragePath, true);
        }
    }
}
