using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Document
    {
        [Key]
        [Column("Document_Id")]
        public int DocumentId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("File_Url")]
        public string FileUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("File_Type")]
        public string FileType { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty; // Kimlik, Poliçe, Talep, vb.

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Version { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // Active, Archived, Deleted

        [Column("Uploaded_At")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Column("Expires_At")]
        public DateTime? ExpiresAt { get; set; }

        [Column("Updated_At")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        [Column("Customer_Id")]
        public int? CustomerId { get; set; }

        [Column("Claim_Id")]
        public int? ClaimId { get; set; }

        [Column("Policy_Id")]
        public int? PolicyId { get; set; }

        [Column("Uploaded_By_User_Id")]
        public int? UploadedByUserId { get; set; }

        [Column("User_Id")]
        public int? UserId { get; set; }

        // Navigation properties
        public Customer? Customer { get; set; }
        public Claim? Claim { get; set; }
        public Policy? Policy { get; set; }
        public User UploadedByUser { get; set; } = null!;
        public User? User { get; set; }
    }
}
