using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Coverage
    {
        [Key]
        [Column("Coverage_Id")]
        public int CoverageId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Limit { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Premium { get; set; }

        [Required]
        public bool IsOptional { get; set; } = false;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // Zorunlu, Opsiyonel

        [Column("Base_Premium")]
        public decimal BasePremium { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Column("Insurance_Type_Id")]
        public int? InsuranceTypeId { get; set; }

        [Column("User_Id")]
        public int? UserId { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Updated_At")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public InsuranceType InsuranceType { get; set; } = null!;
        public ICollection<SelectedCoverage> SelectedCoverages { get; set; } = new List<SelectedCoverage>();
    }
}
