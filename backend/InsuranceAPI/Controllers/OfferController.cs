using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;
using InsuranceAPI.Data;
using InsuranceAPI.Models;
using System.Security.Claims;
using System.Text.Json;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // JWT authentication aktif
    public class OfferController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly IPolicyService _policyService;
        private readonly IDocumentService _documentService;
        private readonly InsuranceDbContext _context;
        
        // Offer service dependency injection
        public OfferController(IOfferService offerService, IPolicyService policyService, IDocumentService documentService, InsuranceDbContext context)
        {
            _offerService = offerService;
            _policyService = policyService;
            _documentService = documentService;
            _context = context;
        }
        
        // Debug: Customer bilgilerini listele (sadece test için)
        [HttpGet("debug/customers")]
        [AllowAnonymous]
        public async Task<ActionResult> DebugGetAllCustomers()
        {
            try
            {
                var customers = await _context.Customers
                    .Include(c => c.User)
                    .ToListAsync();
                
                Console.WriteLine($"🔍 Debug: Found {customers.Count} customers in database");
                foreach (var customer in customers)
                {
                    Console.WriteLine($"  - Customer ID: {customer.CustomerId}, User ID: {customer.UserId}, Email: {customer.User?.Email}");
                }
                
                return Ok(new { 
                    count = customers.Count, 
                    customers = customers.Select(c => new {
                        customerId = c.CustomerId,
                        userId = c.UserId,
                        email = c.User?.Email,
                        name = c.User?.Name
                    })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Debug customers error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Debug: Tüm teklifleri listele (sadece test için)
        [HttpGet("debug/all")]
        [AllowAnonymous]
        public async Task<ActionResult> DebugGetAllOffers()
        {
            try
            {
                var offers = await _context.Offers
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.Agent)
                        .ThenInclude(a => a!.User)
                    .Include(o => o.InsuranceType)
                    .ToListAsync();
                
                Console.WriteLine($"🔍 Debug: Found {offers.Count} offers in database");
                foreach (var offer in offers)
                {
                    Console.WriteLine($"  - Offer ID: {offer.OfferId}, Customer ID: {offer.CustomerId}, Status: {offer.Status}, Insurance: {offer.InsuranceType?.Name}");
                }
                
                return Ok(new { 
                    count = offers.Count, 
                    offers = offers.Select(o => new {
                        offerId = o.OfferId,
                        customerId = o.CustomerId,
                        status = o.Status,
                        insuranceType = o.InsuranceType?.Name,
                        basePrice = o.BasePrice,
                        finalPrice = o.FinalPrice,
                        discountRate = o.DiscountRate
                    })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Debug error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Tüm teklifleri getir
        [HttpGet]
        public async Task<ActionResult<List<OfferDto>>> GetAllOffers()
        {
            try
            {
                // JWT token'dan kullanıcı rolünü al
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                Console.WriteLine($"OfferController.GetAllOffers - UserRole: {userRole}, UserId: {userId}");
                Console.WriteLine($"All Claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
                
                List<OfferDto> offers;
                
                if (userRole == "admin")
                {
                    Console.WriteLine("Admin kullanıcısı - Tüm teklifler getiriliyor");
                    // Admin tüm teklifleri görebilir
                    offers = await _offerService.GetAllOffersAsync();
                }
                else if (userRole == "agent")
                {
                    Console.WriteLine($"Agent kullanıcısı (ID: {userId}) - Departman teklifleri getiriliyor");
                    // Agent sadece kendi departmanındaki teklifleri görebilir
                    offers = await _offerService.GetOffersByAgentDepartmentAsync(userId);
                }
                else
                {
                    Console.WriteLine($"Customer kullanıcısı (ID: {userId}) - Kendi teklifleri getiriliyor");
                    // Customer sadece kendi tekliflerini görebilir
                    offers = await _offerService.GetOffersByCustomerAsync(userId);
                }
                
                Console.WriteLine($"Toplam {offers.Count} teklif döndürüldü");
                return Ok(offers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OfferController.GetAllOffers - Hata: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Teklif verileri alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // ID'ye göre teklif getir
        [HttpGet("{id}")]
        public async Task<ActionResult<OfferDto>> GetOfferById(int id)
        {
            var offer = await _offerService.GetOfferByIdAsync(id);
            
            if (offer == null)
            {
                return NotFound(new { message = "Teklif bulunamadı" });
            }
            
            return Ok(offer);
        }
        
        // Yeni teklif oluştur
        [HttpPost]
        public async Task<ActionResult<OfferDto>> CreateOffer([FromBody] CreateOfferDto createOfferDto)
        {
            Console.WriteLine($"🔍 CreateOffer called with data: {System.Text.Json.JsonSerializer.Serialize(createOfferDto)}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"❌ ModelState validation failed: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequest(ModelState);
            }

            // Customer ID validation
            if (createOfferDto.CustomerId <= 0)
            {
                return BadRequest(new { message = "Geçersiz Customer ID. Customer ID 0'dan büyük olmalıdır." });
            }

            // InsuranceType ID validation
            if (createOfferDto.InsuranceTypeId <= 0)
            {
                return BadRequest(new { message = "Geçersiz InsuranceType ID. InsuranceType ID 0'dan büyük olmalıdır." });
            }

            // Kullanıcı rolünü al
            var userRole = User.FindFirst("role")?.Value;
            Console.WriteLine($"🔍 CreateOffer - UserRole: {userRole}");

            // BasePrice validation - sadece agent ve admin için (customer için bypass)
            if (userRole != "customer" && createOfferDto.BasePrice < 0)
            {
                Console.WriteLine($"❌ BasePrice validation failed for role: {userRole}, BasePrice: {createOfferDto.BasePrice}");
                return BadRequest(new { message = "Geçersiz BasePrice. BasePrice 0'a eşit veya büyük olmalıdır." });
            }
            Console.WriteLine($"✅ BasePrice validation - Role: {userRole}, BasePrice: {createOfferDto.BasePrice}");

            // FinalPrice validation - sadece agent ve admin için (customer için bypass)
            if (userRole != "customer" && createOfferDto.FinalPrice < 0)
            {
                Console.WriteLine($"❌ FinalPrice validation failed for role: {userRole}, FinalPrice: {createOfferDto.FinalPrice}");
                return BadRequest(new { message = "Geçersiz FinalPrice. FinalPrice 0'a eşit veya büyük olmalıdır." });
            }
            Console.WriteLine($"✅ FinalPrice validation - Role: {userRole}, FinalPrice: {createOfferDto.FinalPrice}");

            // Status validation
            if (string.IsNullOrEmpty(createOfferDto.Status))
            {
                return BadRequest(new { message = "Status alanı boş olamaz." });
            }

            // Sigorta türüne özel validation
            var validationResult = await ValidateOfferByInsuranceType(createOfferDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            Console.WriteLine($"🔍 OfferController.CreateOffer - Customer ID: {createOfferDto.CustomerId}");
            
            // Tüm customers'ları listele (debug için)
            var allCustomers = await _context.Customers.ToListAsync();
            Console.WriteLine($"🔍 Total customers in DB: {allCustomers.Count}");
            foreach (var c in allCustomers)
            {
                Console.WriteLine($"   Customer ID: {c.CustomerId}, User ID: {c.UserId}");
            }

            // Customer'ın var olup olmadığını kontrol et
            var customer = await _context.Customers.FindAsync(createOfferDto.CustomerId);
            Console.WriteLine($"🔍 Customer lookup result: {customer?.CustomerId.ToString() ?? "NULL"}");
            
            if (customer == null)
            {
                return BadRequest(new { message = $"Customer ID {createOfferDto.CustomerId} bulunamadı." });
            }

            Console.WriteLine($"OfferController.CreateOffer - Customer ID: {createOfferDto.CustomerId}, Customer found: {customer.CustomerId}");
            Console.WriteLine($"OfferController.CreateOffer - InsuranceTypeId: {createOfferDto.InsuranceTypeId}");
            Console.WriteLine($"OfferController.CreateOffer - Calling CreateOfferAsync...");
            
            var result = await _offerService.CreateOfferAsync(createOfferDto);
            
            Console.WriteLine($"OfferController.CreateOffer - CreateOfferAsync result: {(result != null ? result.OfferId.ToString() : "NULL")}");
            
            if (result == null)
            {
                return BadRequest(new { message = "Teklif oluşturulamadı. Müşteri bulunamadı." });
            }

            // Müşteri tarafından yüklenen dosyaları Document tablosuna ekle
            if (!string.IsNullOrEmpty(createOfferDto.CustomerAdditionalInfo))
            {
                try
                {
                    Console.WriteLine($"🔍 CreateOffer: Processing customerAdditionalInfo for offer {result.OfferId}");
                    var additionalInfo = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(createOfferDto.CustomerAdditionalInfo);
                    var documentRecords = new List<Document>();

                    foreach (var kvp in additionalInfo)
                    {
                        var value = kvp.Value?.ToString();
                        
                        if (!string.IsNullOrEmpty(value) && value.Contains("/uploads/"))
                        {
                            // Dosya URL'sini çıkar
                            var fileUrl = value.Contains("(") && value.Contains(")") 
                                ? value.Substring(value.IndexOf("(") + 1, value.IndexOf(")") - value.IndexOf("(") - 1)
                                : value;

                            // Dosya adını çıkar
                            var fileName = value.Contains("(") 
                                ? value.Substring(0, value.IndexOf("(")).Trim()
                                : Path.GetFileName(fileUrl);

                            // Dosya türünü belirle
                            var fileType = "application/pdf"; // Default
                            if (fileUrl.Contains(".jpg") || fileUrl.Contains(".jpeg") || fileUrl.Contains(".png"))
                            {
                                fileType = "image/jpeg";
                            }

                            var document = new Document
                            {
                                Category = GetDocumentCategory(kvp.Key),
                                FileName = fileName,
                                FileType = fileType,
                                FileUrl = fileUrl,
                                FileSize = 0, // Will be updated if needed
                                Description = $"{GetDocumentCategory(kvp.Key)} - Teklif #{result.OfferId}",
                                Status = "Active",
                                UploadedAt = DateTime.UtcNow,
                                UserId = customer.UserId,
                                CustomerId = result.CustomerId,
                                UploadedByUserId = customer.UserId
                            };

                            documentRecords.Add(document);
                            Console.WriteLine($"📄 Created document record: {fileName} -> {fileUrl}");
                        }
                    }

                    if (documentRecords.Any())
                    {
                        _context.Documents.AddRange(documentRecords);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ Added {documentRecords.Count} document records from customerAdditionalInfo for offer {result.OfferId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error parsing customerAdditionalInfo in CreateOffer: {ex.Message}");
                    // Don't fail the offer creation if document parsing fails
                }
            }
            
            return CreatedAtAction(nameof(GetOfferById), new { id = result.OfferId }, result);
        }
        
        // Sigorta türüne özel validation method'u
        private async Task<(bool IsValid, string ErrorMessage)> ValidateOfferByInsuranceType(CreateOfferDto dto)
        {
            var insuranceType = await _context.InsuranceTypes.FindAsync(dto.InsuranceTypeId);
            if (insuranceType == null)
            {
                return (false, "Sigorta türü bulunamadı.");
            }

            switch (insuranceType.Name.ToLower())
            {
                case "konut sigortası":
                    return ValidateKonutSigortasi(dto);
                case "seyahat sigortası":
                    return ValidateSeyahatSigortasi(dto);
                case "iş yeri sigortası":
                    return ValidateIsYeriSigortasi(dto);
                case "trafik sigortası":
                    return ValidateTrafikSigortasi(dto);
                case "sağlık sigortası":
                    return ValidateSaglikSigortasi(dto);
                case "hayat sigortası":
                    return ValidateHayatSigortasi(dto);
                default:
                    return (true, ""); // Bilinmeyen tür için temel validation
            }
        }

        // Konut Sigortası Validation
        private (bool IsValid, string ErrorMessage) ValidateKonutSigortasi(CreateOfferDto dto)
        {
            if (string.IsNullOrEmpty(dto.CustomerAdditionalInfo))
                return (false, "Konut sigortası için adres bilgisi gereklidir.");
            
            // Kapsam tutarı validation kaldırıldı
            
            return (true, "");
        }

        // Seyahat Sigortası Validation
        private (bool IsValid, string ErrorMessage) ValidateSeyahatSigortasi(CreateOfferDto dto)
        {
            // Bugünün başlangıcını al (saat 00:00:00)
            var today = DateTime.UtcNow.Date;
            
            if (dto.RequestedStartDate <= today)
                return (false, "Seyahat sigortası için gelecek bir tarih seçilmelidir.");
            
            // Kapsam tutarı validation kaldırıldı
            
            return (true, "");
        }

        // İş Yeri Sigortası Validation
        private (bool IsValid, string ErrorMessage) ValidateIsYeriSigortasi(CreateOfferDto dto)
        {
            if (string.IsNullOrEmpty(dto.Department))
                return (false, "İş yeri sigortası için departman bilgisi gereklidir.");
            
            // Kapsam tutarı validation kaldırıldı
            
            return (true, "");
        }

        // Trafik Sigortası Validation
        private (bool IsValid, string ErrorMessage) ValidateTrafikSigortasi(CreateOfferDto dto)
        {
            // Kullanıcı rolünü al
            var userRole = User.FindFirst("role")?.Value;
            
            // BasePrice validation - sadece agent ve admin için (minimum 0 TL)
            if (userRole != "customer" && dto.BasePrice < 0)
                return (false, "Trafik sigortası için minimum temel fiyat 0 TL olmalıdır.");
            
            return (true, "");
        }

        // Sağlık Sigortası Validation
        private (bool IsValid, string ErrorMessage) ValidateSaglikSigortasi(CreateOfferDto dto)
        {
            // Kapsam tutarı validation kaldırıldı
            
            return (true, "");
        }

        // Hayat Sigortası Validation
        private (bool IsValid, string ErrorMessage) ValidateHayatSigortasi(CreateOfferDto dto)
        {
            // Kullanıcı rolünü al
            var userRole = User.FindFirst("role")?.Value;
            
            // Kapsam tutarı validation kaldırıldı
            
            // BasePrice validation - sadece agent ve admin için
            if (userRole != "customer" && dto.BasePrice < 1000)
                return (false, "Hayat sigortası için minimum temel fiyat 1.000 TL olmalıdır.");
            
            return (true, "");
        }
        
        // Teklif güncelle - Admin ve Agent'lar güncelleyebilir
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<OfferDto>> UpdateOffer(int id, [FromBody] UpdateOfferDto updateOfferDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // JWT token'dan user ID'sini al
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "";
            
            // Teklifi kontrol et
            var offer = await _context.Offers
                .Include(o => o.Agent)
                .FirstOrDefaultAsync(o => o.OfferId == id);
                
            if (offer == null)
            {
                return NotFound(new { message = "Teklif bulunamadı" });
            }
            
            // Müşteri onaylamış teklifler düzenlenemez
            if (offer.IsCustomerApproved)
            {
                return BadRequest(new { message = "Müşteri tarafından onaylanmış teklifler düzenlenemez. Bu teklifi yalnızca görüntüleyebilir veya silebilirsiniz." });
            }
            
            // Agent ise sadece kendi departmanındaki teklifleri güncelleyebilir
            if (userRole == "agent")
            {
                var agent = await _context.Agents.FirstOrDefaultAsync(a => a.UserId == userId);
                if (agent == null || agent.Department != offer.Department)
                {
                    return Forbid("Bu teklifi güncelleme yetkiniz yok. Sadece kendi departmanınızdaki teklifleri güncelleyebilirsiniz.");
                }
            }
            
            // İndirim oranı belirtilmişse final fiyatı hesapla
            if (updateOfferDto.DiscountRate.HasValue)
            {
                var discountSuccess = await _offerService.UpdateOfferWithDiscountAsync(id, updateOfferDto.DiscountRate, updateOfferDto.FinalPrice);
                if (!discountSuccess)
                {
                    return BadRequest(new { message = "İndirim hesaplaması yapılamadı" });
                }
            }
            
            var result = await _offerService.UpdateOfferAsync(id, updateOfferDto);
            
            if (result == null)
            {
                return NotFound(new { message = "Teklif bulunamadı" });
            }
            
            return Ok(result);
        }
        
        // Teklif sil - Admin ve Agent'lar silebilir
        [HttpDelete("{id}")]
        [Authorize] // JWT authentication required
        public async Task<ActionResult> DeleteOffer(int id)
        {
            // JWT token'dan kullanıcı bilgilerini al
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            
            Console.WriteLine($"DeleteOffer - UserId: {userId}, UserRole: {userRole}");
            
            // Role kontrolü - admin, agent ve customer silebilir (customer sadece kendi aktif taleplerini)
            if (userRole != "admin" && userRole != "agent" && userRole != "customer")
            {
                Console.WriteLine($"Unauthorized role: {userRole}");
                return StatusCode(403, new { message = "Bu işlem için yetkiniz yok." });
            }
            
            // Teklifi kontrol et
            var offer = await _context.Offers
                .Include(o => o.Agent)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OfferId == id);
                
            if (offer == null)
            {
                return NotFound(new { message = "Teklif bulunamadı" });
            }
            
            // Role bazlı yetki kontrolü
            if (userRole == "customer")
            {
                // Customer'lar sadece ödeme yapılmamış teklifleri silebilir
                // (Kendi tekliflerini silebilir, başkalarının tekliflerini de silebilir - aktif durumda olanları)
                if (offer.Status == "paid" || offer.Status == "active")
                {
                    return StatusCode(400, new { message = "Ödeme yapılmış teklifler silinemez." });
                }
            }
            else if (userRole == "agent")
            {
                // Agent sadece kendi departmanındaki teklifleri silebilir
                var agent = await _context.Agents.FirstOrDefaultAsync(a => a.UserId == userId);
                if (agent == null || agent.Department != offer.Department)
                {
                    return StatusCode(403, new { message = "Bu teklifi silme yetkiniz yok. Sadece kendi departmanınızdaki teklifleri silebilirsiniz." });
                }
            }
            // Admin tüm teklifleri silebilir
            
            var success = await _offerService.DeleteOfferAsync(id);
            
            if (!success)
            {
                return NotFound(new { message = "Teklif bulunamadı" });
            }
            
            return NoContent();
        }
        
        // Duruma göre teklifleri getir
        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<OfferDto>>> GetOffersByStatus(string status)
        {
            var offers = await _offerService.GetOffersByStatusAsync(status);
            return Ok(offers);
        }
        
        // Teklif arama
        [HttpGet("search")]
        public async Task<ActionResult<List<OfferDto>>> SearchOffers([FromQuery] string? insuranceType, [FromQuery] string? status, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
        {
            var offers = await _offerService.SearchOffersAsync(insuranceType, status, minPrice, maxPrice);
            return Ok(offers);
        }
        
        // Sigorta türlerini getir (veritabanından)
        [HttpGet("types")]
        public async Task<ActionResult<List<InsuranceTypeDto>>> GetInsuranceTypes()
        {
            var insuranceTypes = await _context.InsuranceTypes
                .Where(it => it.IsActive)
                .OrderBy(it => it.Category)
                .ThenBy(it => it.Name)
                .Select(it => new InsuranceTypeDto
                {
                    Id = it.InsuranceTypeId,
                    Name = it.Name ?? string.Empty,
                    Category = it.Category ?? string.Empty,
                    Description = it.Description ?? string.Empty,
                    IsActive = it.IsActive,
                    BasePrice = it.BasePrice,
                    CoverageDetails = it.CoverageDetails ?? string.Empty,
                    CreatedAt = it.CreatedAt,
                    UpdatedAt = it.UpdatedAt
                })
                .ToListAsync();
                
            return Ok(insuranceTypes);
        }
        
        // Departman bazlı teklifleri getir (Agent için)
        [HttpGet("department/{department}")]
        [Authorize(Roles = "agent")] // Admin departman bazlı teklifleri göremez
        public async Task<ActionResult<List<OfferDto>>> GetOffersByDepartment(string department)
        {
            // JWT token'dan agent ID'sini al
            var agentId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Agent'ın departmanı ile istenen departman eşleşiyor mu?
            var agent = await _context.Agents.FirstOrDefaultAsync(a => a.UserId == agentId);
            if (agent == null || agent.Department != department)
            {
                return Forbid("Bu departmanın tekliflerini görüntüleme yetkiniz yok. Sadece kendi departmanınızın tekliflerini görebilirsiniz.");
            }
            
            var offers = await _offerService.GetOffersByDepartmentAsync(department);
            return Ok(offers);
        }
        
        // Agent'ın departmanına göre teklifleri getir
        [HttpGet("agent/{agentId}/department")]
        [Authorize(Roles = "agent")] // Admin agent departman tekliflerini göremez
        public async Task<ActionResult<List<OfferDto>>> GetOffersByAgentDepartment(int agentId)
        {
            // JWT token'dan giriş yapan agent ID'sini al
            var currentAgentId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Sadece kendi departmanındaki teklifleri görebilir
            var currentAgent = await _context.Agents.FirstOrDefaultAsync(a => a.UserId == currentAgentId);
            var targetAgent = await _context.Agents.FirstOrDefaultAsync(a => a.UserId == agentId);
            
            if (currentAgent == null || targetAgent == null || currentAgent.Department != targetAgent.Department)
            {
                return Forbid("Bu agent'ın departman tekliflerini görüntüleme yetkiniz yok. Sadece kendi departmanınızdaki agent'ların tekliflerini görebilirsiniz.");
            }
            
            var offers = await _offerService.GetOffersByAgentDepartmentAsync(agentId);
            return Ok(offers);
        }
        
        // Admin için tüm teklifleri getir (departman filtresi olmadan)
        [HttpGet("admin/all")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<OfferDto>>> GetAllOffersForAdmin()
        {
            var offers = await _offerService.GetAllOffersForAdminAsync();
            return Ok(offers);
        }
        
        // Teklif durumlarını getir
        [HttpGet("statuses")]
        public ActionResult GetOfferStatuses()
        {
            var statuses = new[]
            {
                new { Value = "pending", Label = "Beklemede" },
                new { Value = "reviewed", Label = "İncelendi" },
                new { Value = "approved", Label = "Onaylandı" },
                new { Value = "customer_approved", Label = "Müşteri Onayladı" },
                new { Value = "rejected", Label = "Reddedildi" },
                new { Value = "expired", Label = "Süresi Doldu" },
                new { Value = "paid", Label = "Ödendi" }
            };
            
            return Ok(statuses);
        }

        // Ödeme sonrası poliçe ve doküman oluştur
        [HttpPost("{offerId}/create-policy")]
        [Authorize]
        public async Task<ActionResult> CreatePolicyFromPayment(int offerId, [FromBody] CreatePolicyFromPaymentDto dto)
        {
            try
            {
                // Kullanıcı ID'sini al
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Teklifi kontrol et
                var offer = await _context.Offers
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OfferId == offerId);

                if (offer == null)
                {
                    return NotFound(new { message = "Teklif bulunamadı" });
                }

                // Müşteri kontrolü
                if (offer.Customer?.UserId != userId)
                {
                    return Forbid("Bu teklif için poliçe oluşturma yetkiniz yok");
                }

                // Poliçe oluştur
                var policy = await _policyService.CreatePolicyFromPaymentAsync(offerId, dto.PaymentAmount, userId);
                
                if (policy == null)
                {
                    return BadRequest(new { message = "Poliçe oluşturulamadı" });
                }

                // Ödeme kaydı oluştur
                var payment = new Payment
                {
                    PolicyId = policy.PolicyId,
                    Amount = dto.PaymentAmount,
                    Method = dto.PaymentMethod,
                    TransactionId = dto.TransactionId,
                    Status = "Completed",
                    PaidAt = DateTime.UtcNow,
                    Notes = $"Ödeme - Teklif #{offerId}",
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                _context.Payments.Add(payment);

                // Makbuz belgesi oluştur
                var receiptDocument = new Document
                {
                    Category = "Makbuz",
                    FileName = $"makbuz_{dto.TransactionId}.pdf",
                    FileType = "application/pdf",
                    FileUrl = $"/documents/receipts/makbuz_{dto.TransactionId}.pdf",
                    FileSize = 0, // Will be updated when actual file is created
                    Description = $"Ödeme makbuzu - Teklif #{offerId}",
                    Status = "Active",
                    UploadedAt = DateTime.UtcNow,
                    UserId = userId,
                    CustomerId = offer.CustomerId,
                    PolicyId = policy.PolicyId,
                    UploadedByUserId = userId
                };

                _context.Documents.Add(receiptDocument);

                // Müşteri tarafından yüklenen dosyaları Document tablosuna ekle
                if (!string.IsNullOrEmpty(offer.CustomerAdditionalInfo))
                {
                    try
                    {
                        var additionalInfo = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(offer.CustomerAdditionalInfo);
                        var documentRecords = new List<Document>();

                        foreach (var kvp in additionalInfo)
                        {
                            var value = kvp.Value?.ToString();
                            if (!string.IsNullOrEmpty(value) && value.Contains("/uploads/"))
                            {
                                // Dosya URL'sini çıkar
                                var fileUrl = value.Contains("(") && value.Contains(")") 
                                    ? value.Substring(value.IndexOf("(") + 1, value.IndexOf(")") - value.IndexOf("(") - 1)
                                    : value;

                                // Dosya adını çıkar
                                var fileName = value.Contains("(") 
                                    ? value.Substring(0, value.IndexOf("(")).Trim()
                                    : Path.GetFileName(fileUrl);

                                // Dosya türünü belirle
                                var fileType = "application/pdf"; // Default
                                if (fileUrl.Contains(".jpg") || fileUrl.Contains(".jpeg") || fileUrl.Contains(".png"))
                                {
                                    fileType = "image/jpeg";
                                }

                                var document = new Document
                                {
                                    Category = GetDocumentCategory(kvp.Key),
                                    FileName = fileName,
                                    FileType = fileType,
                                    FileUrl = fileUrl,
                                    FileSize = 0, // Will be updated if needed
                                    Description = $"{GetDocumentCategory(kvp.Key)} - Teklif #{offerId}",
                                    Status = "Active",
                                    UploadedAt = DateTime.UtcNow,
                                    UserId = userId,
                                    CustomerId = offer.CustomerId,
                                    PolicyId = policy.PolicyId,
                                    UploadedByUserId = userId
                                };

                                documentRecords.Add(document);
                                Console.WriteLine($"📄 Created document record: {fileName} -> {fileUrl}");
                            }
                        }

                        if (documentRecords.Any())
                        {
                            _context.Documents.AddRange(documentRecords);
                            Console.WriteLine($"✅ Added {documentRecords.Count} document records from customerAdditionalInfo");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error parsing customerAdditionalInfo: {ex.Message}");
                        // Don't fail the payment process if document parsing fails
                    }
                }

                // Teklif durumunu güncelle
                offer.Status = "paid";
                offer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Policy, payment and receipt document created successfully for offer: {offerId}");

                return Ok(new { 
                    message = "Poliçe ve ödeme başarıyla oluşturuldu",
                    policy = policy,
                    payment = new {
                        id = payment.PaymentId,
                        amount = payment.Amount,
                        transactionId = payment.TransactionId,
                        status = payment.Status,
                        paymentDate = payment.PaidAt
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating policy from payment: {ex.Message}");
                return StatusCode(500, new { message = "Poliçe oluşturma hatası", error = ex.Message });
            }
        }
        
        // Müşteri teklif talebi oluştur
        [HttpPost("customer-request")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<OfferDto>> CreateCustomerQuoteRequest([FromBody] CustomerQuoteRequestDto requestDto)
        {
            try
            {
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Müşteri bilgilerini al
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    return NotFound(new { message = "Müşteri bulunamadı" });
                }
                
                // Varsayılan agent'ı bul (departmana göre)
                var defaultAgent = await _context.Agents
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Department == requestDto.ServiceType);
                
                if (defaultAgent == null)
                {
                    return BadRequest(new { message = "Bu hizmet için uygun acente bulunamadı" });
                }
                
                // InsuranceType'ı bul
                var insuranceType = await _context.InsuranceTypes
                    .FirstOrDefaultAsync(it => it.Name.ToLower().Contains(requestDto.ServiceType.ToLower()));
                
                if (insuranceType == null)
                {
                    return BadRequest(new { message = "Sigorta türü bulunamadı" });
                }
                
                // Yeni offer oluştur
                var offer = new Models.Offer
                {
                    CustomerId = customer.CustomerId,
                    AgentId = defaultAgent.AgentId,
                    InsuranceTypeId = insuranceType.InsuranceTypeId,
                    Department = $"Müşteri talebi: {requestDto.ServiceType}",
                    BasePrice = insuranceType.BasePrice,
                    DiscountRate = 0,
                    FinalPrice = insuranceType.BasePrice,
                    Status = "pending",
                    ValidUntil = CalculateValidityPeriod(insuranceType),
                    CustomerAdditionalInfo = requestDto.AdditionalInfo,
                    CoverageAmount = decimal.TryParse(requestDto.CoverageAmount, out var amount) ? amount : 0m,
                    RequestedStartDate = requestDto.StartDate,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Offers.Add(offer);
                await _context.SaveChangesAsync();
                
                // DTO'ya çevir
                var result = await _offerService.GetOfferByIdAsync(offer.OfferId);
                return CreatedAtAction(nameof(GetOfferById), new { id = offer.OfferId }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Teklif talebi oluşturulamadı", error = ex.Message });
            }
        }
        
        // Acente teklif düzenleme
        [HttpPut("{id}/agent-review")]
        [Authorize(Roles = "agent,admin")]
        public async Task<ActionResult<OfferDto>> AgentReviewOffer(int id, [FromBody] AgentQuoteUpdateDto updateDto)
        {
            try
            {
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                
                // Offer'ı bul
                var offer = await _context.Offers
                    .Include(o => o.Agent)
                    .FirstOrDefaultAsync(o => o.OfferId == id);
                
                if (offer == null)
                {
                    return NotFound(new { message = "Teklif bulunamadı" });
                }
                
                // Müşteri onaylamış teklifler düzenlenemez
                if (offer.IsCustomerApproved)
                {
                    return BadRequest(new { message = "Müşteri tarafından onaylanmış teklifler düzenlenemez. Bu teklifi yalnızca görüntüleyebilir veya silebilirsiniz." });
                }
                
                // Yetki kontrolü
                if (userRole == "agent" && offer.AgentId != userId)
                {
                    return Forbid();
                }
                
                // Güncelle
                offer.FinalPrice = updateDto.FinalPrice ?? offer.FinalPrice;
                offer.Status = updateDto.Status;
                offer.ValidUntil = updateDto.ValidUntil ?? offer.ValidUntil;
                offer.ReviewedAt = DateTime.UtcNow;
                offer.ReviewedBy = userId;
                offer.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                var result = await _offerService.GetOfferByIdAsync(offer.OfferId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Teklif düzenlenemedi", error = ex.Message });
            }
        }
        
        // Müşteri teklif onayı
        [HttpPut("{id}/customer-approval")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<OfferDto>> CustomerApproval(int id, [FromBody] CustomerQuoteApprovalDto approvalDto)
        {
            try
            {
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Offer'ı bul
                var offer = await _context.Offers
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OfferId == id);
                
                if (offer == null)
                {
                    return NotFound(new { message = "Teklif bulunamadı" });
                }
                
                // Yetki kontrolü
                if (offer.Customer.UserId != userId)
                {
                    return Forbid();
                }
                
                // Güncelle
                offer.IsCustomerApproved = approvalDto.IsApproved;
                offer.CustomerApprovedAt = approvalDto.IsApproved ? DateTime.UtcNow : null;
                offer.Status = approvalDto.IsApproved ? "customer_approved" : "rejected";
                offer.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                var result = await _offerService.GetOfferByIdAsync(offer.OfferId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Teklif onayı işlenemedi", error = ex.Message });
            }
        }

        // Müşteriye göre teklifleri getir
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<OfferDto>>> GetOffersByCustomer(int customerId)
        {
            try
            {
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                
                Console.WriteLine($"🔍 GetOffersByCustomer - CustomerId: {customerId}, UserId: {userId}, UserRole: {userRole}");
                
                // Müşteri kontrolü - sadece kendi tekliflerini görebilir
                if (userRole == "customer")
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                    Console.WriteLine($"🔍 Customer lookup - Found: {customer?.CustomerId}, UserId: {customer?.UserId}");
                    
                    // Check both CustomerId and UserId since database might use either
                    if (customer == null || (customer.CustomerId != customerId && customer.UserId != customerId))
                    {
                        Console.WriteLine($"❌ Access denied - Customer: {customer?.CustomerId}, UserId: {customer?.UserId}, Requested: {customerId}");
                        return Forbid("Bu teklifleri görme yetkiniz yok.");
                    }
                    
                    // Use the correct customer ID for the query
                    var actualCustomerId = customer.CustomerId;
                    Console.WriteLine($"✅ Using customer ID: {actualCustomerId} for query");
                    customerId = actualCustomerId;
                }
                
                var offers = await _offerService.GetOffersByCustomerAsync(customerId);
                Console.WriteLine($"✅ Found {offers.Count} offers for customer {customerId}");
                
                // Debug: Her teklifin detaylarını logla
                foreach (var offer in offers)
                {
                    Console.WriteLine($"📋 Offer {offer.OfferId}: Status={offer.Status}, CustomerId={offer.CustomerId}, InsuranceType={offer.InsuranceType?.Name}");
                }
                
                return Ok(offers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetOffersByCustomer error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Teklif verileri alınırken hata oluştu", error = ex.Message });
            }
        }

        // Teklif onayını güncelle
        [HttpPut("{id}/approval")]
        public async Task<ActionResult<OfferDto>> UpdateOfferApproval(int id, [FromBody] OfferApprovalDto approvalDto)
        {
            try
            {
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                
                // Offer'ı bul
                var offer = await _context.Offers
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OfferId == id);
                
                if (offer == null)
                {
                    return NotFound(new { message = "Teklif bulunamadı" });
                }
                
                // Yetki kontrolü - sadece teklif sahibi onaylayabilir
                if (userRole == "customer")
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer == null || customer.CustomerId != offer.CustomerId)
                    {
                        return Forbid("Bu teklifi onaylama yetkiniz yok.");
                    }
                }
                
                // Güncelle
                offer.IsCustomerApproved = approvalDto.IsCustomerApproved;
                offer.CustomerApprovedAt = approvalDto.IsCustomerApproved ? DateTime.UtcNow : null;
                offer.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                var result = await _offerService.GetOfferByIdAsync(offer.OfferId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Teklif onayı güncellenemedi", error = ex.Message });
            }
        }

        // Geçerlilik süresi hesaplama
        private DateTime CalculateValidityPeriod(InsuranceType insuranceType)
        {
            int validityDays = insuranceType.ValidityPeriodDays;
            
            // Sigorta türüne göre özel geçerlilik süreleri
            switch (insuranceType.Name.ToLower())
            {
                case "trafik sigortası":
                case "trafik":
                    validityDays = 365; // 1 yıl
                    break;
                case "konut sigortası":
                case "konut":
                    validityDays = 365; // 1 yıl
                    break;
                case "seyahat sigortası":
                case "seyahat":
                    validityDays = 30; // 1 ay
                    break;
                case "iş yeri sigortası":
                case "iş yeri":
                case "isyeri":
                    validityDays = 365; // 1 yıl
                    break;
                case "sağlık sigortası":
                case "saglik":
                    validityDays = 365; // 1 yıl
                    break;
                case "hayat sigortası":
                case "hayat":
                    validityDays = 365; // 1 yıl
                    break;
                default:
                    validityDays = 30; // Varsayılan 1 ay
                    break;
            }
            
            Console.WriteLine($"📅 Controller: Calculated validity period for '{insuranceType.Name}': {validityDays} days");
            return DateTime.UtcNow.AddDays(validityDays);
        }

        private string GetDocumentCategory(string key)
        {
            return key switch
            {
                "deedDocument" => "Tapu Belgesi",
                "healthReport" => "Sağlık Raporu",
                "annualRevenueReport" => "Yıllık Gelir Raporu",
                "riskReport" => "Risk Raporu",
                "idFrontPhoto" => "Kimlik Ön Yüz",
                "idBackPhoto" => "Kimlik Arka Yüz",
                "accidentHistory" => "Kaza Geçmişi",
                _ => "Teklif Belgesi"
            };
        }

    }
}

