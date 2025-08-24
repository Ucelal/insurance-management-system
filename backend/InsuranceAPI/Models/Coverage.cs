using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.Models
{
    public class Coverage
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // "Hastane yatışı", "Ameliyat"
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public decimal Limit { get; set; } // Teminat limiti
        
        [Required]
        public decimal Premium { get; set; } // Ek prim
        
        public bool IsOptional { get; set; } = false; // Opsiyonel mi?
        
        public bool IsActive { get; set; } = true;
        
        [Required]
        public int InsuranceTypeId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual InsuranceType InsuranceType { get; set; } = null!;
        public virtual List<SelectedCoverage> SelectedCoverages { get; set; } = new();
    }
}
