namespace InsuranceAPI.DTOs
{
    public class SelectedCoverageDto
    {
        public int SelectedCoverageId { get; set; } // Changed from Id to SelectedCoverageId
        public int? OfferId { get; set; }
        public int? CoverageId { get; set; }
        public decimal Premium { get; set; }
        public string? Notes { get; set; } // Changed from SelectedLimit to Notes
        
        // Navigation properties
        public CoverageDto? Coverage { get; set; }
    }
    
    public class CreateSelectedCoverageDto
    {
        public int CoverageId { get; set; }
        public decimal Premium { get; set; }
        public string? Notes { get; set; }
    }
    
    public class UpdateSelectedCoverageDto
    {
        public decimal? Premium { get; set; }
        public string? Notes { get; set; }
    }
}
