using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Customer
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // bireysel, kurumsal
        

        
        [Required]
        [MaxLength(50)]
        public string IdNo { get; set; } = string.Empty; // TC kimlik, vergi no
        
        [MaxLength(1000)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        public virtual ICollection<Offer>? Offers { get; set; }
        public virtual ICollection<Document>? Documents { get; set; }
    }
} 