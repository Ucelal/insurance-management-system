using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.DTOs
{
    public class SalesReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalPolicies { get; set; }
        public decimal TotalPremium { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalAgents { get; set; }
        public List<SalesByAgentDto> SalesByAgent { get; set; } = new();
        public List<SalesByMonthDto> SalesByMonth { get; set; } = new();
        public List<SalesByInsuranceTypeDto> SalesByInsuranceType { get; set; } = new();
    }
    
    public class SalesByAgentDto
    {
        public int? AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string AgentCode { get; set; } = string.Empty;
        public int PoliciesSold { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal Commission { get; set; }
    }
    
    public class SalesByMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int PoliciesSold { get; set; }
        public decimal TotalPremium { get; set; }
    }
    
    public class SalesByInsuranceTypeDto
    {
        public string InsuranceType { get; set; } = string.Empty;
        public int PoliciesSold { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal AveragePremium { get; set; }
    }
    
    public class ClaimsReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public int ResolvedClaims { get; set; }
        public decimal TotalClaimAmount { get; set; }
        public decimal AverageClaimAmount { get; set; }
        public List<ClaimsByStatusDto> ClaimsByStatus { get; set; } = new();
        public List<ClaimsByTypeDto> ClaimsByType { get; set; } = new();
        public List<ClaimsByMonthDto> ClaimsByMonth { get; set; } = new();
    }
    
    public class ClaimsByStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }
    
    public class ClaimsByTypeDto
    {
        public string ClaimType { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
    }
    
    public class ClaimsByMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int ClaimsCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
    
    public class CustomerReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int InactiveCustomers { get; set; }
        public List<CustomersByTypeDto> CustomersByType { get; set; } = new();
        public List<CustomersByRegionDto> CustomersByRegion { get; set; } = new();
        public List<CustomerRetentionDto> CustomerRetention { get; set; } = new();
    }
    
    public class CustomersByTypeDto
    {
        public string CustomerType { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
    
    public class CustomersByRegionDto
    {
        public string Region { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
    
    public class CustomerRetentionDto
    {
        public int Year { get; set; }
        public int RetainedCustomers { get; set; }
        public int TotalCustomers { get; set; }
        public decimal RetentionRate { get; set; }
    }
    
    public class PaymentReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public int PendingPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessfulAmount { get; set; }
        public decimal FailedAmount { get; set; }
        public List<PaymentsByMethodDto> PaymentsByMethod { get; set; } = new();
        public List<PaymentsByMonthDto> PaymentsByMonth { get; set; } = new();
    }
    
    public class PaymentsByMethodDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }
    
    public class PaymentsByMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int PaymentsCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
    
    public class ReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CustomerId { get; set; }
        public int? AgentId { get; set; }
        public string? InsuranceType { get; set; }
        public string? Status { get; set; }
        public string? Department { get; set; }
    }
}
