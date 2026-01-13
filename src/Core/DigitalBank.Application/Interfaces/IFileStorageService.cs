using Microsoft.AspNetCore.Http;

namespace DigitalBank.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAvatarAsync(IFormFile file, CancellationToken ct = default);
        Task DeleteIfExistsAsync(string relativePath, CancellationToken ct = default);

    }
}
