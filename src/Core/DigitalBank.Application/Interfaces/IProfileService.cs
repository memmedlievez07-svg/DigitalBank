using DigitalBank.Application.Dtos;
using DigitalBank.Application.Results;
using Microsoft.AspNetCore.Http;

namespace DigitalBank.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ServiceResult<ProfileResponseDto>> GetMyProfileAsync();
        Task<ServiceResultVoid> UpdateMyProfileAsync(ProfileUpdateRequestDto dto);
        Task<ServiceResult<AvatarUploadResponseDto>> UploadAvatarAsync(IFormFile file, CancellationToken ct = default);
        Task<ServiceResult<List<UserBriefDto>>> SearchUsersAsync(string query);
    }
}
