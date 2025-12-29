using InsuranceAPI.DTOs;

namespace InsuranceAPI.Services
{
    public interface IProfileService
    {
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<ProfileUpdateResponseDto> UpdateUserProfileAsync(int userId, UpdateProfileDto updateDto);
        Task<ProfileUpdateResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<ProfileUpdateResponseDto> UpdateCustomerProfileAsync(int userId, UpdateCustomerProfileDto updateDto);
        Task<ProfileUpdateResponseDto> UpdateAgentProfileAsync(int userId, UpdateAgentProfileDto updateDto);
    }
}
