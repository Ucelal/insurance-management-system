using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Customer
    {
        public int Id { get; set; }
        
        public int? UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // "bireysel" veya "kurumsal"
        
        [Required]
        [MaxLength(50)]
        public string IdNo { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
} 