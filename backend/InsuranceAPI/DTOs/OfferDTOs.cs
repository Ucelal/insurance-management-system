using InsuranceAPI.Models;

namespace InsuranceAPI.DTOs
{
    public class OfferDto
    {
        public int OfferId { get; set; } // Changed from Id to OfferId
        public int? CustomerId { get; set; }
        public int? AgentId { get; set; } // Made nullable
        public int? InsuranceTypeId { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal FinalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Yeni alanlar
        public bool IsCustomerApproved { get; set; }
        public DateTime? CustomerApprovedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedBy { get; set; }
        public string? CustomerAdditionalInfo { get; set; }
        public decimal? CoverageAmount { get; set; } // Changed from string to decimal?
        public DateTime? RequestedStartDate { get; set; }
        public string Department { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public string? RejectionReason { get; set; }
        public int? CreatedBy { get; set; }
        
        // Flat properties for easier access
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;
        public string IdNo { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string AgentEmail { get; set; } = string.Empty;
        public string AgentPhone { get; set; } = string.Empty;
        public string AgentAddress { get; set; } = string.Empty;
        public string AgentCode { get; set; } = string.Empty;
        public int AgentUserId { get; set; }
        public string InsuranceTypeName { get; set; } = string.Empty;
        public string InsuranceTypeCategory { get; set; } = string.Empty;
        public string? PolicyPdfUrl { get; set; }
        
        // Navigation properties
        public List<SelectedCoverageDto> SelectedCoverages { get; set; } = new();
        public CustomerDto? Customer { get; set; }
        public InsuranceTypeDto? InsuranceType { get; set; }
    }
    
    public class CreateOfferDto
    {
        public int CustomerId { get; set; }
        public int? AgentId { get; set; } // Nullable yapıldı (müşteri teklif talebi için)
        public int InsuranceTypeId { get; set; }
        public decimal BasePrice { get; set; } // Added
        public decimal DiscountRate { get; set; } // Added
        public decimal FinalPrice { get; set; } // Added
        public string Status { get; set; } = "Pending"; // Added
        public DateTime? ValidUntil { get; set; }
        // Ek alanlar
        public string? Department { get; set; }
        public decimal? CoverageAmount { get; set; } // Changed from string to decimal?
        public DateTime? RequestedStartDate { get; set; }
        public string? CustomerAdditionalInfo { get; set; }
        public int? CreatedBy { get; set; }
    }
    
    public class UpdateOfferDto
    {
        public int? InsuranceTypeId { get; set; }
        public decimal? BasePrice { get; set; } // Added
        public decimal? DiscountRate { get; set; }
        public decimal? FinalPrice { get; set; } // Added
        public string? Status { get; set; }
        public DateTime ValidUntil { get; set; }
 // Added
        public decimal? CoverageAmount { get; set; } // Added
    }
    
    // Müşteri teklif talebi için
    public class CustomerQuoteRequestDto
    {
        public string ServiceType { get; set; } = string.Empty;
        public string CoverageAmount { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string? AdditionalInfo { get; set; }
    }
    
    // Acente teklif düzenleme için
    public class AgentQuoteUpdateDto
    {
        public decimal? FinalPrice { get; set; }
        public string Status { get; set; } = "reviewed";
        public DateTime? ValidUntil { get; set; }
    }
    
    // Müşteri onay için
    public class CustomerQuoteApprovalDto
    {
        public bool IsApproved { get; set; }
    }
    
    // Teklif onayı güncelleme için
    public class OfferApprovalDto
    {
        public bool IsCustomerApproved { get; set; }
        public DateTime? CustomerApprovedAt { get; set; }
    }
    
    // Ödeme sonrası poliçe oluşturma için
    public class CreatePolicyFromPaymentDto
    {
        public decimal PaymentAmount { get; set; }
        public string PaymentMethod { get; set; } = "Kredi Kartı";
        public string? TransactionId { get; set; }
        public string? CardLast4 { get; set; }
    }
    
    // Ödeme makbuzu için
    public class PaymentReceiptDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentDate { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public int OfferId { get; set; }
        public string InsuranceType { get; set; } = string.Empty;
        public decimal CoverageAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public string CardName { get; set; } = string.Empty;
        public string CardLast4 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = "Başarılı";
    }
}
