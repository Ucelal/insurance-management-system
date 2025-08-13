using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Offer
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; } // Müşteri ID'si
        
        [Required]
        [MaxLength(100)]
        public string InsuranceType { get; set; } = string.Empty; // Sigorta türü
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Fiyat
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // Durum (pending, approved, cancelled)
        
        // Navigation property
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
        
        // One-to-one relationship with Policy
        public virtual Policy? Policy { get; set; }
    }
}
