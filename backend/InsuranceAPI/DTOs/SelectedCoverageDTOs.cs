namespace InsuranceAPI.DTOs
{
    public class SelectedCoverageDto
    {
        public int Id { get; set; }
        public int OfferId { get; set; }
        public int CoverageId { get; set; }
        public decimal SelectedLimit { get; set; }
        public decimal Premium { get; set; }
        public bool IsSelected { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public CoverageDto? Coverage { get; set; }
    }
    
    public class CreateSelectedCoverageDto
    {
        public int CoverageId { get; set; }
        public decimal SelectedLimit { get; set; }
        public decimal Premium { get; set; }
        public bool IsSelected { get; set; } = true;
    }
    
    public class UpdateSelectedCoverageDto
    {
        public decimal? SelectedLimit { get; set; }
        public decimal? Premium { get; set; }
        public bool? IsSelected { get; set; }
    }
}
