using DigitalBank.Application.Dtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Domain.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUserContext _current;
        private readonly IFileStorageService _files;

        public ProfileService(UserManager<AppUser> userManager, ICurrentUserContext current, IFileStorageService files)
        {
            _userManager = userManager;
            _current = current;
            _files = files;
        }

        public async Task<ServiceResult<ProfileResponseDto>> GetMyProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResult<ProfileResponseDto>.Fail("Unauthorized", 401);

            var user = await _userManager.FindByIdAsync(_current.UserId);
            if (user == null)
                return ServiceResult<ProfileResponseDto>.Fail("User not found", 404);

            var dto = new ProfileResponseDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                FatherName = user.FatherName,
                Address = user.Address,
                Age = user.Age,
                AvatarUrl = user.AvatarUrl,
                CreatedDate = user.CreatedDate
            };

            return ServiceResult<ProfileResponseDto>.Ok(dto);
        }

        public async Task<ServiceResultVoid> UpdateMyProfileAsync(ProfileUpdateRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResultVoid.Fail("Unauthorized", 401);

            var user = await _userManager.FindByIdAsync(_current.UserId);
            if (user == null)
                return ServiceResultVoid.Fail("User not found", 404);

            // Frontend-dən gələn yeni sahələri mənimsədirik
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.FatherName = dto.FatherName; // Sənin AppUser-ində bu sahə var
            user.Address = dto.Address;       // Sənin AppUser-ində bu sahə var
            user.Age = dto.Age;               // Sənin AppUser-ində bu sahə var

            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
                return ServiceResultVoid.Fail(res.Errors.Select(e => e.Description).ToList(), "Update failed", 400);

            return ServiceResultVoid.Ok("Profile updated");
        }

        public async Task<ServiceResult<AvatarUploadResponseDto>> UploadAvatarAsync(IFormFile file, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResult<AvatarUploadResponseDto>.Fail("Unauthorized", 401);

            var user = await _userManager.FindByIdAsync(_current.UserId);
            if (user == null)
                return ServiceResult<AvatarUploadResponseDto>.Fail("User not found", 404);

            try
            {
                var newPath = await _files.SaveAvatarAsync(file, ct);

                if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
                    await _files.DeleteIfExistsAsync(user.AvatarUrl!, ct);

                user.AvatarUrl = newPath;

                var res = await _userManager.UpdateAsync(user);
                if (!res.Succeeded)
                    return ServiceResult<AvatarUploadResponseDto>.Fail(res.Errors.Select(e => e.Description).ToList(), "Avatar update failed", 400);

                return ServiceResult<AvatarUploadResponseDto>.Ok(new AvatarUploadResponseDto { AvatarUrl = newPath }, "Avatar updated");
            }
            catch (Exception ex)
            {
                return ServiceResult<AvatarUploadResponseDto>.Fail(ex.Message, 400);
            }
        }
        public async Task<ServiceResult<List<UserBriefDto>>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
                return ServiceResult<List<UserBriefDto>>.Ok(new List<UserBriefDto>());

            var users = await _userManager.Users
                .Where(u => u.Id != _current.UserId &&
                           (u.FirstName.Contains(query) ||
                            u.LastName.Contains(query) ||
                            u.Email.Contains(query)))
                .Take(10) // Performans üçün cəmi 10 nəfər gətiririk
                .Select(u => new UserBriefDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserName = u.UserName,
                    Email = u.Email
                })
                .ToListAsync();

            return ServiceResult<List<UserBriefDto>>.Ok(users);
        }

    }
}
