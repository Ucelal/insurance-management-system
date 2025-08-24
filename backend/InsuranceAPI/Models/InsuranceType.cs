using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.Models
{
    public class InsuranceType
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // "Sağlık", "Kasko", "Deprem"
        
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // "Sağlık", "Araç", "Konut", "Hayat"
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        [Required]
        public decimal BasePrice { get; set; }
        
        [MaxLength(2000)]
        public string CoverageDetails { get; set; } = string.Empty; // JSON formatında
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual List<Coverage> Coverages { get; set; } = new();
        public virtual List<Offer> Offers { get; set; } = new();
    }
}
