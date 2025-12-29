using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;

namespace InsuranceAPI.Services
{
    public class OfferService : IOfferService
    {
        private readonly InsuranceDbContext _context;
        
        // Offer service constructor - dependency injection
        public OfferService(InsuranceDbContext context)
        {
            _context = context;
        }
        
        // Tüm teklifleri getir
        public async Task<List<OfferDto>> GetAllOffersAsync()
        {
            try
            {
                var offers = await _context.Offers
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.Agent)
                        .ThenInclude(a => a.User)
                    .Include(o => o.InsuranceType)
                    .OrderBy(o => o.OfferId)
                    .ToListAsync();
                    
                Console.WriteLine($"Found {offers.Count} offers");
                return offers.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllOffersAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }
        
        // ID'ye göre teklif getir
        public async Task<OfferDto?> GetOfferByIdAsync(int id)
        {
            var offer = await _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .FirstOrDefaultAsync(o => o.OfferId == id);
                
            return offer != null ? MapToDto(offer) : null;
        }
        
        // Yeni teklif oluştur
        public async Task<OfferDto?> CreateOfferAsync(CreateOfferDto createOfferDto)
        {
            try
            {
                Console.WriteLine($"OfferService.CreateOfferAsync - Başlangıç: CustomerId={createOfferDto.CustomerId}, InsuranceTypeId={createOfferDto.InsuranceTypeId}");
                
                // Müşteri kontrolü
                var customer = await _context.Customers.FindAsync(createOfferDto.CustomerId);
                if (customer == null)
                {
                    Console.WriteLine($"Müşteri bulunamadı: {createOfferDto.CustomerId}");
                    return null;
                }
                Console.WriteLine($"Müşteri bulundu: {customer.CustomerId}");
                
                // InsuranceType kontrolü
                Console.WriteLine($"InsuranceTypeId aranıyor: {createOfferDto.InsuranceTypeId}");
                var insuranceType = await _context.InsuranceTypes.FindAsync(createOfferDto.InsuranceTypeId);
                if (insuranceType == null)
                {
                    Console.WriteLine($"InsuranceType bulunamadı: {createOfferDto.InsuranceTypeId}");
                    return null;
                }
                Console.WriteLine($"InsuranceType bulundu: {insuranceType.Name}, ID: {insuranceType.InsuranceTypeId}");
                
                // Agent null olabilir (müşteri teklif talebi)
                Agent? agent = null;
                if (createOfferDto.AgentId.HasValue)
                {
                    agent = await _context.Agents.FindAsync(createOfferDto.AgentId.Value);
                    if (agent == null)
                    {
                        Console.WriteLine($"Agent bulunamadı: {createOfferDto.AgentId.Value}");
                        return null;
                    }
                    Console.WriteLine($"Agent bulundu: {agent.Department}");
                }
                
                var offer = new Offer
                {
                    CustomerId = createOfferDto.CustomerId,
                    AgentId = createOfferDto.AgentId, // Agent yoksa null (varsayılan)
                    InsuranceTypeId = createOfferDto.InsuranceTypeId,
                    Department = createOfferDto.Department ?? insuranceType.Name, // DTO'dan veya InsuranceType'dan al
                    BasePrice = createOfferDto.BasePrice, // DTO'dan gelen değeri kullan
                    DiscountRate = createOfferDto.DiscountRate,
                    FinalPrice = createOfferDto.FinalPrice, // DTO'dan gelen değeri kullan
                    Status = createOfferDto.Status ?? "pending",
                    ValidUntil = createOfferDto.ValidUntil ?? CalculateValidityPeriod(insuranceType),
                    CreatedAt = DateTime.UtcNow,
                    // Ek alanlar
                    CoverageAmount = createOfferDto.CoverageAmount,
                    RequestedStartDate = createOfferDto.RequestedStartDate,
                    CustomerAdditionalInfo = createOfferDto.CustomerAdditionalInfo,
                    CreatedBy = createOfferDto.CreatedBy
                };
                
                Console.WriteLine($"Teklif oluşturuluyor: {offer.Department}");
                
                _context.Offers.Add(offer);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Teklif başarıyla oluşturuldu: ID={offer.OfferId}");
                
                // Oluşturulan teklifi müşteri bilgisiyle birlikte getir
                var createdOffer = await _context.Offers
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.Agent)
                        .ThenInclude(a => a.User)
                    .Include(o => o.InsuranceType)
                    .FirstOrDefaultAsync(o => o.OfferId == offer.OfferId);
                    
                return createdOffer != null ? MapToDto(createdOffer) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OfferService.CreateOfferAsync - Hata: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Geçerlilik süresi hesaplama
        private DateTime CalculateValidityPeriod(InsuranceType insuranceType)
        {
            Console.WriteLine($"🔍 CalculateValidityPeriod called for insurance type: '{insuranceType.Name}'");
            Console.WriteLine($"🔍 InsuranceType.ValidityPeriodDays: {insuranceType.ValidityPeriodDays}");
            
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
            
            var calculatedDate = DateTime.UtcNow.AddDays(validityDays);
            Console.WriteLine($"📅 Calculated validity period for '{insuranceType.Name}': {validityDays} days");
            Console.WriteLine($"📅 Current time (UTC): {DateTime.UtcNow}");
            Console.WriteLine($"📅 Calculated ValidUntil: {calculatedDate}");
            Console.WriteLine($"📅 Calculated ValidUntil (ISO): {calculatedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")}");
            
            return calculatedDate;
        }
        
        // Teklif güncelle
        public async Task<OfferDto?> UpdateOfferAsync(int id, UpdateOfferDto updateOfferDto)
        {
            var offer = await _context.Offers.FindAsync(id);
            
            if (offer == null)
            {
                return null;
            }
            
            // InsuranceType kontrolü
            if (updateOfferDto.InsuranceTypeId.HasValue)
            {
                var insuranceType = await _context.InsuranceTypes.FindAsync(updateOfferDto.InsuranceTypeId.Value);
                if (insuranceType == null)
                {
                    return null;
                }
                offer.InsuranceTypeId = updateOfferDto.InsuranceTypeId.Value;
                offer.BasePrice = insuranceType.BasePrice;
            }
            
            // BasePrice güncellemesi
            if (updateOfferDto.BasePrice.HasValue)
            {
                offer.BasePrice = updateOfferDto.BasePrice.Value;
                Console.WriteLine($"✅ BasePrice updated to: {offer.BasePrice}");
            }
            
            // CoverageAmount güncellemesi
            if (updateOfferDto.CoverageAmount.HasValue)
            {
                offer.CoverageAmount = updateOfferDto.CoverageAmount.Value;
                Console.WriteLine($"✅ CoverageAmount updated to: {offer.CoverageAmount}");
            }
            
            if (updateOfferDto.DiscountRate.HasValue)
            {
                offer.DiscountRate = updateOfferDto.DiscountRate.Value;
                Console.WriteLine($"✅ DiscountRate updated to: {offer.DiscountRate}%");
            }
            
            // FinalPrice güncellemesi - frontend'den gelen değeri kullan
            if (updateOfferDto.FinalPrice.HasValue)
            {
                offer.FinalPrice = updateOfferDto.FinalPrice.Value;
                Console.WriteLine($"✅ FinalPrice updated to: {offer.FinalPrice} (from frontend)");
            }
            
            if (!string.IsNullOrEmpty(updateOfferDto.Status))
            {
                offer.Status = updateOfferDto.Status;
            }
            
            offer.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            // Güncellenmiş teklifi müşteri bilgisiyle birlikte getir
            var updatedOffer = await _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .FirstOrDefaultAsync(o => o.OfferId == id);
                
            return updatedOffer != null ? MapToDto(updatedOffer) : null;
        }
        
        // Teklif sil
        public async Task<bool> DeleteOfferAsync(int id)
        {
            var offer = await _context.Offers.FindAsync(id);
            
            if (offer == null)
            {
                return false;
            }
            
            _context.Offers.Remove(offer);
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        // Müşteriye göre teklifleri getir
        public async Task<List<OfferDto>> GetOffersByCustomerAsync(int customerId)
        {
            Console.WriteLine($"🔍 OfferService.GetOffersByCustomerAsync - CustomerId: {customerId}");
            
            var offers = await _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .Where(o => o.CustomerId == customerId)
                .OrderBy(o => o.OfferId)
                .ToListAsync();
                
            Console.WriteLine($"🔍 OfferService: Found {offers.Count} offers in database for customer {customerId}");
            
            var result = offers.Select(MapToDto).ToList();
            Console.WriteLine($"🔍 OfferService: Mapped to {result.Count} DTOs");
            
            return result;
        }
        
        // Teklif durumuna göre getir
        public async Task<List<OfferDto>> GetOffersByStatusAsync(string status)
        {
            var offers = await _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .Where(o => o.Status == status)
                .OrderBy(o => o.OfferId)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // Teklif arama
        public async Task<List<OfferDto>> SearchOffersAsync(string? insuranceType, string? status, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(insuranceType))
            {
                query = query.Where(o => o.InsuranceType.Name.Contains(insuranceType));
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }
            
            if (minPrice.HasValue)
            {
                query = query.Where(o => o.FinalPrice >= minPrice.Value);
            }
            
            if (maxPrice.HasValue)
            {
                query = query.Where(o => o.FinalPrice <= maxPrice.Value);
            }
            
            var offers = await query.OrderBy(o => o.OfferId).ToListAsync();
            
            return offers.Select(MapToDto).ToList();
        }
        
        // Departman bazlı teklifleri getir (Agent için)
        public async Task<List<OfferDto>> GetOffersByDepartmentAsync(string department)
        {
            var offers = await _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .Where(o => o.Agent != null && o.Agent.Department == department)
                .OrderBy(o => o.OfferId)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // Agent'ın departmanına göre teklifleri getir
        public async Task<List<OfferDto>> GetOffersByAgentDepartmentAsync(int agentId)
        {
            try
            {
                Console.WriteLine($"OfferService.GetOffersByAgentDepartmentAsync - AgentId: {agentId}");
                
                var agent = await _context.Agents.FindAsync(agentId);
                if (agent == null)
                {
                    Console.WriteLine($"Agent bulunamadı: {agentId}");
                    return new List<OfferDto>();
                }
                
                Console.WriteLine($"Agent bulundu: {agent.User?.Name ?? "İsimsiz"}, Departman: {agent.Department}");
                
                var offers = await _context.Offers
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.Agent)
                        .ThenInclude(a => a.User)
                    .Include(o => o.InsuranceType)
                    .Where(o => o.Agent != null && o.Agent.Department == agent.Department)
                    .OrderBy(o => o.OfferId)
                    .ToListAsync();
                
                Console.WriteLine($"Departman '{agent.Department}' için {offers.Count} teklif bulundu");
                
                var result = offers.Select(MapToDto).ToList();
                Console.WriteLine($"DTO'ya dönüştürüldü: {result.Count} teklif");
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OfferService.GetOffersByAgentDepartmentAsync - Hata: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
        
        // Admin için tüm teklifleri getir (departman filtresi olmadan)
        public async Task<List<OfferDto>> GetAllOffersForAdminAsync()
        {
            var offers = await _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .OrderBy(o => o.OfferId)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // Agent'a göre teklifleri getir
        public async Task<List<OfferDto>> GetOffersByAgentAsync(int agentId)
        {
            var offers = await _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .Where(o => o.AgentId == agentId)
                .OrderBy(o => o.OfferId)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // Fiyat aralığına göre teklifleri getir
        public async Task<List<OfferDto>> GetOffersByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var offers = await _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .Where(o => o.FinalPrice >= minPrice && o.FinalPrice <= maxPrice)
                .OrderBy(o => o.OfferId)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // Müşteri ve duruma göre teklifleri getir
        public async Task<List<OfferDto>> GetOffersByCustomerAndStatusAsync(int customerId, string status)
        {
            var offers = await _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .Where(o => o.CustomerId == customerId && o.Status == status)
                .OrderBy(o => o.OfferId)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // Sigorta türüne göre teklifleri getir
        public async Task<List<OfferDto>> GetOffersByInsuranceTypeAsync(int insuranceTypeId)
        {
            var offers = await _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .Where(o => o.InsuranceTypeId == insuranceTypeId)
                .OrderBy(o => o.OfferId)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // Tarih aralığına göre teklifleri getir
        public async Task<List<OfferDto>> GetOffersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var offers = await _context.Offers
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Agent)
                    .ThenInclude(a => a.User)
                .Include(o => o.InsuranceType)
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .OrderBy(o => o.OfferId)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // Offer entity'sini OfferDto'ya dönüştür
        private static OfferDto MapToDto(Offer offer)
        {
            Console.WriteLine($"🔍 MapToDto - Offer {offer.OfferId}: ValidUntil = {offer.ValidUntil}");
            Console.WriteLine($"🔍 MapToDto - Offer {offer.OfferId}: ValidUntil type = {offer.ValidUntil.GetType()}");
            Console.WriteLine($"🔍 MapToDto - Offer {offer.OfferId}: ValidUntil == DateTime.MinValue = {offer.ValidUntil == DateTime.MinValue}");
            
            return new OfferDto
            {
                OfferId = offer.OfferId,
                CustomerId = offer.CustomerId,
                AgentId = offer.AgentId,
                InsuranceTypeId = offer.InsuranceTypeId,
                BasePrice = offer.BasePrice,
                DiscountRate = offer.DiscountRate,
                FinalPrice = offer.FinalPrice,
                Status = offer.Status,
                ValidUntil = offer.ValidUntil,
                CreatedAt = offer.CreatedAt,
                UpdatedAt = offer.UpdatedAt,
                
                // Flat properties for easier access
                CustomerName = offer.Customer?.User?.Name ?? string.Empty,
                CustomerEmail = offer.Customer?.User?.Email ?? string.Empty,
                CustomerPhone = offer.Customer?.Phone ?? string.Empty,
                CustomerAddress = offer.Customer?.Address ?? string.Empty,
                IdNo = offer.Customer?.IdNo ?? string.Empty,
                UserId = offer.Customer?.UserId ?? 0,
                AgentName = offer.Agent?.User?.Name ?? string.Empty,
                AgentEmail = offer.Agent?.User?.Email ?? string.Empty,
                AgentPhone = offer.Agent?.Phone ?? string.Empty,
                AgentAddress = offer.Agent?.Address ?? string.Empty,
                AgentCode = offer.Agent?.AgentCode ?? string.Empty,
                AgentUserId = offer.Agent?.UserId ?? 0,
                InsuranceTypeName = offer.InsuranceType?.Name ?? string.Empty,
                InsuranceTypeCategory = offer.InsuranceType?.Category ?? string.Empty,
                PolicyPdfUrl = offer.PolicyPdfUrl,
                
                // Navigation properties
                InsuranceType = offer.InsuranceType != null ? new InsuranceTypeDto
                {
                    Id = offer.InsuranceType.InsuranceTypeId,
                    InsuranceTypeId = offer.InsuranceType.InsuranceTypeId,
                    InsTypeId = offer.InsuranceType.InsuranceTypeId,
                    Name = offer.InsuranceType.Name,
                    Category = offer.InsuranceType.Category,
                    Description = offer.InsuranceType.Description,
                    BasePrice = offer.InsuranceType.BasePrice,
                    CoverageDetails = offer.InsuranceType.CoverageDetails,
                    IsActive = offer.InsuranceType.IsActive,
                    ValidityPeriodDays = offer.InsuranceType.ValidityPeriodDays,
                    CreatedAt = offer.InsuranceType.CreatedAt,
                    UpdatedAt = offer.InsuranceType.UpdatedAt,
                    UserId = offer.InsuranceType.UserId
                } : null,
                
                // Additional properties
                IsCustomerApproved = offer.IsCustomerApproved,
                CustomerApprovedAt = offer.CustomerApprovedAt,
                ReviewedAt = offer.ReviewedAt,
                ReviewedBy = offer.ReviewedBy,
                CreatedBy = offer.CreatedBy,
                CustomerAdditionalInfo = offer.CustomerAdditionalInfo,
                CoverageAmount = offer.CoverageAmount,
                RequestedStartDate = offer.RequestedStartDate,
                Department = offer.Department,
                AdminNotes = offer.AdminNotes,
                RejectionReason = offer.RejectionReason,
                
                SelectedCoverages = offer.SelectedCoverages?.Select(sc => new SelectedCoverageDto
                {
                    SelectedCoverageId = sc.SelectedCoverageId,
                    OfferId = sc.OfferId,
                    CoverageId = sc.CoverageId,
                    Premium = sc.Premium,
                    Notes = sc.Notes
                }).ToList() ?? new List<SelectedCoverageDto>()
            };
        }
        
        // İndirim oranına ve kapsam artış oranına göre final fiyat hesapla
        public decimal CalculateFinalPrice(decimal basePrice, decimal discountRate, decimal coverageIncreaseRate = 0)
        {
            if (discountRate < 0) discountRate = 0;
            if (discountRate > 100) discountRate = 100;
            if (coverageIncreaseRate < 0) coverageIncreaseRate = 0;
            if (coverageIncreaseRate > 100) coverageIncreaseRate = 100;
            
            // Kapsam artış oranını uygula
            decimal priceWithCoverage = basePrice * (1 + coverageIncreaseRate / 100);
            
            // İndirim oranını uygula
            decimal discountAmount = priceWithCoverage * (discountRate / 100);
            decimal finalPrice = priceWithCoverage - discountAmount;
            
            return Math.Max(0, finalPrice); // Negatif fiyat olmasın
        }
        
        // Teklif güncelleme sırasında final fiyatı otomatik hesapla
        public async Task<bool> UpdateOfferWithDiscountAsync(int offerId, decimal? discountRate, decimal? finalPrice = null)
        {
            try
            {
                var offer = await _context.Offers.FindAsync(offerId);
                if (offer == null) return false;
                
                if (discountRate.HasValue)
                {
                    offer.DiscountRate = discountRate.Value;
                    // Eğer final fiyat belirtilmemişse otomatik hesapla
                    if (!finalPrice.HasValue)
                    {
                        // Kapsam artış oranını coverageAmount'dan al (0, 25, 40)
                        decimal coverageIncreaseRate = offer.CoverageAmount ?? 0;
                        offer.FinalPrice = CalculateFinalPrice(offer.BasePrice, discountRate.Value, coverageIncreaseRate);
                    }
                    else
                    {
                        offer.FinalPrice = finalPrice.Value;
                    }
                }
                
                offer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateOfferWithDiscountAsync - Hata: {ex.Message}");
                return false;
            }
        }
    }
}






