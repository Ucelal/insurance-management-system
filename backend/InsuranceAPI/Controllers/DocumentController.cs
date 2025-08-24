using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;
using InsuranceAPI.Models;
using System.Security.Claims;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // JWT authentication aktif
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentController> _logger;
        
        public DocumentController(IDocumentService documentService, ILogger<DocumentController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }
        
        #region CRUD Operations
        
        // Tüm dökümanları getir (admin ve agent)
        [HttpGet]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<DocumentDto>>> GetAllDocuments()
        {
            try
            {
                var documents = await _documentService.GetAllDocumentsAsync();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm dökümanlar alınırken hata oluştu");
                return StatusCode(500, new { message = "Döküman listesi alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // ID'ye göre döküman getir (admin ve agent)
        [HttpGet("{id}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<DocumentDto>> GetDocumentById(int id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return NotFound(new { message = "Döküman bulunamadı" });
                }
                
                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman ID: {Id} alınırken hata oluştu", id);
                return StatusCode(500, new { message = "Döküman bilgisi alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Yeni döküman oluştur
        [HttpPost]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<DocumentDto>> CreateDocument([FromBody] CreateDocumentDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                // JWT token'dan kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı" });
                }
                
                var document = await _documentService.CreateDocumentAsync(createDto, userId.Value);
                return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, document);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman oluşturulurken hata oluştu");
                return StatusCode(500, new { message = "Döküman oluşturulurken hata oluştu", error = ex.Message });
            }
        }
        
        // Döküman güncelle
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<DocumentDto>> UpdateDocument(int id, [FromBody] UpdateDocumentDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var document = await _documentService.UpdateDocumentAsync(id, updateDto);
                if (document == null)
                {
                    return NotFound(new { message = "Döküman bulunamadı" });
                }
                
                return Ok(document);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman ID: {Id} güncellenirken hata oluştu", id);
                return StatusCode(500, new { message = "Döküman güncellenirken hata oluştu", error = ex.Message });
            }
        }
        
        // Döküman sil
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteDocument(int id)
        {
            try
            {
                var result = await _documentService.DeleteDocumentAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Döküman bulunamadı" });
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman ID: {Id} silinirken hata oluştu", id);
                return StatusCode(500, new { message = "Döküman silinirken hata oluştu", error = ex.Message });
            }
        }
        
        #endregion
        
        #region Query Operations
        
        // Müşteriye göre dökümanları getir (admin ve agent)
        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<DocumentDto>>> GetDocumentsByCustomer(int customerId)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByCustomerAsync(customerId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri ID: {CustomerId} dökümanları alınırken hata oluştu", customerId);
                return StatusCode(500, new { message = "Müşteri dökümanları alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Hasara göre dökümanları getir (admin ve agent)
        [HttpGet("claim/{claimId}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<DocumentDto>>> GetDocumentsByClaim(int claimId)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByClaimAsync(claimId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasar ID: {ClaimId} dökümanları alınırken hata oluştu", claimId);
                return StatusCode(500, new { message = "Hasar dökümanları alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Poliçeye göre dökümanları getir (admin ve agent)
        [HttpGet("policy/{policyId}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<DocumentDto>>> GetDocumentsByPolicy(int policyId)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByPolicyAsync(policyId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Poliçe ID: {PolicyId} dökümanları alınırken hata oluştu", policyId);
                return StatusCode(500, new { message = "Poliçe dökümanları alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Kategoriye göre dökümanları getir (admin ve agent)
        [HttpGet("category/{category}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<DocumentDto>>> GetDocumentsByCategory(string category)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByCategoryAsync(category);
                return Ok(documents);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori: {Category} dökümanları alınırken hata oluştu", category);
                return StatusCode(500, new { message = "Kategori dökümanları alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Duruma göre dökümanları getir (admin ve agent)
        [HttpGet("status/{status}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<DocumentDto>>> GetDocumentsByStatus(string status)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByStatusAsync(status);
                return Ok(documents);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Durum: {Status} dökümanları alınırken hata oluştu", status);
                return StatusCode(500, new { message = "Durum dökümanları alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Dosya türüne göre dökümanları getir (admin ve agent)
        [HttpGet("filetype/{fileType}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<DocumentDto>>> GetDocumentsByFileType(string fileType)
        {
            try
            {
                var documents = await _documentService.GetDocumentsByFileTypeAsync(fileType);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya türü: {FileType} dökümanları alınırken hata oluştu", fileType);
                return StatusCode(500, new { message = "Dosya türü dökümanları alınırken hata oluştu", error = ex.Message });
            }
        }
        
        #endregion
        
        #region Search and Filter
        
        // Döküman arama
        [HttpGet("search")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<DocumentDto>>> SearchDocuments([FromQuery] DocumentSearchDto searchDto)
        {
            try
            {
                var documents = await _documentService.SearchDocumentsAsync(searchDto);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman arama yapılırken hata oluştu");
                return StatusCode(500, new { message = "Döküman arama yapılırken hata oluştu", error = ex.Message });
            }
        }
        
        // Döküman istatistikleri
        [HttpGet("statistics")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<DocumentStatisticsDto>> GetDocumentStatistics()
        {
            try
            {
                var statistics = await _documentService.GetDocumentStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman istatistikleri alınırken hata oluştu");
                return StatusCode(500, new { message = "Döküman istatistikleri alınırken hata oluştu", error = ex.Message });
            }
        }
        
        #endregion
        
        #region File Operations
        
        // Döküman yükleme
        [HttpPost("upload")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<DocumentUploadResponseDto>> UploadDocument([FromBody] CreateDocumentDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                // JWT token'dan kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Kullanıcı kimliği doğrulanamadı" });
                }
                
                var response = await _documentService.UploadDocumentAsync(createDto, userId.Value);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman yüklenirken hata oluştu");
                return StatusCode(500, new { message = "Döküman yüklenirken hata oluştu", error = ex.Message });
            }
        }
        
        // Döküman durumu güncelle
        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult> UpdateDocumentStatus(int id, [FromBody] string status)
        {
            try
            {
                var result = await _documentService.UpdateDocumentStatusAsync(id, status);
                if (!result)
                {
                    return BadRequest(new { message = "Geçersiz durum veya döküman bulunamadı" });
                }
                
                return Ok(new { message = "Döküman durumu başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman durumu güncellenirken hata oluştu: ID {Id}", id);
                return StatusCode(500, new { message = "Döküman durumu güncellenirken hata oluştu", error = ex.Message });
            }
        }
        
        // Döküman arşivle
        [HttpPut("{id}/archive")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult> ArchiveDocument(int id)
        {
            try
            {
                var result = await _documentService.ArchiveDocumentAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Döküman bulunamadı" });
                }
                
                return Ok(new { message = "Döküman başarıyla arşivlendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman ID: {Id} arşivlenirken hata oluştu", id);
                return StatusCode(500, new { message = "Döküman arşivlenirken hata oluştu", error = ex.Message });
            }
        }
        
        // Döküman geri yükle
        [HttpPut("{id}/restore")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult> RestoreDocument(int id)
        {
            try
            {
                var result = await _documentService.RestoreDocumentAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Döküman bulunamadı" });
                }
                
                return Ok(new { message = "Döküman başarıyla geri yüklendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman ID: {Id} geri yüklenirken hata oluştu", id);
                return StatusCode(500, new { message = "Döküman geri yüklenirken hata oluştu", error = ex.Message });
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        // Döküman kategorileri (admin ve agent)
        [HttpGet("categories")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<string>>> GetDocumentCategories()
        {
            try
            {
                var categories = await _documentService.GetDocumentCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman kategorileri alınırken hata oluştu");
                return StatusCode(500, new { message = "Döküman kategorileri alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Döküman durumları (admin ve agent)
        [HttpGet("statuses")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<string>>> GetDocumentStatuses()
        {
            try
            {
                var statuses = await _documentService.GetDocumentStatusesAsync();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman durumları alınırken hata oluştu");
                return StatusCode(500, new { message = "Döküman durumları alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Desteklenen dosya türleri (admin ve agent)
        [HttpGet("filetypes")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<string>>> GetSupportedFileTypes()
        {
            try
            {
                var fileTypes = await _documentService.GetSupportedFileTypesAsync();
                return Ok(fileTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Desteklenen dosya türleri alınırken hata oluştu");
                return StatusCode(500, new { message = "Desteklenen dosya türleri alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Toplam depolama alanı
        [HttpGet("storage")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<object>> GetTotalStorageUsed()
        {
            try
            {
                var totalSize = await _documentService.GetTotalStorageUsedAsync();
                var totalSizeMB = totalSize / (1024 * 1024);
                
                return Ok(new { 
                    TotalSizeBytes = totalSize,
                    TotalSizeMB = totalSizeMB,
                    TotalSizeGB = totalSizeMB / 1024.0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toplam depolama alanı hesaplanırken hata oluştu");
                return StatusCode(500, new { message = "Toplam depolama alanı hesaplanırken hata oluştu", error = ex.Message });
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private int? GetCurrentUserId()
        {
            // JWT token'dan kullanıcı ID'sini al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            
            _logger.LogWarning("No user ID found in JWT claims");
            return null;
        }
        
        #endregion
    }
}
