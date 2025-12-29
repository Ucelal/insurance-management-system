using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class Customer
    {
        [Key]
        [Column("Customer_Id")]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Id_No")]
        public string IdNo { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("User_Id")]
        public int? UserId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
} 