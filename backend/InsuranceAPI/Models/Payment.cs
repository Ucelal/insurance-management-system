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
        [MaxLength(50)]
        public string Method { get; set; } = "nakit"; // Ödeme yöntemi

        [ForeignKey("PolicyId")]
        public virtual Policy? Policy { get; set; }
    }
}



