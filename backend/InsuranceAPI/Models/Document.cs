using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Document
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        public int? ClaimId { get; set; }
        
        public int? PolicyId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string FileUrl { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string FileType { get; set; } = string.Empty; // pdf, jpg, png, doc, etc.
        
        [Required]
        public long FileSize { get; set; } // bytes
        
        [Required]
        public DocumentCategory Category { get; set; } = DocumentCategory.Diger;
        
        [Required]
        public DocumentStatus Status { get; set; } = DocumentStatus.Aktif;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(100)]
        public string? Version { get; set; } = "1.0";
        
        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
        
        [Required]
        public int UploadedByUserId { get; set; }
        
        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
        
        [ForeignKey("ClaimId")]
        public virtual Claim? Claim { get; set; }
        
        [ForeignKey("PolicyId")]
        public virtual Policy? Policy { get; set; }
        
        [ForeignKey("UploadedByUserId")]
        public virtual User? UploadedByUser { get; set; }
    }
}
