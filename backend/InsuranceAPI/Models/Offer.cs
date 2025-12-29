using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Offer
    {
        [Key]
        [Column("Offer_Id")]
        public int OfferId { get; set; }


        [Column("Base_Price")]
        public decimal BasePrice { get; set; }

        [Column("Final_Price")]
        public decimal FinalPrice { get; set; }

        [Column("Discount_Rate")]
        public decimal DiscountRate { get; set; }

        [Column("Coverage_Amount")]
        public decimal? CoverageAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Expired

        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;


        [Column("Customer_Additional_Info")]
        [MaxLength(1000)]
        public string? CustomerAdditionalInfo { get; set; }

        [Column("Requested_Start_Date")]
        public DateTime? RequestedStartDate { get; set; }

        [Column("Valid_Until")]
        public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(30);

        [Column("Is_Customer_Approved")]
        public bool IsCustomerApproved { get; set; } = false;

        [Column("Customer_Approved_At")]
        public DateTime? CustomerApprovedAt { get; set; }

        [Column("Reviewed_At")]
        public DateTime? ReviewedAt { get; set; }

        [Column("Reviewed_By")]
        public int? ReviewedBy { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Updated_At")]
        public DateTime? UpdatedAt { get; set; }

        [Column("Policy_Pdf_Url")]
        [MaxLength(500)]
        public string? PolicyPdfUrl { get; set; }



        [Column("Admin_Notes")]
        [MaxLength(1000)]
        public string? AdminNotes { get; set; }

        [Column("Rejection_Reason")]
        [MaxLength(1000)]
        public string? RejectionReason { get; set; }


        // Foreign keys
        [Column("Customer_Id")]
        public int? CustomerId { get; set; }

        [Column("Agent_Id")]
        public int? AgentId { get; set; }

        [Column("Insurance_Type_Id")]
        public int? InsuranceTypeId { get; set; }

        [Column("Created_By")]
        public int? CreatedBy { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public Agent? Agent { get; set; }
        public Agent? ReviewedByAgent { get; set; }
        public InsuranceType InsuranceType { get; set; } = null!;
        public ICollection<SelectedCoverage> SelectedCoverages { get; set; } = new List<SelectedCoverage>();
        public ICollection<Policy> Policies { get; set; } = new List<Policy>();
    }
}
