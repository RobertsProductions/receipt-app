using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyApi.HealthChecks;

public class FileStorageHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageHealthCheck> _logger;

    public FileStorageHealthCheck(
        IConfiguration configuration,
        ILogger<FileStorageHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var uploadPath = _configuration["FileStorage:UploadPath"] ?? "uploads/receipts";
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), uploadPath);

            // Check if directory exists
            if (!Directory.Exists(fullPath))
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Upload directory does not exist: {fullPath}"));
            }

            // Test write permissions
            var testFile = Path.Combine(fullPath, $".health_check_{Guid.NewGuid()}.tmp");
            try
            {
                File.WriteAllText(testFile, "health check test");
                File.Delete(testFile);
            }
            catch (UnauthorizedAccessException)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"No write permission to upload directory: {fullPath}"));
            }

            // Check available disk space
            var drive = new DriveInfo(Path.GetPathRoot(fullPath)!);
            var availableSpaceGB = drive.AvailableFreeSpace / (1024.0 * 1024 * 1024);

            var data = new Dictionary<string, object>
            {
                { "uploadPath", fullPath },
                { "availableSpaceGB", Math.Round(availableSpaceGB, 2) }
            };

            if (availableSpaceGB < 1)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Low disk space: {Math.Round(availableSpaceGB, 2)} GB available",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"File storage healthy. Available space: {Math.Round(availableSpaceGB, 2)} GB",
                data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file storage health");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Error checking file storage.",
                exception: ex));
        }
    }
}
