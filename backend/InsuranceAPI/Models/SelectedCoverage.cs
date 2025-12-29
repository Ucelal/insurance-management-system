using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class SelectedCoverage
    {
        [Key]
        [Column("Sel_Cov_Id")]
        public int SelectedCoverageId { get; set; }

        [Column("Offer_Id")]
        public int? OfferId { get; set; }

        [Column("Coverage_Id")]
        public int? CoverageId { get; set; }

        [Column("User_Id")]
        public int? UserId { get; set; }

        [Required]
        [Column("Selected_Limit", TypeName = "decimal(18,2)")]
        public decimal SelectedLimit { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Premium { get; set; }

        [Required]
        public bool IsSelected { get; set; } = true;

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        public Offer Offer { get; set; } = null!;
        public Coverage Coverage { get; set; } = null!;
    }
}
