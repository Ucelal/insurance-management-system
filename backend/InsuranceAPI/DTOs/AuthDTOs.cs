using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
    
    public class RegisterDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = "Customer";
    }
    
    // Customer kayıt için özel DTO
    public class CustomerRegisterDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(11)]
        public string TcNo { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        [Attributes.Phone]
        public string Phone { get; set; } = string.Empty;
        
        public string CustomerType { get; set; } = "bireysel"; // bireysel, kurumsal
    }
    
    // Agent kayıt için özel DTO
    public class AgentRegisterDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(10)]
        public string AgentCode { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        [Attributes.Phone]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;
    }
    
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = new();
    }
    
    // Admin paneli için DTO
    public class UpdateUserRoleDto
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
} 