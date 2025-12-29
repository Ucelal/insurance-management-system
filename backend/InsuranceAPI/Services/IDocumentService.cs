using InsuranceAPI.DTOs;

namespace InsuranceAPI.Services
{
    public interface IDocumentService
    {
        // CRUD Operations
        Task<List<DocumentDto>> GetAllDocumentsAsync();
        Task<DocumentDto?> GetDocumentByIdAsync(int id);
        Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDto, int uploadedByUserId);
        Task<DocumentDto?> UpdateDocumentAsync(int id, UpdateDocumentDto updateDto);
        Task<bool> DeleteDocumentAsync(int id);
        
        // Query Operations
        Task<List<DocumentDto>> GetDocumentsByCustomerAsync(int customerId);
        Task<List<DocumentDto>> GetDocumentsByClaimAsync(int claimId);
        Task<List<DocumentDto>> GetDocumentsByPolicyAsync(int policyId);
        Task<List<DocumentDto>> GetDocumentsByCategoryAsync(string category);
        Task<List<DocumentDto>> GetDocumentsByStatusAsync(string status);
        Task<List<DocumentDto>> GetDocumentsByFileTypeAsync(string fileType);
        
        // Search and Filter
        Task<List<DocumentDto>> SearchDocumentsAsync(DocumentSearchDto searchDto);
        Task<DocumentStatisticsDto> GetDocumentStatisticsAsync();
        
        // File Operations
        Task<DocumentUploadResponseDto> UploadDocumentAsync(CreateDocumentDto createDto, int uploadedByUserId);
        Task<bool> UpdateDocumentStatusAsync(int id, string status);
        Task<bool> ArchiveDocumentAsync(int id);
        Task<bool> RestoreDocumentAsync(int id);
        
        // Utility Methods
        Task<List<string>> GetDocumentCategoriesAsync();
        Task<List<string>> GetDocumentStatusesAsync();
        Task<List<string>> GetSupportedFileTypesAsync();
        Task<bool> IsFileTypeSupportedAsync(string fileType);
        Task<long> GetTotalStorageUsedAsync();
        
        // Policy Document Creation
        Task<DocumentDto> CreatePolicyDocumentAsync(int policyId, int customerId, int uploadedByUserId);
    }
}
