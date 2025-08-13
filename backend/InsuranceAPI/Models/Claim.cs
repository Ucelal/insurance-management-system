using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Claim
    {
        public int Id { get; set; }

        [Required]
        public int PolicyId { get; set; } // Poliçe ID'si

        public string? Description { get; set; } // Açıklama

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "pending"; // Durum

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Oluşturulma tarihi

        [ForeignKey("PolicyId")]
        public virtual Policy? Policy { get; set; }
    }
}



