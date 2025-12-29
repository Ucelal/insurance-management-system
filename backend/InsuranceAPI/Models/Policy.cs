using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Policy
    {
        [Key]
        [Column("Policy_Id")]
        public int PolicyId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Policy_Number")]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        [Column("Start_Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("End_Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("Total_Premium")]
        public decimal TotalPremium { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Active, Expired, Cancelled, Suspended

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Updated_At")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        [Column("Offer_Id")]
        public int? OfferId { get; set; }

        [Column("User_Id")]
        public int? UserId { get; set; }

        [Column("Ins_Type_Id")]
        public int? InsuranceTypeId { get; set; }

        [Column("Agent_Id")]
        public int? AgentId { get; set; }

        // Navigation properties
        public Offer Offer { get; set; } = null!;
        public InsuranceType? InsuranceType { get; set; }
        public Agent? Agent { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
