using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace InsuranceAPI.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly InsuranceDbContext _context;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string _uploadPath;
        private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public FileUploadService(InsuranceDbContext context, ILogger<FileUploadService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _uploadPath = configuration["FileUpload:Path"] ?? "wwwroot/uploads";
            
            // Upload dizinini oluştur
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<FileUploadResponseDto> UploadFileAsync(IFormFile file, FileUploadDto uploadDto, int uploadedByUserId)
        {
            try
            {
                // File validation
                if (!await ValidateFileAsync(file))
                {
                    throw new InvalidOperationException("Invalid file format or size");
                }

                // Customer validation
                var customer = await _context.Customers.FindAsync(uploadDto.CustomerId);
                if (customer == null)
                {
                    throw new InvalidOperationException($"Customer with ID {uploadDto.CustomerId} not found");
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(_uploadPath, fileName);
                var fileUrl = await GenerateFileUrlAsync(fileName, Path.GetExtension(file.FileName));

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create document record
                var document = new Document
                {
                    CustomerId = uploadDto.CustomerId,
                    ClaimId = uploadDto.ClaimId,
                    PolicyId = uploadDto.PolicyId,
                    FileName = file.FileName,
                    FileUrl = fileUrl,
                    FileType = Path.GetExtension(file.FileName).ToLowerInvariant(),
                    FileSize = file.Length,
                    Category = uploadDto.Category,
                    Description = uploadDto.Description,
                    Version = uploadDto.Version ?? "1.0",
                    Status = DocumentStatus.Aktif,
                    UploadedAt = DateTime.UtcNow,
                    ExpiresAt = uploadDto.ExpiresAt,
                    UploadedByUserId = uploadedByUserId
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("File uploaded successfully: {FileName} by user {UserId}", file.FileName, uploadedByUserId);

                // Return response
                return new FileUploadResponseDto
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    FileUrl = document.FileUrl,
                    FileType = document.FileType,
                    FileSize = document.FileSize,
                    Category = document.Category,
                    Status = document.Status,
                    Description = document.Description,
                    Version = document.Version,
                    UploadedAt = document.UploadedAt,
                    ExpiresAt = document.ExpiresAt,
                    CustomerId = document.CustomerId,
                    ClaimId = document.ClaimId,
                    PolicyId = document.PolicyId,
                    UploadedByUserId = document.UploadedByUserId,
                    UploadedByUserName = (await _context.Users.FindAsync(uploadedByUserId))?.Name ?? "Unknown"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file {FileName}", file.FileName);
                throw;
            }
        }

        public async Task<FileDownloadDto?> DownloadFileAsync(int documentId)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.UploadedByUser)
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                if (document == null)
                {
                    _logger.LogWarning("Document with ID {Id} not found for download", documentId);
                    return null;
                }

                var filePath = Path.Combine(_uploadPath, Path.GetFileName(document.FileUrl));
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found on disk: {FilePath}", filePath);
                    return null;
                }

                return new FileDownloadDto
                {
                    FileName = document.FileName,
                    FileUrl = document.FileUrl,
                    FileType = document.FileType,
                    FileSize = document.FileSize,
                    UploadedAt = document.UploadedAt,
                    ExpiresAt = document.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading document with ID {Id}", documentId);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(int documentId)
        {
            try
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document with ID {Id} not found for deletion", documentId);
                    return false;
                }

                // Delete file from disk
                var filePath = Path.Combine(_uploadPath, Path.GetFileName(document.FileUrl));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Delete from database
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document with ID {Id} deleted successfully", documentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting document with ID {Id}", documentId);
                throw;
            }
        }

        public async Task<FileUploadResponseDto?> UpdateFileMetadataAsync(int documentId, FileUpdateDto updateDto)
        {
            try
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document with ID {Id} not found for update", documentId);
                    return null;
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(updateDto.Description))
                    document.Description = updateDto.Description;
                if (!string.IsNullOrEmpty(updateDto.Version))
                    document.Version = updateDto.Version;
                if (updateDto.Category.HasValue)
                    document.Category = updateDto.Category.Value;
                if (updateDto.Status.HasValue)
                    document.Status = updateDto.Status.Value;
                if (updateDto.ExpiresAt.HasValue)
                    document.ExpiresAt = updateDto.ExpiresAt.Value;

                document.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Document metadata updated for ID {Id}", documentId);

                // Return updated document
                return await GetFilesByCustomerAsync(document.CustomerId)
                    .ContinueWith(t => t.Result.FirstOrDefault(d => d.Id == documentId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating document metadata for ID {Id}", documentId);
                throw;
            }
        }

        public async Task<IEnumerable<FileUploadResponseDto>> GetFilesByCustomerAsync(int customerId)
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.CustomerId == customerId)
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();

                return documents.Select(d => new FileUploadResponseDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FileUrl = d.FileUrl,
                    FileType = d.FileType,
                    FileSize = d.FileSize,
                    Category = d.Category,
                    Status = d.Status,
                    Description = d.Description,
                    Version = d.Version,
                    UploadedAt = d.UploadedAt,
                    ExpiresAt = d.ExpiresAt,
                    CustomerId = d.CustomerId,
                    ClaimId = d.ClaimId,
                    PolicyId = d.PolicyId,
                    UploadedByUserId = d.UploadedByUserId,
                    UploadedByUserName = d.UploadedByUser?.Name ?? "Unknown"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving files for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<FileUploadResponseDto>> GetFilesByClaimAsync(int claimId)
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.ClaimId == claimId)
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();

                return documents.Select(d => new FileUploadResponseDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FileUrl = d.FileUrl,
                    FileType = d.FileType,
                    FileSize = d.FileSize,
                    Category = d.Category,
                    Status = d.Status,
                    Description = d.Description,
                    Version = d.Version,
                    UploadedAt = d.UploadedAt,
                    ExpiresAt = d.ExpiresAt,
                    CustomerId = d.CustomerId,
                    ClaimId = d.ClaimId,
                    PolicyId = d.PolicyId,
                    UploadedByUserId = d.UploadedByUserId,
                    UploadedByUserName = d.UploadedByUser?.Name ?? "Unknown"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving files for claim {ClaimId}", claimId);
                throw;
            }
        }

        public async Task<IEnumerable<FileUploadResponseDto>> GetFilesByPolicyAsync(int policyId)
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.PolicyId == policyId)
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();

                return documents.Select(d => new FileUploadResponseDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    FileUrl = d.FileUrl,
                    FileType = d.FileType,
                    FileSize = d.FileSize,
                    Category = d.Category,
                    Status = d.Status,
                    Description = d.Description,
                    Version = d.Version,
                    UploadedAt = d.UploadedAt,
                    ExpiresAt = d.ExpiresAt,
                    CustomerId = d.CustomerId,
                    ClaimId = d.ClaimId,
                    PolicyId = d.PolicyId,
                    UploadedByUserId = d.UploadedByUserId,
                    UploadedByUserName = d.UploadedByUser?.Name ?? "Unknown"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving files for policy {PolicyId}", policyId);
                throw;
            }
        }

        public async Task<bool> IsFileAccessibleAsync(int documentId, int userId, string userRole)
        {
            try
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document == null) return false;

                // Admin can access all files
                if (userRole == "admin") return true;

                // Agent can access files from their customers
                if (userRole == "agent")
                {
                    var customer = await _context.Customers
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.Id == document.CustomerId);
                    
                    if (customer?.User != null)
                    {
                        var agent = await _context.Agents
                            .FirstOrDefaultAsync(a => a.UserId == userId);
                        
                        if (agent != null)
                        {
                            // Check if agent has access to this customer
                            // This logic can be enhanced based on business rules
                            return true;
                        }
                    }
                }

                // Customer can only access their own files
                if (userRole == "customer")
                {
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.UserId == userId);
                    
                    return customer?.Id == document.CustomerId;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking file access for document {DocumentId} and user {UserId}", documentId, userId);
                return false;
            }
        }

        public Task<string> GenerateFileUrlAsync(string fileName, string fileType)
        {
            // Generate a unique URL for the file
            var baseUrl = "https://localhost:5000/uploads/";
            return Task.FromResult($"{baseUrl}{fileName}");
        }

        public Task<bool> ValidateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Task.FromResult(false);

            if (file.Length > _maxFileSize)
                return Task.FromResult(false);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
    }
}
