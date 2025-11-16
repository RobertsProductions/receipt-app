namespace MyApi.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string userId);
    Task<Stream> GetFileAsync(string storagePath);
    Task DeleteFileAsync(string storagePath);
    Task<bool> FileExistsAsync(string storagePath);
}
