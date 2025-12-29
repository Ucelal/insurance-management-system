using InsuranceAPI.DTOs;
using InsuranceAPI.Models;

namespace InsuranceAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        
        // Customer kayıt işlemi - özel validasyon ve iş mantığı
        Task<AuthResponseDto?> RegisterCustomerAsync(CustomerRegisterDto customerRegisterDto);
        
        // Agent kayıt işlemi - özel validasyon ve iş mantığı
        Task<AuthResponseDto?> RegisterAgentAsync(AgentRegisterDto agentRegisterDto);
        
        // Admin kayıt işlemi - özel validasyon ve iş mantığı
        Task<AuthResponseDto?> RegisterAdminAsync(AdminRegisterDto adminRegisterDto);
        
        Task<bool> ValidateTokenAsync(string token);
        Task<UserDto?> GetUserFromTokenAsync(string token);
        Task<User?> GetUserByEmailAsync(string email);
    }
} 