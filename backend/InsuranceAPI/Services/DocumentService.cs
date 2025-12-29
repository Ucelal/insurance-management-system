using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;

#nullable enable

namespace InsuranceAPI.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly InsuranceDbContext _context;
        private readonly ILogger<DocumentService> _logger;
        private readonly IPdfService _pdfService;
        
        // Desteklenen dosya türleri
        private readonly HashSet<string> _supportedFileTypes = new()
        {
            "pdf", "doc", "docx", "xls", "xlsx", "jpg", "jpeg", "png", "gif", "txt", "zip", "rar"
        };
        
        // Maksimum dosya boyutu (100MB)
        private const long MaxFileSize = 100 * 1024 * 1024;

        public DocumentService(InsuranceDbContext context, ILogger<DocumentService> logger, IPdfService pdfService)
        {
            _context = context;
            _logger = logger;
            _pdfService = pdfService;
        }

        #region CRUD Operations

        public async Task<List<DocumentDto>> GetAllDocumentsAsync()
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Customer)
                    .ThenInclude(c => c.User)
                    .Include(d => d.Policy)
                    .Include(d => d.Claim)
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.Status != "Silindi")
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();

                return documents.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm dökümanlar alınırken hata oluştu");
                throw;
            }
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Customer)
                    .ThenInclude(c => c.User)
                    .Include(d => d.Policy)
                    .Include(d => d.Claim)
                    .Include(d => d.UploadedByUser)
                    .FirstOrDefaultAsync(d => d.UserId == id && d.Status != "Silindi");

                return document != null ? MapToDto(document) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman ID: {Id} alınırken hata oluştu", id);
                throw;
            }
        }

        public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDto, int uploadedByUserId)
        {
            try
            {
                // Validasyon
                if (!await IsFileTypeSupportedAsync(createDto.FileType))
                {
                    throw new ArgumentException($"Desteklenmeyen dosya türü: {createDto.FileType}");
                }

                if (createDto.FileSize > MaxFileSize)
                {
                    throw new ArgumentException($"Dosya boyutu çok büyük. Maksimum: {MaxFileSize / (1024 * 1024)}MB");
                }

                var document = new Document
                {
                    CustomerId = createDto.CustomerId,
                    ClaimId = createDto.ClaimId,
                    PolicyId = createDto.PolicyId,
                    FileName = createDto.FileName,
                    FileUrl = createDto.FileUrl,
                    FileType = createDto.FileType.ToLower(),
                    FileSize = createDto.FileSize,
                    Category = createDto.Category,
                    Status = "Aktif",
                    Description = createDto.Description,
                    Version = createDto.Version,
                    UploadedAt = DateTime.UtcNow,
                    ExpiresAt = createDto.ExpiresAt,
                    UploadedByUserId = uploadedByUserId
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni döküman oluşturuldu: {FileName} (ID: {Id})", document.FileName, document.DocumentId);

                return await GetDocumentByIdAsync(document.DocumentId) ?? throw new InvalidOperationException("Döküman oluşturulamadı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman oluşturulurken hata oluştu");
                throw;
            }
        }

        public async Task<DocumentDto?> UpdateDocumentAsync(int id, UpdateDocumentDto updateDto)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null || document.Status == "Silindi")
                {
                    return null;
                }

                // Güncelleme
                if (!string.IsNullOrEmpty(updateDto.FileName))
                    document.FileName = updateDto.FileName;
                
                if (!string.IsNullOrEmpty(updateDto.FileUrl))
                    document.FileUrl = updateDto.FileUrl;
                
                if (!string.IsNullOrEmpty(updateDto.FileType))
                    document.FileType = updateDto.FileType.ToLower();
                
                if (updateDto.FileSize.HasValue)
                    document.FileSize = updateDto.FileSize.Value;
                
                if (!string.IsNullOrEmpty(updateDto.Category))
                    document.Category = updateDto.Category;
                
                if (!string.IsNullOrEmpty(updateDto.Status))
                    document.Status = updateDto.Status;
                
                if (!string.IsNullOrEmpty(updateDto.Description))
                    document.Description = updateDto.Description;
                
                if (!string.IsNullOrEmpty(updateDto.Version))
                    document.Version = updateDto.Version;
                
                if (updateDto.ExpiresAt.HasValue)
                    document.ExpiresAt = updateDto.ExpiresAt;

                document.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Döküman güncellendi: ID {Id}", id);

                return await GetDocumentByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman ID: {Id} güncellenirken hata oluştu", id);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                {
                    return false;
                }

                // Soft delete
                document.Status = "Silindi";
                document.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Döküman silindi: ID {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman ID: {Id} silinirken hata oluştu", id);
                throw;
            }
        }

        #endregion

        #region Query Operations

        public async Task<List<DocumentDto>> GetDocumentsByCustomerAsync(int userId)
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Customer)
                    .ThenInclude(c => c.User)
                    .Include(d => d.Policy)
                    .Include(d => d.Claim)
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.Status != "Silindi" && 
                               ((d.Customer != null && d.Customer.UserId == userId) || 
                                (d.UserId == userId)))
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();

                return documents.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri UserId: {UserId} dökümanları alınırken hata oluştu", userId);
                throw;
            }
        }

        public async Task<List<DocumentDto>> GetDocumentsByClaimAsync(int claimId)
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Customer)
                    .ThenInclude(c => c.User)
                    .Include(d => d.Policy)
                    .Include(d => d.Claim)
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.ClaimId == claimId && d.Status != "Silindi")
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();

                return documents.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasar ID: {ClaimId} dökümanları alınırken hata oluştu", claimId);
                throw;
            }
        }

        public async Task<List<DocumentDto>> GetDocumentsByPolicyAsync(int policyId)
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Customer)
                    .ThenInclude(c => c.User)
                    .Include(d => d.Policy)
                    .Include(d => d.Claim)
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.PolicyId == policyId && d.Status != "Silindi")
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();

                return documents.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Poliçe ID: {PolicyId} dökümanları alınırken hata oluştu", policyId);
                throw;
            }
        }

        public async Task<List<DocumentDto>> GetDocumentsByCategoryAsync(string category)
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Customer)
                    .ThenInclude(c => c.User)
                    .Include(d => d.Policy)
                    .Include(d => d.Claim)
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.Category == category && d.Status != "Silindi")
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();

                return documents.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori: {Category} dökümanları alınırken hata oluştu", category);
                throw;
            }
        }

        public async Task<List<DocumentDto>> GetDocumentsByStatusAsync(string status)
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Customer)
                    .ThenInclude(c => c.User)
                    .Include(d => d.Policy)
                    .Include(d => d.Claim)
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.Status == status)
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();

                return documents.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Durum: {Status} dökümanları alınırken hata oluştu", status);
                throw;
            }
        }

        public async Task<List<DocumentDto>> GetDocumentsByFileTypeAsync(string fileType)
        {
            try
            {
                var documents = await _context.Documents
                    .Include(d => d.Customer)
                    .ThenInclude(c => c.User)
                    .Include(d => d.Policy)
                    .Include(d => d.Claim)
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.FileType.ToLower() == fileType.ToLower() && d.Status != "Silindi")
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();

                return documents.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya türü: {FileType} dökümanları alınırken hata oluştu", fileType);
                throw;
            }
        }

        #endregion

        #region Search and Filter

        public async Task<List<DocumentDto>> SearchDocumentsAsync(DocumentSearchDto searchDto)
        {
            try
            {
                var query = _context.Documents
                    .Include(d => d.Customer)
                    .ThenInclude(c => c.User)
                    .Include(d => d.Policy)
                    .Include(d => d.Claim)
                    .Include(d => d.UploadedByUser)
                    .Where(d => d.Status != "Silindi");

                // Filtreler
                if (searchDto.CustomerId.HasValue)
                    query = query.Where(d => d.CustomerId == searchDto.CustomerId.Value);

                if (searchDto.ClaimId.HasValue)
                    query = query.Where(d => d.ClaimId == searchDto.ClaimId.Value);

                if (searchDto.PolicyId.HasValue)
                    query = query.Where(d => d.PolicyId == searchDto.PolicyId.Value);

                if (!string.IsNullOrEmpty(searchDto.Category))
                    query = query.Where(d => d.Category.ToString() == searchDto.Category);

                if (!string.IsNullOrEmpty(searchDto.Status))
                    query = query.Where(d => d.Status.ToString() == searchDto.Status);

                if (!string.IsNullOrEmpty(searchDto.FileType))
                    query = query.Where(d => d.FileType.ToLower() == searchDto.FileType.ToLower());

                if (searchDto.UploadedFrom.HasValue)
                    query = query.Where(d => d.UploadedAt >= searchDto.UploadedFrom.Value);

                if (searchDto.UploadedTo.HasValue)
                    query = query.Where(d => d.UploadedAt <= searchDto.UploadedTo.Value);

                if (!string.IsNullOrEmpty(searchDto.FileNameContains))
                    query = query.Where(d => d.FileName.Contains(searchDto.FileNameContains));

                // Sıralama ve sayfalama
                var totalCount = await query.CountAsync();
                var documents = await query
                    .OrderByDescending(d => d.UploadedAt)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return documents.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman arama yapılırken hata oluştu");
                throw;
            }
        }

        public async Task<DocumentStatisticsDto> GetDocumentStatisticsAsync()
        {
            try
            {
                var documents = await _context.Documents
                    .Where(d => d.Status != "Silindi")
                    .ToListAsync();

                var statistics = new DocumentStatisticsDto
                {
                    TotalDocuments = documents.Count,
                    ActiveDocuments = documents.Count(d => d.Status == "Aktif"),
                    ArchivedDocuments = documents.Count(d => d.Status == "Arşivlendi"),
                    TotalFileSize = documents.Sum(d => d.FileSize)
                };

                // Kategori bazında gruplama
                statistics.DocumentsByCategory = documents
                    .GroupBy(d => d.Category.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                // Dosya türü bazında gruplama
                statistics.DocumentsByFileType = documents
                    .GroupBy(d => d.FileType.ToUpper())
                    .ToDictionary(g => g.Key, g => g.Count());

                // Ay bazında gruplama
                statistics.DocumentsByMonth = documents
                    .GroupBy(d => d.UploadedAt.ToString("yyyy-MM"))
                    .ToDictionary(g => g.Key, g => g.Count());

                // Durum bazında gruplama
                statistics.DocumentsByStatus = documents
                    .GroupBy(d => d.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman istatistikleri alınırken hata oluştu");
                throw;
            }
        }

        #endregion

        #region File Operations

        public async Task<DocumentUploadResponseDto> UploadDocumentAsync(CreateDocumentDto createDto, int uploadedByUserId)
        {
            try
            {
                // Validasyon
                if (!await IsFileTypeSupportedAsync(createDto.FileType))
                {
                    return new DocumentUploadResponseDto
                    {
                        Success = false,
                        Message = "Desteklenmeyen dosya türü",
                        Error = $"Dosya türü '{createDto.FileType}' desteklenmiyor"
                    };
                }

                if (createDto.FileSize > MaxFileSize)
                {
                    return new DocumentUploadResponseDto
                    {
                        Success = false,
                        Message = "Dosya boyutu çok büyük",
                        Error = $"Maksimum dosya boyutu: {MaxFileSize / (1024 * 1024)}MB"
                    };
                }

                // Döküman oluştur
                var document = await CreateDocumentAsync(createDto, uploadedByUserId);

                return new DocumentUploadResponseDto
                {
                    Success = true,
                    Message = "Döküman başarıyla yüklendi",
                    Document = document
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman yüklenirken hata oluştu");
                return new DocumentUploadResponseDto
                {
                    Success = false,
                    Message = "Döküman yüklenirken hata oluştu",
                    Error = ex.Message
                };
            }
        }

        public async Task<bool> UpdateDocumentStatusAsync(int id, string status)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                {
                    return false;
                }

                document.Status = status;
                document.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Döküman durumu güncellendi: ID {Id}, Yeni durum: {Status}", id, status);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Döküman durumu güncellenirken hata oluştu: ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> ArchiveDocumentAsync(int id)
        {
            return await UpdateDocumentStatusAsync(id, "Arşivlendi");
        }

        public async Task<bool> RestoreDocumentAsync(int id)
        {
            return await UpdateDocumentStatusAsync(id, "Aktif");
        }

        #endregion

        #region Utility Methods

        public Task<List<string>> GetDocumentCategoriesAsync()
        {
            return Task.FromResult(new List<string> { "Kimlik", "Poliçe", "Talep", "Diğer" });
        }

        public Task<List<string>> GetDocumentStatusesAsync()
        {
            return Task.FromResult(new List<string> { "Aktif", "Arşivlendi", "Silindi" });
        }

        public Task<List<string>> GetSupportedFileTypesAsync()
        {
            return Task.FromResult(_supportedFileTypes.ToList());
        }

        public Task<bool> IsFileTypeSupportedAsync(string fileType)
        {
            return Task.FromResult(_supportedFileTypes.Contains(fileType.ToLower()));
        }

        public async Task<long> GetTotalStorageUsedAsync()
        {
            try
            {
                return await _context.Documents
                    .Where(d => d.Status != "Silindi")
                    .SumAsync(d => d.FileSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toplam depolama alanı hesaplanırken hata oluştu");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private DocumentDto MapToDto(Document document)
        {
            return new DocumentDto
            {
                Id = document.DocumentId,
                CustomerId = document.CustomerId,
                ClaimId = document.ClaimId,
                PolicyId = document.PolicyId,
                FileName = document.FileName ?? string.Empty,
                FileUrl = document.FileUrl ?? string.Empty,
                FileType = document.FileType ?? string.Empty,
                FileSize = document.FileSize,
                Category = document.Category ?? string.Empty,
                Status = document.Status ?? string.Empty,
                Description = document.Description ?? string.Empty,
                Version = document.Version ?? string.Empty,
                UploadedAt = document.UploadedAt,
                UpdatedAt = document.UpdatedAt,
                ExpiresAt = document.ExpiresAt,
                UploadedByUserId = document.UploadedByUserId,
                UploadedByUserName = document.UploadedByUser?.Name ?? "Bilinmeyen",
                CustomerName = document.Customer?.User?.Name ?? "Bilinmeyen",
                PolicyNumber = document.Policy?.PolicyNumber ?? string.Empty,
                ClaimNumber = document.Claim?.ClaimId.ToString() ?? string.Empty
            };
        }

        #endregion

        #region Policy Document Creation

        public async Task<DocumentDto> CreatePolicyDocumentAsync(int policyId, int customerId, int uploadedByUserId)
        {
            try
            {
                // Poliçe bilgilerini getir
                var policy = await _context.Policies
                    .Include(p => p.Offer)
                    .ThenInclude(o => o.Customer)
                    .ThenInclude(c => c.User)
                    .Include(p => p.Offer)
                    .ThenInclude(o => o.InsuranceType)
                    .FirstOrDefaultAsync(p => p.PolicyId == policyId);

                if (policy == null)
                {
                    throw new ArgumentException($"Policy not found: {policyId}");
                }

                // PolicyDto oluştur
                var policyDto = new PolicyDto
                {
                    PolicyId = policy.PolicyId,
                    OfferId = policy.OfferId,
                    StartDate = policy.StartDate,
                    EndDate = policy.EndDate,
                    PolicyNumber = policy.PolicyNumber,
                    TotalPremium = policy.TotalPremium,
                    Status = policy.Status,
                    Notes = policy.Notes,
                    CreatedAt = policy.CreatedAt,
                    UpdatedAt = policy.UpdatedAt,
                    Offer = policy.Offer != null ? new OfferDto
                    {
                        OfferId = policy.Offer.OfferId,
                        CustomerId = policy.Offer.CustomerId,
                        AgentId = policy.Offer.AgentId ?? 0,
                        InsuranceTypeId = policy.Offer.InsuranceTypeId,
                        Department = policy.Offer.Department ?? string.Empty,
                        BasePrice = policy.Offer.BasePrice,
                        DiscountRate = policy.Offer.DiscountRate,
                        FinalPrice = policy.Offer.FinalPrice,
                        Status = policy.Offer.Status ?? string.Empty,
                        ValidUntil = policy.Offer.ValidUntil,
                        CreatedAt = policy.Offer.CreatedAt,
                        UpdatedAt = policy.Offer.UpdatedAt,
                        InsuranceTypeName = policy.Offer.InsuranceType?.Name ?? "Bilinmeyen",
                        CoverageAmount = policy.Offer.CoverageAmount
                    } : null
                };

                // PDF oluştur
                Console.WriteLine($"🔍 DocumentService: Creating PDF for policy: {policy.PolicyNumber}");
                Console.WriteLine($"🔍 DocumentService: Policy data - ID: {policyDto.PolicyId}, Number: {policyDto.PolicyNumber}, Premium: {policyDto.TotalPremium}");
                
                byte[] pdfBytes;
                try
                {
                    pdfBytes = await _pdfService.CreatePolicyPdfAsync(policyDto);
                    Console.WriteLine($"✅ DocumentService: PDF created successfully, size: {pdfBytes.Length} bytes");
                }
                catch (Exception pdfEx)
                {
                    Console.WriteLine($"❌ DocumentService: PDF creation failed: {pdfEx.Message}");
                    throw new Exception($"Policy PDF oluşturma hatası: {pdfEx.Message}", pdfEx);
                }
                var fileName = $"Poliçe_{policy.PolicyNumber}_{DateTime.UtcNow:yyyyMMdd}.pdf";
                Console.WriteLine($"🔍 DocumentService: Saving PDF with filename: {fileName}");
                var fileUrl = await _pdfService.SavePdfAsync(pdfBytes, fileName);
                Console.WriteLine($"✅ DocumentService: PDF saved to: {fileUrl}");

                // Doküman oluştur
                var document = new Document
                {
                    FileName = fileName,
                    FileUrl = fileUrl,
                    FileType = "pdf",
                    FileSize = pdfBytes.Length,
                    Category = "Poliçe",
                    Description = $"Sigorta Poliçesi - {policy.PolicyNumber}",
                    Version = "1.0",
                    Status = "Active",
                    PolicyId = policyId,
                    CustomerId = customerId,
                    UploadedByUserId = uploadedByUserId,
                    UserId = uploadedByUserId,
                    UploadedAt = DateTime.UtcNow,
                    ExpiresAt = policy.EndDate.AddYears(5) // Poliçe bitiş tarihinden 5 yıl sonra
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Policy PDF document created: {fileName} for policy: {policy.PolicyNumber}");

                // Oluşturulan dokümanı detaylarıyla birlikte getir
                var createdDocument = await _context.Documents
                    .Include(d => d.Customer)
                    .ThenInclude(c => c.User)
                    .Include(d => d.Policy)
                    .Include(d => d.UploadedByUser)
                    .FirstOrDefaultAsync(d => d.DocumentId == document.DocumentId);

                return MapToDto(createdDocument!);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating policy document: {ex.Message}");
                throw;
            }
        }

        private string GeneratePolicyDocumentContent(Policy policy)
        {
            var offer = policy.Offer;
            var customer = offer?.Customer;
            var user = customer?.User;
            var insuranceType = offer?.InsuranceType;

            return $@"SİGORTA POLİÇESİ
==================

Poliçe Bilgileri:
------------------
Poliçe Numarası: {policy.PolicyNumber}
Sigorta Türü: {insuranceType?.Name ?? "Bilinmeyen"}
Başlangıç Tarihi: {policy.StartDate:dd.MM.yyyy}
Bitiş Tarihi: {policy.EndDate:dd.MM.yyyy}
Toplam Prim: ₺{policy.TotalPremium:N2}
Durum: {policy.Status}
Oluşturulma Tarihi: {policy.CreatedAt:dd.MM.yyyy HH:mm}

Müşteri Bilgileri:
------------------
Ad Soyad: {user?.Name ?? "Bilinmeyen"}
E-posta: {user?.Email ?? "Bilinmeyen"}
Kimlik No: {customer?.IdNo ?? "Bilinmeyen"}
Adres: {customer?.Address ?? "Bilinmeyen"}
Telefon: {customer?.Phone ?? "Bilinmeyen"}

Teklif Bilgileri:
------------------
Teklif Numarası: #{offer?.OfferId ?? 0}
Departman: {offer?.Department ?? "Bilinmeyen"}
Temel Fiyat: ₺{offer?.BasePrice ?? 0:N2}
İndirim Oranı: %{offer?.DiscountRate ?? 0:N2}
Final Fiyat: ₺{offer?.FinalPrice ?? 0:N2}
Teminat Tutarı: ₺{offer?.CoverageAmount ?? 0:N2}
Departman: {offer?.Department ?? "Bilinmeyen"}

Ek Bilgiler:
------------
{offer?.CustomerAdditionalInfo ?? "Ek bilgi bulunmuyor"}

Notlar:
-------
{policy.Notes ?? "Not bulunmuyor"}

---
Bu poliçe elektronik ortamda oluşturulmuştur.
Sigorta şirketi tarafından düzenlenmiştir.
Geçerlilik tarihi: {policy.EndDate:dd.MM.yyyy}
";
        }

        #endregion
    }
}
