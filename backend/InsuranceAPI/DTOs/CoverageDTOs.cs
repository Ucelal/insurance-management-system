using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.DTOs
{
    public class CoverageDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Limit { get; set; }
        public decimal Premium { get; set; }
        public bool IsOptional { get; set; }
        public bool IsActive { get; set; }
        public int InsuranceTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public InsuranceTypeDto? InsuranceType { get; set; }
    }
    
    public class CreateCoverageDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public decimal Limit { get; set; }
        
        [Required]
        public decimal Premium { get; set; }
        
        public bool IsOptional { get; set; } = false;
        
        [Required]
        public int InsuranceTypeId { get; set; }
    }
    
    public class UpdateCoverageDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public decimal? Limit { get; set; }
        
        public decimal? Premium { get; set; }
        
        public bool? IsOptional { get; set; }
        
        public bool? IsActive { get; set; }
        
        public int? InsuranceTypeId { get; set; }
    }
}
