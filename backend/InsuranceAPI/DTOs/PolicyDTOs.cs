using InsuranceAPI.Models;

namespace InsuranceAPI.DTOs
{
    public class PolicyDto
    {
        public int Id { get; set; }
        public int OfferId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public OfferDto? Offer { get; set; }
    }
    
    public class CreatePolicyDto
    {
        public int OfferId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
    }
    
    public class UpdatePolicyDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
    }
}
