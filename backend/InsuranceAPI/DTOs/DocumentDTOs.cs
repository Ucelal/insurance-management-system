using System.ComponentModel.DataAnnotations;

namespace InsuranceAPI.DTOs
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? ClaimId { get; set; }
        public int? PolicyId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? UploadedByUserId { get; set; }
        public string UploadedByUserName { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? PolicyNumber { get; set; }
        public string? ClaimNumber { get; set; }
    }
    
    public class CreateDocumentDto
    {
        public int? CustomerId { get; set; }
        
        public int? ClaimId { get; set; }
        
        public int? PolicyId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string FileUrl { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string FileType { get; set; } = string.Empty;
        
        [Required]
        public long FileSize { get; set; }
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public string Version { get; set; } = "1.0";
        
        public DateTime? ExpiresAt { get; set; }
    }
    
    public class UpdateDocumentDto
    {
        [MaxLength(255)]
        public string? FileName { get; set; }
        
        [MaxLength(1000)]
        public string? FileUrl { get; set; }
        
        [MaxLength(100)]
        public string? FileType { get; set; }
        
        public long? FileSize { get; set; }
        
        public string? Category { get; set; }
        
        public string? Status { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public string? Version { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
    }
    
    public class DocumentSearchDto
    {
        public int? CustomerId { get; set; }
        public int? ClaimId { get; set; }
        public int? PolicyId { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public string? FileType { get; set; }
        public DateTime? UploadedFrom { get; set; }
        public DateTime? UploadedTo { get; set; }
        public string? FileNameContains { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
    
    public class DocumentStatisticsDto
    {
        public int TotalDocuments { get; set; }
        public int ActiveDocuments { get; set; }
        public int ArchivedDocuments { get; set; }
        public long TotalFileSize { get; set; }
        public Dictionary<string, int> DocumentsByCategory { get; set; } = new();
        public Dictionary<string, int> DocumentsByFileType { get; set; } = new();
        public Dictionary<string, int> DocumentsByMonth { get; set; } = new();
        public Dictionary<string, int> DocumentsByStatus { get; set; } = new();
    }
    
    public class DocumentUploadResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DocumentDto? Document { get; set; }
        public string? Error { get; set; }
    }
}
