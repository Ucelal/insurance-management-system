using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.DTOs
{
    public class InsuranceTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public decimal BasePrice { get; set; }
        public string CoverageDetails { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public List<CoverageDto> Coverages { get; set; } = new();
    }
    
    public class CreateInsuranceTypeDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public decimal BasePrice { get; set; }
        
        [MaxLength(2000)]
        public string CoverageDetails { get; set; } = string.Empty;
    }
    
    public class UpdateInsuranceTypeDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        
        [MaxLength(50)]
        public string? Category { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public decimal? BasePrice { get; set; }
        
        [MaxLength(2000)]
        public string? CoverageDetails { get; set; }
        
        public bool? IsActive { get; set; }
    }
}
