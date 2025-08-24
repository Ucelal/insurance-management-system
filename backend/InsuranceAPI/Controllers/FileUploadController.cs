using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;
using System.Security.Claims;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(IFileUploadService fileUploadService, ILogger<FileUploadController> logger)
        {
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        /// <summary>
        /// Dosya yükleme
        /// </summary>
        [HttpPost("upload")]
        [Authorize(Roles = "admin,agent,customer")]
        public async Task<ActionResult<FileUploadResponseDto>> UploadFile(
            [FromForm] IFormFile file,
            [FromForm] FileUploadDto uploadDto)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Dosya seçilmedi veya boş dosya");
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("Kullanıcı kimliği doğrulanamadı");
                }

                var result = await _fileUploadService.UploadFileAsync(file, uploadDto, userId.Value);
                
                _logger.LogInformation("File uploaded successfully: {FileName} by user {UserId}", file.FileName, userId);
                
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("File upload validation failed: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file {FileName}", file?.FileName);
                return StatusCode(500, "Dosya yükleme sırasında bir hata oluştu");
            }
        }

        /// <summary>
        /// Dosya indirme
        /// </summary>
        [HttpGet("download/{documentId}")]
        [Authorize(Roles = "admin,agent,customer")]
        public async Task<ActionResult<FileDownloadDto>> DownloadFile(int documentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                if (userId == null)
                {
                    return Unauthorized("Kullanıcı kimliği doğrulanamadı");
                }

                // Dosya erişim kontrolü
                var hasAccess = await _fileUploadService.IsFileAccessibleAsync(documentId, userId.Value, userRole);
                if (!hasAccess)
                {
                    return Forbid("Bu dosyaya erişim yetkiniz yok");
                }

                var result = await _fileUploadService.DownloadFileAsync(documentId);
                if (result == null)
                {
                    return NotFound("Dosya bulunamadı");
                }

                _logger.LogInformation("File downloaded successfully: Document ID {DocumentId} by user {UserId}", documentId, userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading document with ID {DocumentId}", documentId);
                return StatusCode(500, "Dosya indirme sırasında bir hata oluştu");
            }
        }

        /// <summary>
        /// Dosya silme
        /// </summary>
        [HttpDelete("{documentId}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult> DeleteFile(int documentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("Kullanıcı kimliği doğrulanamadı");
                }

                var result = await _fileUploadService.DeleteFileAsync(documentId);
                if (!result)
                {
                    return NotFound("Dosya bulunamadı");
                }

                _logger.LogInformation("File deleted successfully: Document ID {DocumentId} by user {UserId}", documentId, userId);
                
                return Ok(new { message = "Dosya başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting document with ID {DocumentId}", documentId);
                return StatusCode(500, "Dosya silme sırasında bir hata oluştu");
            }
        }

        /// <summary>
        /// Dosya metadata güncelleme
        /// </summary>
        [HttpPut("{documentId}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<FileUploadResponseDto>> UpdateFileMetadata(
            int documentId,
            [FromBody] FileUpdateDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("Kullanıcı kimliği doğrulanamadı");
                }

                var result = await _fileUploadService.UpdateFileMetadataAsync(documentId, updateDto);
                if (result == null)
                {
                    return NotFound("Dosya bulunamadı");
                }

                _logger.LogInformation("File metadata updated successfully: Document ID {DocumentId} by user {UserId}", documentId, userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating document metadata for ID {DocumentId}", documentId);
                return StatusCode(500, "Dosya metadata güncelleme sırasında bir hata oluştu");
            }
        }

        /// <summary>
        /// Müşteriye ait dosyaları getir
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "admin,agent,customer")]
        public async Task<ActionResult<IEnumerable<FileUploadResponseDto>>> GetFilesByCustomer(int customerId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                if (userId == null)
                {
                    return Unauthorized("Kullanıcı kimliği doğrulanamadı");
                }

                // Customer rolündeki kullanıcılar sadece kendi dosyalarını görebilir
                if (userRole == "customer")
                {
                    var currentCustomer = await GetCurrentCustomerId();
                    if (currentCustomer != customerId)
                    {
                        return Forbid("Bu müşterinin dosyalarına erişim yetkiniz yok");
                    }
                }

                var result = await _fileUploadService.GetFilesByCustomerAsync(customerId);
                
                _logger.LogInformation("Files retrieved for customer {CustomerId} by user {UserId}", customerId, userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving files for customer {CustomerId}", customerId);
                return StatusCode(500, "Dosyalar getirilirken bir hata oluştu");
            }
        }

        /// <summary>
        /// Talebe ait dosyaları getir
        /// </summary>
        [HttpGet("claim/{claimId}")]
        [Authorize(Roles = "admin,agent,customer")]
        public async Task<ActionResult<IEnumerable<FileUploadResponseDto>>> GetFilesByClaim(int claimId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                if (userId == null)
                {
                    return Unauthorized("Kullanıcı kimliği doğrulanamadı");
                }

                var result = await _fileUploadService.GetFilesByClaimAsync(claimId);
                
                _logger.LogInformation("Files retrieved for claim {ClaimId} by user {UserId}", claimId, userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving files for claim {ClaimId}", claimId);
                return StatusCode(500, "Dosyalar getirilirken bir hata oluştu");
            }
        }

        /// <summary>
        /// Poliçeye ait dosyaları getir
        /// </summary>
        [HttpGet("policy/{policyId}")]
        [Authorize(Roles = "admin,agent,customer")]
        public async Task<ActionResult<IEnumerable<FileUploadResponseDto>>> GetFilesByPolicy(int policyId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                if (userId == null)
                {
                    return Unauthorized("Kullanıcı kimliği doğrulanamadı");
                }

                var result = await _fileUploadService.GetFilesByPolicyAsync(policyId);
                
                _logger.LogInformation("Files retrieved for policy {PolicyId} by user {UserId}", policyId, userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving files for policy {PolicyId}", policyId);
                return StatusCode(500, "Dosyalar getirilirken bir hata oluştu");
            }
        }

        /// <summary>
        /// Dosya erişim kontrolü
        /// </summary>
        [HttpGet("access/{documentId}")]
        [Authorize(Roles = "admin,agent,customer")]
        public async Task<ActionResult<bool>> CheckFileAccess(int documentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                if (userId == null)
                {
                    return Unauthorized("Kullanıcı kimliği doğrulanamadı");
                }

                var hasAccess = await _fileUploadService.IsFileAccessibleAsync(documentId, userId.Value, userRole);
                
                return Ok(hasAccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking file access for document {DocumentId}", documentId);
                return StatusCode(500, "Dosya erişim kontrolü sırasında bir hata oluştu");
            }
        }

        /// <summary>
        /// Desteklenen dosya formatları
        /// </summary>
        [HttpGet("supported-formats")]
        [AllowAnonymous]
        public ActionResult<object> GetSupportedFormats()
        {
            return Ok(new
            {
                SupportedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx" },
                MaxFileSize = "10MB",
                MaxFileSizeBytes = 10 * 1024 * 1024
            });
        }

        /// <summary>
        /// Dosya yükleme durumu kontrolü
        /// </summary>
        [HttpGet("status/{documentId}")]
        [Authorize(Roles = "admin,agent,customer")]
        public async Task<ActionResult<object>> GetFileStatus(int documentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                if (userId == null)
                {
                    return Unauthorized("Kullanıcı kimliği doğrulanamadı");
                }

                // Dosya erişim kontrolü
                var hasAccess = await _fileUploadService.IsFileAccessibleAsync(documentId, userId.Value, userRole);
                if (!hasAccess)
                {
                    return Forbid("Bu dosyaya erişim yetkiniz yok");
                }

                var downloadInfo = await _fileUploadService.DownloadFileAsync(documentId);
                if (downloadInfo == null)
                {
                    return NotFound("Dosya bulunamadı");
                }

                var status = new
                {
                    DocumentId = documentId,
                    FileName = downloadInfo.FileName,
                    FileSize = downloadInfo.FileSize,
                    FileType = downloadInfo.FileType,
                    UploadedAt = downloadInfo.UploadedAt,
                    ExpiresAt = downloadInfo.ExpiresAt,
                    IsExpired = downloadInfo.ExpiresAt.HasValue && downloadInfo.ExpiresAt.Value < DateTime.UtcNow,
                    FileUrl = downloadInfo.FileUrl
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting file status for document {DocumentId}", documentId);
                return StatusCode(500, "Dosya durumu getirilirken bir hata oluştu");
            }
        }

        #region Helper Methods

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "customer";
        }

        private Task<int?> GetCurrentCustomerId()
        {
            // Bu method gerçek implementasyonda CustomerService kullanarak current user'ın customer ID'sini getirir
            // Şimdilik basit bir placeholder
            var userId = GetCurrentUserId();
            if (userId == null) return Task.FromResult<int?>(null);

            // Gerçek implementasyonda:
            // var customer = await _customerService.GetCustomerByUserIdAsync(userId.Value);
            // return customer?.Id;
            
            return Task.FromResult<int?>(userId); // Placeholder
        }

        #endregion
    }
}
