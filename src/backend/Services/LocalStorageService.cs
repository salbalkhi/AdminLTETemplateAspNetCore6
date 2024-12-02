using Microsoft.AspNetCore.Http;

namespace Tadawi.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _uploadDirectory;
    private readonly string _baseUrl;

    public LocalStorageService(IConfiguration configuration)
    {
        _uploadDirectory = configuration["Storage:UploadDirectory"] ?? "wwwroot/uploads";
        _baseUrl = configuration["Storage:BaseUrl"] ?? "http://localhost:5000/uploads";
        
        // Ensure upload directory exists
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }
    }

    public async Task<(string storagePath, string publicUrl)> SaveFileAsync(IFormFile file, string prefix)
    {
        // Generate a unique filename
        var fileName = $"{prefix}_{DateTime.UtcNow.Ticks}_{Path.GetFileName(file.FileName)}";
        var relativePath = Path.Combine(prefix, fileName);
        var fullPath = Path.Combine(_uploadDirectory, relativePath);
        var publicUrl = $"{_baseUrl}/{relativePath.Replace("\\", "/")}";

        // Ensure directory exists
        var directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        // Save the file
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return (relativePath, publicUrl);
    }

    public Task DeleteFileAsync(string storagePath)
    {
        var fullPath = Path.Combine(_uploadDirectory, storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }
}
