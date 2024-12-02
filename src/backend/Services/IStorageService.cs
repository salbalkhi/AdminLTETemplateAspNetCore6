using Microsoft.AspNetCore.Http;

namespace Tadawi.Services;

public interface IStorageService
{
    Task<(string storagePath, string publicUrl)> SaveFileAsync(IFormFile file, string prefix);
    Task DeleteFileAsync(string storagePath);
}
