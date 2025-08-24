using InsuranceAPI.DTOs;
using Microsoft.AspNetCore.Http;

namespace InsuranceAPI.Services
{
    public interface IFileUploadService
    {
        Task<FileUploadResponseDto> UploadFileAsync(IFormFile file, FileUploadDto uploadDto, int uploadedByUserId);
        Task<FileDownloadDto?> DownloadFileAsync(int documentId);
        Task<bool> DeleteFileAsync(int documentId);
        Task<FileUploadResponseDto?> UpdateFileMetadataAsync(int documentId, FileUpdateDto updateDto);
        Task<IEnumerable<FileUploadResponseDto>> GetFilesByCustomerAsync(int customerId);
        Task<IEnumerable<FileUploadResponseDto>> GetFilesByClaimAsync(int claimId);
        Task<IEnumerable<FileUploadResponseDto>> GetFilesByPolicyAsync(int policyId);
        Task<bool> IsFileAccessibleAsync(int documentId, int userId, string userRole);
        Task<string> GenerateFileUrlAsync(string fileName, string fileType);
        Task<bool> ValidateFileAsync(IFormFile file);
    }
}
