using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.DTOs
{
    // Hasar listesi için DTO
    public class ClaimDto
    {
        public int ClaimId { get; set; }
        public int? PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public int? CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string? CreatedByUserEmail { get; set; }
        public int? ProcessedByUserId { get; set; }
        public string? ProcessedByUserName { get; set; }
        public string? ProcessedByUserEmail { get; set; }
        public string? ProcessedByUserPhone { get; set; }
        public int? UserId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal? ApprovedAmount { get; set; }
        public DateTime? IncidentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
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
        
        // Olay tarihi
        public DateTime? IncidentDate { get; set; }
        
        // İletişim bilgileri
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactAddress { get; set; }
        
        // Araç sigortası için ek alanlar
        public string? VehiclePlate { get; set; }
        public string? VehicleBrand { get; set; }
        public string? VehicleModel { get; set; }
        public string? VehicleYear { get; set; }
        public string? AccidentLocation { get; set; }
        public string? AccidentTime { get; set; }
        
        // Konut sigortası için ek alanlar
        public string? PropertyAddress { get; set; }
        public string? PropertyFloor { get; set; }
        public string? PropertyType { get; set; }
        
        // Sağlık sigortası için ek alanlar
        public string? PatientName { get; set; }
        public string? PatientAge { get; set; }
        public string? PatientGender { get; set; }
        public string? PatientTc { get; set; }
        public string? HospitalName { get; set; }
        public string? DoctorName { get; set; }
        
        // Seyahat sigortası için ek alanlar
        public string? TravelStartDate { get; set; }
        public string? TravelEndDate { get; set; }
        public string? TravelCountry { get; set; }
        public string? TravelPurpose { get; set; }
        
        // Hayat sigortası için ek alanlar
        public string? InsuredName { get; set; }
        public string? InsuredAge { get; set; }
        public string? InsuredGender { get; set; }
        public string? InsuredTc { get; set; }
        
        // İş yeri sigortası için ek alanlar
        public string? WorkplaceName { get; set; }
        public string? WorkplaceAddress { get; set; }
        public string? EmployeeCount { get; set; }
        public string? BusinessSector { get; set; }
        public string? SgkNumber { get; set; }
        public string? SafetyOfficer { get; set; }
        public string? EmergencyContact { get; set; }
    }
    
    // Hasar güncelleme için DTO
    public class UpdateClaimDto
    {
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public decimal? ApprovedAmount { get; set; }
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
        public Dictionary<string, int> ClaimsByMonth { get; set; } = new();
    }
    
    // Hasar arama için DTO
    public class ClaimSearchDto
    {
        public string? Status { get; set; }
        public string? Type { get; set; }
        public int? PolicyId { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
