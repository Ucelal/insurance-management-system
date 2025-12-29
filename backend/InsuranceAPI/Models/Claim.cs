using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Claim
    {
        [Key]
        [Column("Claim_Id")]
        public int ClaimId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // ArabaKazası, Hırsızlık, DoğalAfet, vb.

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // Pending, UnderReview, Approved, Rejected, Closed

        [Column("Incident_Date")]
        public DateTime IncidentDate { get; set; }

        [Column("Processed_At")]
        public DateTime? ProcessedAt { get; set; }

        [Column("Approved_Amount")]
        public decimal? ApprovedAmount { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Updated_At")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        [Column("Policy_Id")]
        public int? PolicyId { get; set; }

        [Column("Created_By_User_Id")]
        public int? CreatedByUserId { get; set; }

        [Column("Processed_By_User_Id")]
        public int? ProcessedByUserId { get; set; }

        // Navigation properties
        public Policy? Policy { get; set; }
        public User CreatedByUser { get; set; } = null!;
        public User? ProcessedByUser { get; set; }
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}



