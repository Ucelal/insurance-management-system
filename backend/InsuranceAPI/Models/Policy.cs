using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Policy
    {
        public int Id { get; set; }
        
        [Required]
        public int OfferId { get; set; }
        
        // Poliçe Detayları
        [Required]
        [MaxLength(100)]
        public string PolicyNumber { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPremium { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "active"; // active, expired, cancelled, suspended
        
        // Ödeme Bilgileri
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // credit_card, bank_transfer, cash
        
        public DateTime? PaidAt { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "pending"; // pending, paid, failed, refunded
        
        // Ek Bilgiler
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("OfferId")]
        public virtual Offer? Offer { get; set; }
        
        public virtual List<Claim>? Claims { get; set; }
        public virtual List<Payment>? Payments { get; set; }
    }
}
