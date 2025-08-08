using InsuranceAPI.DTOs;
using InsuranceAPI.Models;

namespace InsuranceAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
            Task<bool> ValidateTokenAsync(string token);
    Task<UserDto?> GetUserFromTokenAsync(string token);
    Task<User?> GetUserByEmailAsync(string email);
    }
} 