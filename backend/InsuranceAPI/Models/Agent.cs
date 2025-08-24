using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Agent
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(10)]
        public string AgentCode { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        public virtual ICollection<Offer>? Offers { get; set; }
    }
}
