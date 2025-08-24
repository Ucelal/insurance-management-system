using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string IdNo { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public UserDto? User { get; set; }
    }
    
    public class CreateCustomerDto
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string IdNo { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
    }
    
    public class UpdateCustomerDto
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string IdNo { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
    }
    
    // Toplu güncelleme için DTO
    public class BulkUpdateCustomerDto
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }
    
    // Müşteri istatistikleri için DTO
    public class CustomerStatisticsDto
    {
        public int TotalCustomers { get; set; }
        public int IndividualCustomers { get; set; }
        public int CorporateCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int InactiveCustomers { get; set; }
        public Dictionary<string, int> CustomersByType { get; set; } = new();
        public Dictionary<string, int> CustomersByMonth { get; set; } = new();
    }
    
    // Müşteri aktivite için DTO
    public class CustomerActivityDto
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? UserName { get; set; }
    }
}