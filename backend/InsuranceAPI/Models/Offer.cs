using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Offer
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int AgentId { get; set; }
        
        // Sigorta Bilgileri
        [Required]
        public int InsuranceTypeId { get; set; }
        
        // Teklif Detayları
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountRate { get; set; } = 0; // Yüzde olarak
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalPrice { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "pending"; // pending, approved, rejected, expired
        
        public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(30);
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
        
        [ForeignKey("AgentId")]
        public virtual Agent? Agent { get; set; }
        
        [ForeignKey("InsuranceTypeId")]
        public virtual InsuranceType InsuranceType { get; set; } = null!;
        
        public virtual Policy? Policy { get; set; }
        
        public virtual List<SelectedCoverage> SelectedCoverages { get; set; } = new();
    }
}
