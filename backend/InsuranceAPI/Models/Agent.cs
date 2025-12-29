using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Agent
    {
        [Key]
        [Column("Agent_Id")]
        public int AgentId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("Agent_Code")]
        public string AgentCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("User_Id")]
        public int? UserId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }
}
