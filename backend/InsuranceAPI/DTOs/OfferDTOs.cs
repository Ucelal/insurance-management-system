using InsuranceAPI.Models;

namespace InsuranceAPI.DTOs
{
    public class OfferDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string InsuranceType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public CustomerDto? Customer { get; set; }
    }
    
    public class CreateOfferDto
    {
        public int CustomerId { get; set; }
        public string InsuranceType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = "pending";
    }
    
    public class UpdateOfferDto
    {
        public string InsuranceType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
