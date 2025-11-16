namespace MyApi.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageBasePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _storageBasePath = configuration["FileStorage:BasePath"] 
            ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads", "receipts");
        
        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
            _logger.LogInformation("Created storage directory: {Path}", _storageBasePath);
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, string userId)
    {
        var userDirectory = Path.Combine(_storageBasePath, userId);
        if (!Directory.Exists(userDirectory))
        {
            Directory.CreateDirectory(userDirectory);
        }

        var fileId = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var safeFileName = $"{fileId}{extension}";
        var filePath = Path.Combine(userDirectory, safeFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = Path.Combine(userId, safeFileName);
        _logger.LogInformation("Saved file to: {Path}", relativePath);
        
        return relativePath;
    }

    public async Task<Stream> GetFileAsync(string storagePath)
    {
        var fullPath = Path.Combine(_storageBasePath, storagePath);
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Receipt file not found", storagePath);
        }

        var memoryStream = new MemoryStream();
        using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
        {
            await fileStream.CopyToAsync(memoryStream);
        }
        memoryStream.Position = 0;
        
        return memoryStream;
    }

    public Task DeleteFileAsync(string storagePath)
    {
        var fullPath = Path.Combine(_storageBasePath, storagePath);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted file: {Path}", storagePath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string storagePath)
    {
        var fullPath = Path.Combine(_storageBasePath, storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }
}
