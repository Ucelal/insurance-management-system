using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Payment
    {
        [Key]
        [Column("Payment_Id")]
        public int PaymentId { get; set; }

        [Required]
        [Column("Amount")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Method { get; set; } = string.Empty; // Nakit, Kredi Kartı, Banka Transferi

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // Pending, Completed, Failed, Refunded

        [MaxLength(100)]
        public string? TransactionId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Column("Paid_At")]
        public DateTime? PaidAt { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Updated_At")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        [Column("Policy_Id")]
        public int? PolicyId { get; set; }

        [Column("User_Id")]
        public int? UserId { get; set; }

        // Navigation properties
        public Policy Policy { get; set; } = null!;
    }
}



