using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.DTOs
{
    // Kullanıcı profil bilgilerini güncelleme
    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
    }
    
    // Şifre değiştirme
    public class ChangePasswordDto
    {
        [Required]
        public string currentPassword { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string newPassword { get; set; } = string.Empty;
        
        [Required]
        [Compare("newPassword")]
        public string confirmNewPassword { get; set; } = string.Empty;
    }
    
    // Müşteri profil bilgilerini güncelleme
    public class UpdateCustomerProfileDto
    {
        [MaxLength(1000)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
    }
    
    // Agent profil bilgilerini güncelleme
    public class UpdateAgentProfileDto
    {
        [MaxLength(1000)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
    }
    
    // Kullanıcı profil bilgilerini getirme
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public CustomerProfileDto? Customer { get; set; }
        public AgentProfileDto? Agent { get; set; }
    }
    
    // Müşteri profil bilgileri
    public class CustomerProfileDto
    {
        public int CustomerId { get; set; }
        public string IdNo { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }
    
    // Agent profil bilgileri
    public class AgentProfileDto
    {
        public int AgentId { get; set; }
        public string AgentCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }
    
    // Profil güncelleme yanıtı
    public class ProfileUpdateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserProfileDto? User { get; set; }
    }
}
