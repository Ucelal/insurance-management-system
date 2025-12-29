using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class InsuranceType
    {
        [Key]
        [Column("Ins_Type_Id")]
        public int InsuranceTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        [Required]
        [Column("Validity_Period_Days")]
        public int ValidityPeriodDays { get; set; } = 30; // Geçerlilik süresi (gün)

        [MaxLength(2000)]
        [Column("Coverage_Details")]
        public string? CoverageDetails { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Column("User_Id")]
        public int? UserId { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Updated_At")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Coverage> Coverages { get; set; } = new List<Coverage>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }
}
