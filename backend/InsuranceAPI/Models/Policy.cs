using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Policy
    {
        public int Id { get; set; }
        
        [Required]
        public int OfferId { get; set; } // Teklif ID'si
        
        [Required]
        public DateTime StartDate { get; set; } // Başlangıç tarihi
        
        [Required]
        public DateTime EndDate { get; set; } // Bitiş tarihi
        
        [Required]
        [MaxLength(100)]
        public string PolicyNumber { get; set; } = string.Empty; // Poliçe numarası
        
        // Navigation property
        [ForeignKey("OfferId")]
        public virtual Offer? Offer { get; set; }
        
        // One-to-many relationship with Claims
        public virtual ICollection<Claim>? Claims { get; set; }
        
        // One-to-many relationship with Payments
        public virtual ICollection<Payment>? Payments { get; set; }
    }
}
