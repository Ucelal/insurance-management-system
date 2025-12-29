using System.ComponentModel.DataAnnotations;
using InsuranceAPI.Models;

namespace InsuranceAPI.DTOs
{
    public class FileUploadDto
    {
        public int? CustomerId { get; set; }
        
        public int? ClaimId { get; set; }
        
        public int? PolicyId { get; set; }
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(100)]
        public string? Version { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
    }
    
    public class FileUploadResponseDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Version { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? CustomerId { get; set; }
        public int? ClaimId { get; set; }
        public int? PolicyId { get; set; }
        public int? UploadedByUserId { get; set; }
        public string UploadedByUserName { get; set; } = string.Empty;
        public int? UserId { get; set; }
    }
    
    public class FileDownloadDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
    
    public class FileUpdateDto
    {
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(100)]
        public string? Version { get; set; }
        
        public string? Category { get; set; }
        
        public string? Status { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
    }
}
