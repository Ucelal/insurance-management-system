using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.DTOs;

namespace InsuranceAPI.Services
{
    public class ProfileService : IProfileService
    {
        private readonly InsuranceDbContext _context;

        public ProfileService(InsuranceDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Customer)
                    .Include(u => u.Agent)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return null;

                var userProfile = new UserProfileDto
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    Customer = user.Customer != null ? new CustomerProfileDto
                    {
                        CustomerId = user.Customer.CustomerId,
                        IdNo = user.Customer.IdNo,
                        Address = user.Customer.Address,
                        Phone = user.Customer.Phone
                    } : null,
                    Agent = user.Agent != null ? new AgentProfileDto
                    {
                        AgentId = user.Agent.AgentId,
                        AgentCode = user.Agent.AgentCode,
                        Department = user.Agent.Department,
                        Address = user.Agent.Address,
                        Phone = user.Agent.Phone
                    } : null
                };

                return userProfile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProfileService.GetUserProfileAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<ProfileUpdateResponseDto> UpdateUserProfileAsync(int userId, UpdateProfileDto updateDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ProfileUpdateResponseDto
                    {
                        Success = false,
                        Message = "Kullanıcı bulunamadı"
                    };
                }

                // Email kontrolü - başka kullanıcıda var mı?
                if (user.Email != updateDto.Email)
                {
                    var emailExists = await _context.Users
                        .AnyAsync(u => u.Email == updateDto.Email && u.UserId != userId);
                    
                    if (emailExists)
                    {
                        return new ProfileUpdateResponseDto
                        {
                            Success = false,
                            Message = "Bu email adresi başka bir kullanıcı tarafından kullanılıyor"
                        };
                    }
                }

                // Kullanıcı bilgilerini güncelle
                user.Name = updateDto.Name;
                user.Email = updateDto.Email;

                await _context.SaveChangesAsync();

                // Güncellenmiş profil bilgilerini getir
                var updatedProfile = await GetUserProfileAsync(userId);

                return new ProfileUpdateResponseDto
                {
                    Success = true,
                    Message = "Profil bilgileri başarıyla güncellendi",
                    User = updatedProfile
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProfileService.UpdateUserProfileAsync error: {ex.Message}");
                return new ProfileUpdateResponseDto
                {
                    Success = false,
                    Message = "Profil güncellenirken hata oluştu"
                };
            }
        }

        public async Task<ProfileUpdateResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ProfileUpdateResponseDto
                    {
                        Success = false,
                        Message = "Kullanıcı bulunamadı"
                    };
                }

                // Mevcut şifreyi kontrol et
                var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordDto.currentPassword, user.PasswordHash);
                if (!isCurrentPasswordValid)
                {
                    return new ProfileUpdateResponseDto
                    {
                        Success = false,
                        Message = "Mevcut şifre yanlış"
                    };
                }

                // Yeni şifreyi hash'le ve kaydet
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.newPassword);
                await _context.SaveChangesAsync();

                return new ProfileUpdateResponseDto
                {
                    Success = true,
                    Message = "Şifre başarıyla değiştirildi"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProfileService.ChangePasswordAsync error: {ex.Message}");
                return new ProfileUpdateResponseDto
                {
                    Success = false,
                    Message = "Şifre değiştirilirken hata oluştu"
                };
            }
        }

        public async Task<ProfileUpdateResponseDto> UpdateCustomerProfileAsync(int userId, UpdateCustomerProfileDto updateDto)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    return new ProfileUpdateResponseDto
                    {
                        Success = false,
                        Message = "Müşteri bilgileri bulunamadı"
                    };
                }

                // Müşteri bilgilerini güncelle
                if (!string.IsNullOrEmpty(updateDto.Address))
                    customer.Address = updateDto.Address;
                
                if (!string.IsNullOrEmpty(updateDto.Phone))
                    customer.Phone = updateDto.Phone;

                await _context.SaveChangesAsync();

                // Güncellenmiş profil bilgilerini getir
                var updatedProfile = await GetUserProfileAsync(userId);

                return new ProfileUpdateResponseDto
                {
                    Success = true,
                    Message = "Müşteri bilgileri başarıyla güncellendi",
                    User = updatedProfile
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProfileService.UpdateCustomerProfileAsync error: {ex.Message}");
                return new ProfileUpdateResponseDto
                {
                    Success = false,
                    Message = "Müşteri bilgileri güncellenirken hata oluştu"
                };
            }
        }

        public async Task<ProfileUpdateResponseDto> UpdateAgentProfileAsync(int userId, UpdateAgentProfileDto updateDto)
        {
            try
            {
                var agent = await _context.Agents
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (agent == null)
                {
                    return new ProfileUpdateResponseDto
                    {
                        Success = false,
                        Message = "Agent bilgileri bulunamadı"
                    };
                }

                // Agent bilgilerini güncelle
                if (!string.IsNullOrEmpty(updateDto.Address))
                    agent.Address = updateDto.Address;
                
                if (!string.IsNullOrEmpty(updateDto.Phone))
                    agent.Phone = updateDto.Phone;

                await _context.SaveChangesAsync();

                // Güncellenmiş profil bilgilerini getir
                var updatedProfile = await GetUserProfileAsync(userId);

                return new ProfileUpdateResponseDto
                {
                    Success = true,
                    Message = "Agent bilgileri başarıyla güncellendi",
                    User = updatedProfile
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProfileService.UpdateAgentProfileAsync error: {ex.Message}");
                return new ProfileUpdateResponseDto
                {
                    Success = false,
                    Message = "Agent bilgileri güncellenirken hata oluştu"
                };
            }
        }
    }
}
