using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceAPI.Models
{
    public class TokenBlacklist
    {
        [Key]
        [Column("Token_black_Id")]
        public int TokenBlacklistId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Token { get; set; } = string.Empty;

        [Column("Blacklisted_At")]
        public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;

        [Column("Expires_At")]
        public DateTime? ExpiresAt { get; set; }

        [MaxLength(255)]
        [Column("User_Email")]
        public string? UserEmail { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
