using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.DTOs
{
    // Hasar listesi için DTO
    public class ClaimDto
    {
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public int? ProcessedByUserId { get; set; }
        public string? ProcessedByUserName { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public decimal? ClaimAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? EstimatedResolutionDate { get; set; }
        public string? Notes { get; set; }
        
        // Navigation DTOs
        public PolicyDto? Policy { get; set; }
        public UserDto? CreatedByUser { get; set; }
        public UserDto? ProcessedByUser { get; set; }
    }
    
    // Yeni hasar oluşturma için DTO
    public class CreateClaimDto
    {
        [Required]
        public int PolicyId { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        public string Priority { get; set; } = string.Empty;
        
        [Range(0, double.MaxValue)]
        public decimal? ClaimAmount { get; set; }
        
        public DateTime? EstimatedResolutionDate { get; set; }
    }
    
    // Hasar güncelleme için DTO
    public class UpdateClaimDto
    {
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? Priority { get; set; }
        public decimal? ClaimAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public DateTime? EstimatedResolutionDate { get; set; }
        public string? Notes { get; set; }
    }
    
    // Hasar istatistikleri için DTO
    public class ClaimStatisticsDto
    {
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int InReviewClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public int ResolvedClaims { get; set; }
        public int ClosedClaims { get; set; }
        public decimal TotalClaimAmount { get; set; }
        public decimal TotalApprovedAmount { get; set; }
        public Dictionary<string, int> ClaimsByType { get; set; } = new();
        public Dictionary<string, int> ClaimsByPriority { get; set; } = new();
        public Dictionary<string, int> ClaimsByMonth { get; set; } = new();
    }
    
    // Hasar arama için DTO
    public class ClaimSearchDto
    {
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? Priority { get; set; }
        public int? PolicyId { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}
