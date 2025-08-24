using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InsuranceAPI.Models;

namespace InsuranceAPI.Models
{
    public class Claim
    {
        public int Id { get; set; }
        
        [Required]
        public int PolicyId { get; set; }
        
        [Required]
        public int CreatedByUserId { get; set; } // Hasarı bildiren kullanıcı
        
        public int? ProcessedByUserId { get; set; } // İşleyen kullanıcı (admin/agent)
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
        
        [Required]
        public ClaimType Type { get; set; } = ClaimType.Diger;
        
        [Required]
        public ClaimPriority Priority { get; set; } = ClaimPriority.Normal;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ClaimAmount { get; set; } // Hasar tutarı
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ApprovedAmount { get; set; } // Onaylanan tutar
        
        public DateTime? EstimatedResolutionDate { get; set; } // Tahmini çözüm tarihi
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ProcessedAt { get; set; } // İşlem tarihi
        
        [MaxLength(1000)]
        public string? Notes { get; set; } // Admin/agent notları
        
        // Navigation properties
        [ForeignKey("PolicyId")]
        public virtual Policy? Policy { get; set; }
        
        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedByUser { get; set; }
        
        [ForeignKey("ProcessedByUserId")]
        public virtual User? ProcessedByUser { get; set; }
    }
}



