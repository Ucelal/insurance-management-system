using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int PolicyId { get; set; } // Poliçe ID'si

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Tutar

        public DateTime PaidAt { get; set; } = DateTime.UtcNow; // Ödeme tarihi

        [Required]
        public PaymentMethod Method { get; set; } = PaymentMethod.KrediKarti; // Ödeme yöntemi

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Beklemede; // Ödeme durumu

        [MaxLength(100)]
        public string? TransactionId { get; set; } // Banka işlem numarası

        [MaxLength(500)]
        public string? Notes { get; set; } // Notlar

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Oluşturulma tarihi

        public DateTime? UpdatedAt { get; set; } // Güncellenme tarihi

        [ForeignKey("PolicyId")]
        public virtual Policy? Policy { get; set; }
    }
}



