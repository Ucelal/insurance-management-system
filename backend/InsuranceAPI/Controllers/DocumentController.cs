using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;
using InsuranceAPI.Models;
using InsuranceAPI.Data;
using Microsoft.EntityFrameworkCore;
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
        private readonly InsuranceDbContext _context;
        
        public DocumentController(IDocumentService documentService, ILogger<DocumentController> logger, InsuranceDbContext context)
        {
            _documentService = documentService;
            _logger = logger;
            _context = context;
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

        // Müşterinin kendi dokümanlarını getir
        [HttpGet("my-documents")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<List<DocumentDto>>> GetMyDocuments()
        {
            try
            {
                // Debug: Tüm claims'leri logla
                _logger.LogInformation("🔍 DocumentController: All claims: {Claims}", 
                    string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                
                // Debug: Role claim'ini kontrol et
                var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                _logger.LogInformation("🔍 DocumentController: Role claim: {RoleClaim}", roleClaim?.Value ?? "null");
                
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("🔍 DocumentController: UserId: {UserId}", userId);
                
                var documents = await _documentService.GetDocumentsByCustomerAsync(userId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri dokümanları alınırken hata oluştu");
                return StatusCode(500, new { message = "Doküman listesi alınırken hata oluştu", error = ex.Message });
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
                return CreatedAtAction(nameof(GetDocumentById), new { id = document.UserId }, document);
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
        
        // Döküman sil (admin)
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
        
        // Customer kendi claim belgesini sil
        [HttpDelete("my-claim-documents/{id}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult> DeleteMyClaimDocument(int id)
        {
            try
            {
                // Kullanıcı ID'sini JWT token'dan al
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }
                
                // Belgeyi al ve kontrol et
                var document = await _context.Documents
                    .Include(d => d.Claim)
                    .FirstOrDefaultAsync(d => d.DocumentId == id);
                
                if (document == null)
                {
                    return NotFound(new { message = "Belge bulunamadı" });
                }
                
                // Belge bir claim'e ait mi kontrol et
                if (document.ClaimId == null)
                {
                    return BadRequest(new { message = "Bu belge bir olay bildirimine ait değil" });
                }
                
                // Claim'in sahibi mi kontrol et
                if (document.Claim?.CreatedByUserId != userId)
                {
                    return Forbid();
                }
                
                // Sadece Pending durumundaki claim'lerin belgeleri silinebilir
                if (document.Claim?.Status != "Pending")
                {
                    return BadRequest(new { message = "Sadece beklemedeki olay bildirimlerinin belgeleri silinebilir" });
                }
                
                // Belgeyi sil
                Console.WriteLine($"🗑️ DeleteMyClaimDocument: Deleting document ID: {id}");
                var result = await _documentService.DeleteDocumentAsync(id);
                if (!result)
                {
                    Console.WriteLine($"❌ DeleteMyClaimDocument: Document not found: {id}");
                    return NotFound(new { message = "Belge silinemedi" });
                }
                
                Console.WriteLine($"✅ DeleteMyClaimDocument: Document deleted successfully: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Belge ID: {Id} silinirken hata oluştu", id);
                return StatusCode(500, new { message = "Belge silinirken hata oluştu", error = ex.Message });
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
        

        // PDF dosya yükleme endpoint'i (Customer için)
        [HttpPost("upload-pdf")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<string>> UploadPdfFile(IFormFile file)
        {
            try
            {
                // Dosya kontrolü
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Dosya seçilmedi." });
                }
                
                // Dosya türü kontrolü
                if (file.ContentType != "application/pdf")
                {
                    return BadRequest(new { message = "Sadece PDF dosyası yüklenebilir." });
                }
                
                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }
                
                // Dosyayı kaydet
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var uploadPath = Path.Combine("wwwroot", "uploads", "customer-documents");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Dosya URL'sini döndür
                var fileUrl = $"/uploads/customer-documents/{fileName}";
                
                Console.WriteLine($"✅ PDF uploaded: {fileName}, Size: {file.Length} bytes, Path: {filePath}");
                
                return Ok(new { 
                    message = "PDF dosyası başarıyla yüklendi.", 
                    fileName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF yükleme sırasında hata oluştu");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu", error = ex.Message });
            }
        }

        // Tapu belgesi PDF dosya yükleme endpoint'i (Customer için)
        [HttpPost("upload-tapu")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<string>> UploadTapuFile(IFormFile file)
        {
            try
            {
                // Dosya kontrolü
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Dosya seçilmedi." });
                }
                
                // Dosya türü kontrolü
                if (file.ContentType != "application/pdf")
                {
                    return BadRequest(new { message = "Sadece PDF dosyası yüklenebilir." });
                }
                
                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }
                
                // Dosyayı kaydet
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var uploadPath = Path.Combine("wwwroot", "uploads", "customer-documents", "tapu");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Dosya URL'sini döndür
                var fileUrl = $"/uploads/customer-documents/tapu/{fileName}";
                
                Console.WriteLine($"✅ Tapu PDF uploaded: {fileName}, Size: {file.Length} bytes, Path: {filePath}");
                
                return Ok(new { 
                    message = "Tapu belgesi başarıyla yüklendi.", 
                    fileName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tapu belgesi yükleme sırasında hata oluştu");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu", error = ex.Message });
            }
        }

        // Sağlık raporu PDF dosya yükleme endpoint'i (Customer için)
        [HttpPost("upload-health-report")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<string>> UploadHealthReportFile(IFormFile file)
        {
            try
            {
                // Dosya kontrolü
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Dosya seçilmedi." });
                }
                
                // Dosya türü kontrolü
                if (file.ContentType != "application/pdf")
                {
                    return BadRequest(new { message = "Sadece PDF dosyası yüklenebilir." });
                }
                
                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }
                
                // Dosyayı kaydet
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var uploadPath = Path.Combine("wwwroot", "uploads", "customer-documents", "health-reports");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Dosya URL'sini döndür
                var fileUrl = $"/uploads/customer-documents/health-reports/{fileName}";
                
                Console.WriteLine($"✅ Health Report PDF uploaded: {fileName}, Size: {file.Length} bytes, Path: {filePath}");
                
                return Ok(new { 
                    message = "Sağlık raporu başarıyla yüklendi.", 
                    fileName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sağlık raporu yükleme sırasında hata oluştu");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu", error = ex.Message });
            }
        }

        // Yıllık ciro raporu PDF dosya yükleme endpoint'i (Customer için)
        [HttpPost("upload-annual-revenue")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<string>> UploadAnnualRevenueFile(IFormFile file)
        {
            try
            {
                // Dosya kontrolü
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Dosya seçilmedi." });
                }
                
                // Dosya türü kontrolü
                if (file.ContentType != "application/pdf")
                {
                    return BadRequest(new { message = "Sadece PDF dosyası yüklenebilir." });
                }
                
                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }
                
                // Dosyayı kaydet
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var uploadPath = Path.Combine("wwwroot", "uploads", "customer-documents", "annual-revenue");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Dosya URL'sini döndür
                var fileUrl = $"/uploads/customer-documents/annual-revenue/{fileName}";
                
                Console.WriteLine($"✅ Annual Revenue PDF uploaded: {fileName}, Size: {file.Length} bytes, Path: {filePath}");
                
                return Ok(new { 
                    message = "Yıllık ciro raporu başarıyla yüklendi.", 
                    fileName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yıllık ciro raporu yükleme sırasında hata oluştu");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu", error = ex.Message });
            }
        }

        // Risk raporu PDF dosya yükleme endpoint'i (Customer için)
        [HttpPost("upload-risk-report")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<string>> UploadRiskReportFile(IFormFile file)
        {
            try
            {
                // Dosya kontrolü
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Dosya seçilmedi." });
                }
                
                // Dosya türü kontrolü
                if (file.ContentType != "application/pdf")
                {
                    return BadRequest(new { message = "Sadece PDF dosyası yüklenebilir." });
                }
                
                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }
                
                // Dosyayı kaydet
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var uploadPath = Path.Combine("wwwroot", "uploads", "customer-documents", "risk-reports");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Dosya URL'sini döndür
                var fileUrl = $"/uploads/customer-documents/risk-reports/{fileName}";
                
                Console.WriteLine($"✅ Risk Report PDF uploaded: {fileName}, Size: {file.Length} bytes, Path: {filePath}");
                
                return Ok(new { 
                    message = "Risk raporu başarıyla yüklendi.", 
                    fileName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Risk raporu yükleme sırasında hata oluştu");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu", error = ex.Message });
            }
        }

        // Tıbbi geçmiş raporu PDF dosya yükleme endpoint'i (Customer için)
        [HttpPost("upload-medical-history")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<string>> UploadMedicalHistoryFile(IFormFile file)
        {
            try
            {
                // Dosya kontrolü
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Dosya seçilmedi." });
                }
                
                // Dosya türü kontrolü
                if (file.ContentType != "application/pdf")
                {
                    return BadRequest(new { message = "Sadece PDF dosyası yüklenebilir." });
                }
                
                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }
                
                // Dosyayı kaydet
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var uploadPath = Path.Combine("wwwroot", "uploads", "customer-documents", "medical-history");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Dosya URL'sini döndür
                var fileUrl = $"/uploads/customer-documents/medical-history/{fileName}";
                
                // Müşteri bilgisini al
                var userIdClaim = User.FindFirst("nameid")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer != null)
                    {
                        // Veritabanına kaydet
                        var document = new Document
                        {
                            FileName = file.FileName,
                            FileUrl = fileUrl,
                            FileType = "PDF",
                            FileSize = file.Length,
                            Category = "Sağlık Sigortası - Tıbbi Geçmiş",
                            Description = "Müşteri tarafından yüklenen tıbbi geçmiş raporu",
                            Status = "Active",
                            UploadedAt = DateTime.UtcNow,
                            CustomerId = customer.CustomerId,
                            UploadedByUserId = userId
                        };
                        
                        _context.Documents.Add(document);
                        await _context.SaveChangesAsync();
                        
                        Console.WriteLine($"✅ Medical History PDF uploaded and saved to database: {fileName}, DocumentId: {document.DocumentId}");
                    }
                }
                
                Console.WriteLine($"✅ Medical History PDF uploaded: {fileName}, Size: {file.Length} bytes, Path: {filePath}");
                
                return Ok(new { 
                    message = "Tıbbi geçmiş raporu başarıyla yüklendi.", 
                    fileName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tıbbi geçmiş raporu yükleme sırasında hata oluştu");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu", error = ex.Message });
            }
        }

        // Aile geçmişi raporu PDF dosya yükleme endpoint'i (Customer için)
        [HttpPost("upload-family-history")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<string>> UploadFamilyHistoryFile(IFormFile file)
        {
            try
            {
                // Dosya kontrolü
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Dosya seçilmedi." });
                }
                
                // Dosya türü kontrolü
                if (file.ContentType != "application/pdf")
                {
                    return BadRequest(new { message = "Sadece PDF dosyası yüklenebilir." });
                }
                
                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }
                
                // Dosyayı kaydet
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var uploadPath = Path.Combine("wwwroot", "uploads", "customer-documents", "family-history");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Dosya URL'sini döndür
                var fileUrl = $"/uploads/customer-documents/family-history/{fileName}";
                
                // Müşteri bilgisini al
                var userIdClaim = User.FindFirst("nameid")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer != null)
                    {
                        // Veritabanına kaydet
                        var document = new Document
                        {
                            FileName = file.FileName,
                            FileUrl = fileUrl,
                            FileType = "PDF",
                            FileSize = file.Length,
                            Category = "Sağlık Sigortası - Aile Geçmişi",
                            Description = "Müşteri tarafından yüklenen aile geçmişi raporu",
                            Status = "Active",
                            UploadedAt = DateTime.UtcNow,
                            CustomerId = customer.CustomerId,
                            UploadedByUserId = userId
                        };
                        
                        _context.Documents.Add(document);
                        await _context.SaveChangesAsync();
                        
                        Console.WriteLine($"✅ Family History PDF uploaded and saved to database: {fileName}, DocumentId: {document.DocumentId}");
                    }
                }
                
                Console.WriteLine($"✅ Family History PDF uploaded: {fileName}, Size: {file.Length} bytes, Path: {filePath}");
                
                return Ok(new { 
                    message = "Aile geçmişi raporu başarıyla yüklendi.", 
                    fileName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aile geçmişi raporu yükleme sırasında hata oluştu");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu", error = ex.Message });
            }
        }

        // Kimlik ön yüz fotoğrafı yükleme endpoint'i (Customer için)
        [HttpPost("upload-id-front")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<string>> UploadIdFrontPhoto(IFormFile file)
        {
            try
            {
                // Dosya kontrolü
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Dosya seçilmedi." });
                }
                
                // Dosya türü kontrolü (sadece resim)
                if (!file.ContentType.StartsWith("image/"))
                {
                    return BadRequest(new { message = "Sadece resim dosyası yüklenebilir." });
                }
                
                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }
                
                // Dosyayı kaydet
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var uploadPath = Path.Combine("wwwroot", "uploads", "customer-documents", "id-photos", "front");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Dosya URL'sini döndür
                var fileUrl = $"/uploads/customer-documents/id-photos/front/{fileName}";
                
                // Müşteri bilgisini al
                var userIdClaim = User.FindFirst("nameid")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer != null)
                    {
                        // Veritabanına kaydet
                        var document = new Document
                        {
                            FileName = file.FileName,
                            FileUrl = fileUrl,
                            FileType = "IMAGE",
                            FileSize = file.Length,
                            Category = "Hayat Sigortası - Kimlik Ön Yüz",
                            Description = "Müşteri tarafından yüklenen kimlik ön yüz fotoğrafı",
                            Status = "Active",
                            UploadedAt = DateTime.UtcNow,
                            CustomerId = customer.CustomerId,
                            UploadedByUserId = userId
                        };
                        
                        _context.Documents.Add(document);
                        await _context.SaveChangesAsync();
                        
                        Console.WriteLine($"✅ ID Front Photo uploaded and saved to database: {fileName}, DocumentId: {document.DocumentId}");
                    }
                }
                
                Console.WriteLine($"✅ ID Front Photo uploaded: {fileName}, Size: {file.Length} bytes, Path: {filePath}");
                
                return Ok(new { 
                    message = "Kimlik ön yüz fotoğrafı başarıyla yüklendi.", 
                    fileName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kimlik ön yüz fotoğrafı yükleme sırasında hata oluştu");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu", error = ex.Message });
            }
        }

        // Kimlik arka yüz fotoğrafı yükleme endpoint'i (Customer için)
        [HttpPost("upload-id-back")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<string>> UploadIdBackPhoto(IFormFile file)
        {
            try
            {
                // Dosya kontrolü
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Dosya seçilmedi." });
                }
                
                // Dosya türü kontrolü (sadece resim)
                if (!file.ContentType.StartsWith("image/"))
                {
                    return BadRequest(new { message = "Sadece resim dosyası yüklenebilir." });
                }
                
                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }
                
                // Dosyayı kaydet
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var uploadPath = Path.Combine("wwwroot", "uploads", "customer-documents", "id-photos", "back");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Dosya URL'sini döndür
                var fileUrl = $"/uploads/customer-documents/id-photos/back/{fileName}";
                
                // Müşteri bilgisini al
                var userIdClaim = User.FindFirst("nameid")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer != null)
                    {
                        // Veritabanına kaydet
                        var document = new Document
                        {
                            FileName = file.FileName,
                            FileUrl = fileUrl,
                            FileType = "IMAGE",
                            FileSize = file.Length,
                            Category = "Hayat Sigortası - Kimlik Arka Yüz",
                            Description = "Müşteri tarafından yüklenen kimlik arka yüz fotoğrafı",
                            Status = "Active",
                            UploadedAt = DateTime.UtcNow,
                            CustomerId = customer.CustomerId,
                            UploadedByUserId = userId
                        };
                        
                        _context.Documents.Add(document);
                        await _context.SaveChangesAsync();
                        
                        Console.WriteLine($"✅ ID Back Photo uploaded and saved to database: {fileName}, DocumentId: {document.DocumentId}");
                    }
                }
                
                Console.WriteLine($"✅ ID Back Photo uploaded: {fileName}, Size: {file.Length} bytes, Path: {filePath}");
                
                return Ok(new { 
                    message = "Kimlik arka yüz fotoğrafı başarıyla yüklendi.", 
                    fileName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kimlik arka yüz fotoğrafı yükleme sırasında hata oluştu");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu", error = ex.Message });
            }
        }

        // Olay Formu için genel dosya yükleme endpoint'i (Customer için)
        [HttpPost("upload-incident-document")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<string>> UploadIncidentDocument(IFormFile file, [FromForm] int claimId)
        {
            try
            {
                // Dosya kontrolü
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Dosya seçilmedi." });
                }
                
                // Desteklenen dosya türleri
                var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png", "image/jpg", 
                                         "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                                         "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" };
                
                if (!allowedTypes.Contains(file.ContentType))
                {
                    return BadRequest(new { message = "Desteklenmeyen dosya türü. Sadece PDF, JPG, PNG, DOC, DOCX, XLS, XLSX dosyaları yüklenebilir." });
                }
                
                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }
                
                // Dosyayı kaydet
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var uploadPath = Path.Combine("wwwroot", "uploads", "incident-documents");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Dosya URL'sini döndür
                var fileUrl = $"/uploads/incident-documents/{fileName}";
                
                // Müşteri bilgisini al
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"🔍 UploadIncidentDocument: UserIdClaim = {userIdClaim}");
                
                if (int.TryParse(userIdClaim, out int userId))
                {
                    Console.WriteLine($"🔍 UploadIncidentDocument: UserId = {userId}");
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                    
                    if (customer != null)
                    {
                        Console.WriteLine($"🔍 UploadIncidentDocument: Customer found, CustomerId = {customer.CustomerId}");
                        
                        // Veritabanına kaydet
                        var document = new Document
                        {
                            FileName = file.FileName,
                            FileUrl = fileUrl,
                            FileType = file.ContentType.StartsWith("image/") ? "IMAGE" : 
                                      file.ContentType.Contains("pdf") ? "PDF" : "DOCUMENT",
                            FileSize = file.Length,
                            Category = "Olay Formu Belgesi",
                            Description = $"Olay Formu için yüklenen belge - {file.FileName}",
                            Status = "Active",
                            UploadedAt = DateTime.UtcNow,
                            CustomerId = customer.CustomerId,
                            ClaimId = claimId,
                            UploadedByUserId = userId
                        };
                        
                        _context.Documents.Add(document);
                        await _context.SaveChangesAsync();
                        
                        Console.WriteLine($"✅ Incident Document uploaded and saved to database: {fileName}, DocumentId: {document.DocumentId}, ClaimId: {claimId}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ UploadIncidentDocument: Customer not found for UserId = {userId}");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ UploadIncidentDocument: Could not parse userId from claim: {userIdClaim}");
                }
                
                Console.WriteLine($"✅ Incident Document uploaded: {fileName}, Size: {file.Length} bytes, Path: {filePath}");
                
                return Ok(new { 
                    message = "Belge başarıyla yüklendi.", 
                    fileName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length,
                    claimId = claimId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Olay Formu belgesi yükleme sırasında hata oluştu");
                return StatusCode(500, new { message = "Dosya yüklenirken hata oluştu", error = ex.Message });
            }
        }

        #endregion

        // Dosya serve etmek için endpoint
        [HttpGet("serve/{*filePath}")]
        [AllowAnonymous]
        public IActionResult ServeFile(string filePath)
        {
            try
            {
                // URL decode yap
                filePath = Uri.UnescapeDataString(filePath);
                Console.WriteLine($"🔍 ServeFile: Decoded filePath: {filePath}");
                
                string fullPath;
                
                // Eğer filePath zaten documents/pdfs ile başlıyorsa
                if (filePath.StartsWith("documents/pdfs/"))
                {
                    fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
                }
                // Eğer sadece dosya adı verilmişse, önce documents/pdfs'te ara
                else if (filePath.Contains("Poliçe_") || filePath.Contains("payment_receipt_"))
                {
                    fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", "pdfs", Path.GetFileName(filePath));
                }
                // Diğer durumlarda uploads klasöründe ara
                else
                {
                    fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", filePath);
                }
                
                Console.WriteLine($"🔍 ServeFile: Looking for file at: {fullPath}");
                
                if (!System.IO.File.Exists(fullPath))
                {
                    Console.WriteLine($"❌ File not found: {fullPath}");
                    return NotFound(new { message = "Dosya bulunamadı" });
                }

                var fileBytes = System.IO.File.ReadAllBytes(fullPath);
                var contentType = GetContentType(fullPath);
                
                Console.WriteLine($"✅ File served: {fullPath}, Size: {fileBytes.Length} bytes");
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error serving file: {ex.Message}");
                return StatusCode(500, new { message = "Dosya servis edilirken hata oluştu" });
            }
        }

        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        [HttpPost("upload-policy-pdf")]
        [Authorize(Roles = "admin,agent")]
        public async Task<IActionResult> UploadPolicyPdf(IFormFile file, [FromForm] int offerId)
        {
            try
            {
                Console.WriteLine($"🔍 UploadPolicyPdf: Received offerId = {offerId}");
                
                // Dosya kontrolü
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "PDF dosyası seçilmedi." });
                }
                
                // Dosya türü kontrolü
                if (file.ContentType != "application/pdf")
                {
                    return BadRequest(new { message = "Sadece PDF dosyası yüklenebilir." });
                }
                
                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }
                
                // Teklif kontrolü
                Console.WriteLine($"🔍 UploadPolicyPdf: Looking for offer with ID = {offerId}");
                var offer = await _context.Offers
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OfferId == offerId);
                if (offer == null)
                {
                    Console.WriteLine($"❌ UploadPolicyPdf: Offer with ID {offerId} not found");
                    // Debug: Let's see what offers exist
                    var allOffers = await _context.Offers.ToListAsync();
                    Console.WriteLine($"🔍 UploadPolicyPdf: Total offers in database: {allOffers.Count}");
                    foreach (var o in allOffers)
                    {
                        Console.WriteLine($"🔍 UploadPolicyPdf: Offer ID: {o.OfferId}, Status: {o.Status}");
                    }
                    return NotFound(new { message = "Teklif bulunamadı." });
                }
                
                Console.WriteLine($"✅ UploadPolicyPdf: Found offer {offerId}, Status: {offer.Status}");
                Console.WriteLine($"🔍 UploadPolicyPdf: Customer ID: {offer.Customer?.CustomerId}, User ID: {offer.Customer?.UserId}");
                
                // Customer ve User ID kontrolü
                if (offer.Customer == null)
                {
                    Console.WriteLine($"❌ UploadPolicyPdf: Customer not found for offer {offerId}");
                    return BadRequest(new { message = "Teklif ile ilişkili müşteri bulunamadı." });
                }
                
                if (offer.Customer.UserId == null || offer.Customer.UserId == 0)
                {
                    Console.WriteLine($"❌ UploadPolicyPdf: Invalid User ID for customer {offer.Customer.CustomerId}");
                    return BadRequest(new { message = "Müşteri ile ilişkili kullanıcı bulunamadı." });
                }
                
                // Dosyayı kaydet
                var fileName = $"Poliçe_Offer_{offerId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
                var uploadPath = Path.Combine("wwwroot", "documents", "pdfs");
                
                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Dosya URL'sini oluştur
                var fileUrl = $"/documents/pdfs/{fileName}";
                
                // Get current user ID from JWT token
                var currentUserId = int.Parse(HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                Console.WriteLine($"🔍 UploadPolicyPdf: Current user ID from JWT: {currentUserId}");
                
                // Verify the current user exists in the database
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                {
                    Console.WriteLine($"❌ UploadPolicyPdf: Current user with ID {currentUserId} not found in database");
                    return BadRequest(new { message = "Geçerli kullanıcı bulunamadı." });
                }
                Console.WriteLine($"✅ UploadPolicyPdf: Current user found: {currentUser.Name} ({currentUser.Email})");
                
                // Veritabanında doküman kaydı oluştur
                var document = new Models.Document
                {
                    FileName = fileName,
                    FileUrl = fileUrl,
                    FileType = "application/pdf",
                    FileSize = file.Length,
                    Category = "Poliçe",
                    Description = $"Teklif #{offerId} için yüklenen poliçe PDF'i",
                    Status = "Active",
                    UploadedAt = DateTime.UtcNow,
                    CustomerId = offer.CustomerId,
                    UserId = offer.Customer.UserId.Value, // Customer who owns the document
                    UploadedByUserId = currentUserId // User who uploaded the document
                };
                
                _context.Documents.Add(document);
                await _context.SaveChangesAsync();
                
                // Teklifte PDF URL'ini güncelle (eğer böyle bir alan varsa)
                offer.PolicyPdfUrl = fileUrl;
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ Policy PDF uploaded: {fileName}, Size: {file.Length} bytes, Path: {filePath}");
                
                return Ok(new { 
                    message = "Poliçe PDF dosyası başarıyla yüklendi.", 
                    fileName = fileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length,
                    documentId = document.DocumentId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Poliçe PDF yükleme sırasında hata oluştu");
                return StatusCode(500, new { message = "PDF dosyası yüklenirken hata oluştu", error = ex.Message });
            }
        }

        [HttpPost("create-payment-receipt-pdf")]
        public async Task<IActionResult> CreatePaymentReceiptPdf([FromBody] PaymentReceiptDto receiptDto)
        {
            try
            {
                Console.WriteLine($"📄 DocumentController: Creating payment receipt PDF for transaction: {receiptDto.TransactionId}");
                
                var pdfService = new PdfService(_context);
                var pdfUrl = await pdfService.CreatePaymentReceiptPdfAsync(receiptDto);
                
                Console.WriteLine($"✅ DocumentController: Payment receipt PDF created successfully: {pdfUrl}");
                
                return Ok(new { success = true, pdfUrl = pdfUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DocumentController: Error creating payment receipt PDF: {ex.Message}");
                Console.WriteLine($"❌ DocumentController: Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "PDF oluşturulurken hata oluştu" });
            }
        }
    }
}
