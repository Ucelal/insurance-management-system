using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.DTOs
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int? PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UserId { get; set; }
        public PolicyDto? Policy { get; set; }
    }

    public class CreatePaymentDto
    {
        [Required]
        public int PolicyId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır")]
        public decimal Amount { get; set; }

        [Required]
        public string Method { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdatePaymentDto
    {
        public string? Status { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
    }

    public class PaymentStatisticsDto
    {
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public int PendingPayments { get; set; }
        public int FailedPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessfulAmount { get; set; }
        public Dictionary<string, int> PaymentsByMethod { get; set; } = new();
        public Dictionary<string, int> PaymentsByStatus { get; set; } = new();
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
    }

    public class PaymentSearchDto
    {
        public string? Status { get; set; }
        public string? Method { get; set; }
        public int? PolicyId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }

    public class ProcessPaymentDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
    }
}
