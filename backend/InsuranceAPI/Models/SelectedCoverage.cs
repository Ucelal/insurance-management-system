using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.Models
{
    public class SelectedCoverage
    {
        public int Id { get; set; }
        
        [Required]
        public int OfferId { get; set; }
        
        [Required]
        public int CoverageId { get; set; }
        
        [Required]
        public decimal SelectedLimit { get; set; }
        
        [Required]
        public decimal Premium { get; set; }
        
        public bool IsSelected { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Offer Offer { get; set; } = null!;
        public virtual Coverage Coverage { get; set; } = null!;
    }
}
