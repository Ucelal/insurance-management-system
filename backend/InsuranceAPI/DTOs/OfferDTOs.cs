using InsuranceAPI.Models;

namespace InsuranceAPI.DTOs
{
    public class OfferDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int AgentId { get; set; }
        public int InsuranceTypeId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal FinalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public CustomerDto? Customer { get; set; }
        public AgentDto? Agent { get; set; }
        public InsuranceTypeDto? InsuranceType { get; set; }
        public List<SelectedCoverageDto> SelectedCoverages { get; set; } = new();
    }
    
    public class CreateOfferDto
    {
        public int CustomerId { get; set; }
        public int AgentId { get; set; }
        public int InsuranceTypeId { get; set; }
        public string? Description { get; set; }
        public decimal? DiscountRate { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
    
    public class UpdateOfferDto
    {
        public int? InsuranceTypeId { get; set; }
        public string? Description { get; set; }
        public decimal? DiscountRate { get; set; }
        public string? Status { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}
