using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.DTOs
{
    public class AgentDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string AgentCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        
        // Navigation properties
        public UserDto? User { get; set; }
    }
    
    public class CreateAgentDto
    {
        [Required]
        public int UserId { get; set; }
        
        // AgentCode artık departmana göre otomatik oluşturulacak
        
        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
    }
    
    public class UpdateAgentDto
    {
        // User bilgileri
        [MaxLength(255)]
        public string? Name { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }
        
        [MinLength(6)]
        public string? Password { get; set; }
        
        // Agent bilgileri
        [MaxLength(10)]
        public string? AgentCode { get; set; }
        
        [MaxLength(100)]
        public string? Department { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
    }
}
