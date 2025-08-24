using InsuranceAPI.Models;

namespace InsuranceAPI.DTOs
{
    public class PolicyDto
    {
        public int Id { get; set; }
        public int OfferId { get; set; }
        
        // Poliçe Detayları
        public string PolicyNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPremium { get; set; }
        public string Status { get; set; } = string.Empty;
        
        // Ödeme Bilgileri
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        
        // Ek Bilgiler
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public OfferDto? Offer { get; set; }
    }
    
    public class CreatePolicyDto
    {
        public int OfferId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public decimal TotalPremium { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }
    }
    
    public class UpdatePolicyDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PolicyNumber { get; set; }
        public decimal? TotalPremium { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? Notes { get; set; }
    }
}
