using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;
using InsuranceAPI.Services;

namespace InsuranceAPI.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly InsuranceDbContext _context;
        
        // Policy service constructor - dependency injection
        public PolicyService(InsuranceDbContext context)
        {
            _context = context;
        }
        
        // Tüm poliçeleri getir
        public async Task<List<PolicyDto>> GetAllPoliciesAsync()
        {
            var policies = await _context.Policies
                .Include(p => p.Offer)
                    .ThenInclude(o => o.Agent)
                        .ThenInclude(a => a.User)
                .Include(p => p.Offer)
                    .ThenInclude(o => o.ReviewedByAgent)
                        .ThenInclude(a => a.User)
                .Include(p => p.InsuranceType)
                .Include(p => p.Agent)
                    .ThenInclude(a => a.User)
                .OrderBy(p => p.PolicyId)
                .ToListAsync();
                
            return policies.Select(MapToDto).ToList();
        }
        
        // ID'ye göre poliçe getir
        public async Task<PolicyDto?> GetPolicyByIdAsync(int id)
        {
            var policy = await _context.Policies
                .Include(p => p.Offer)
                    .ThenInclude(o => o.Agent)
                        .ThenInclude(a => a.User)
                .Include(p => p.Offer)
                    .ThenInclude(o => o.ReviewedByAgent)
                        .ThenInclude(a => a.User)
                .Include(p => p.InsuranceType)
                .Include(p => p.Agent)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.PolicyId == id);
                
            return policy != null ? MapToDto(policy) : null;
        }
        
        // Yeni poliçe oluştur
        public async Task<PolicyDto?> CreatePolicyAsync(CreatePolicyDto createPolicyDto)
        {
            // Teklif kontrolü
            var offer = await _context.Offers.FindAsync(createPolicyDto.OfferId);
            if (offer == null)
            {
                return null;
            }
            
            // Poliçe numarası kontrolü
            var existingPolicy = await _context.Policies.FirstOrDefaultAsync(p => p.PolicyNumber == createPolicyDto.PolicyNumber);
            if (existingPolicy != null)
            {
                return null;
            }
            
            var policy = new Policy
            {
                OfferId = createPolicyDto.OfferId,
                StartDate = createPolicyDto.StartDate,
                EndDate = createPolicyDto.EndDate,
                PolicyNumber = createPolicyDto.PolicyNumber,
                InsuranceTypeId = offer.InsuranceTypeId, // Sigorta türü ID'sini ekle
                AgentId = offer.ReviewedBy // Teklifi onaylayan agent'ın ID'si
            };
            
            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();
            
            // Oluşturulan poliçeyi teklif bilgisiyle birlikte getir
            var createdPolicy = await _context.Policies
                .Include(p => p.Offer)
                    .ThenInclude(o => o.Agent)
                        .ThenInclude(a => a.User)
                .Include(p => p.Offer)
                    .ThenInclude(o => o.ReviewedByAgent)
                        .ThenInclude(a => a.User)
                .Include(p => p.InsuranceType)
                .Include(p => p.Agent)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.PolicyId == policy.PolicyId);
                
            if (createdPolicy != null)
            {
                var policyDto = MapToDto(createdPolicy);
                
                // Policy PDF oluştur
                try
                {
                    var pdfService = new PdfService(_context);
                    var pdfBytes = await pdfService.CreatePolicyPdfAsync(policyDto);
                    var fileName = $"Poliçe_{policyDto.PolicyNumber}_{policyDto.CreatedAt:yyyyMMdd}.pdf";
                    var pdfUrl = await pdfService.SavePdfAsync(
                        pdfBytes, 
                        fileName, 
                        "Policy", 
                        $"Poliçe dokümanı - Poliçe No: {policyDto.PolicyNumber}",
                        createdPolicy.Offer?.CustomerId,
                        createdPolicy.Offer?.Customer?.UserId
                    );
                    
                    Console.WriteLine($"✅ Policy PDF created: {pdfUrl}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error creating policy PDF: {ex.Message}");
                    // PDF oluşturma hatası policy oluşturmayı engellemez
                }
                
                return policyDto;
            }
            
            return null;
        }
        
        // Poliçe güncelle
        public async Task<PolicyDto?> UpdatePolicyAsync(int id, UpdatePolicyDto updatePolicyDto)
        {
            var policy = await _context.Policies.FindAsync(id);
            
            if (policy == null)
            {
                return null;
            }
            
            // Poliçe numarası değişmişse kontrol et
            if (policy.PolicyNumber != updatePolicyDto.PolicyNumber)
            {
                var existingPolicy = await _context.Policies.FirstOrDefaultAsync(p => p.PolicyNumber == updatePolicyDto.PolicyNumber);
                if (existingPolicy != null)
                {
                    return null;
                }
            }
            
            if (updatePolicyDto.StartDate.HasValue)
            {
                policy.StartDate = updatePolicyDto.StartDate.Value;
            }
            
            if (updatePolicyDto.EndDate.HasValue)
            {
                policy.EndDate = updatePolicyDto.EndDate.Value;
            }
            
            if (!string.IsNullOrEmpty(updatePolicyDto.PolicyNumber))
            {
                policy.PolicyNumber = updatePolicyDto.PolicyNumber;
            }
            
            if (updatePolicyDto.TotalPremium.HasValue)
            {
                policy.TotalPremium = updatePolicyDto.TotalPremium.Value;
            }
            
            if (!string.IsNullOrEmpty(updatePolicyDto.Status))
            {
                policy.Status = updatePolicyDto.Status;
            }
            

            
            if (!string.IsNullOrEmpty(updatePolicyDto.Notes))
            {
                policy.Notes = updatePolicyDto.Notes;
            }
            
            policy.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            // Güncellenmiş poliçeyi teklif bilgisiyle birlikte getir
            var updatedPolicy = await _context.Policies
                .Include(p => p.Offer)
                    .ThenInclude(o => o.Agent)
                        .ThenInclude(a => a.User)
                .Include(p => p.Offer)
                    .ThenInclude(o => o.ReviewedByAgent)
                        .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.PolicyId == id);
                
            return updatedPolicy != null ? MapToDto(updatedPolicy) : null;
        }
        
        // Poliçe sil
        public async Task<bool> DeletePolicyAsync(int id)
        {
            var policy = await _context.Policies.FindAsync(id);
            
            if (policy == null)
            {
                return false;
            }
            
            _context.Policies.Remove(policy);
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        // Teklif ID'sine göre poliçe getir
        public async Task<PolicyDto?> GetPolicyByOfferAsync(int offerId)
        {
            var policy = await _context.Policies
                .Include(p => p.Offer)
                    .ThenInclude(o => o.Agent)
                        .ThenInclude(a => a.User)
                .Include(p => p.Offer)
                    .ThenInclude(o => o.ReviewedByAgent)
                        .ThenInclude(a => a.User)
                .Include(p => p.InsuranceType)
                .Include(p => p.Agent)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.OfferId == offerId);
                
            return policy != null ? MapToDto(policy) : null;
        }
        
        // Poliçe numarasına göre getir
        public async Task<PolicyDto?> GetPolicyByNumberAsync(string policyNumber)
        {
            var policy = await _context.Policies
                .Include(p => p.Offer)
                    .ThenInclude(o => o.Agent)
                        .ThenInclude(a => a.User)
                .Include(p => p.Offer)
                    .ThenInclude(o => o.ReviewedByAgent)
                        .ThenInclude(a => a.User)
                .Include(p => p.InsuranceType)
                .Include(p => p.Agent)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
                
            return policy != null ? MapToDto(policy) : null;
        }
        
        // Poliçe arama
        public async Task<List<PolicyDto>> SearchPoliciesAsync(string? policyNumber, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Policies
                .Include(p => p.Offer)
                    .ThenInclude(o => o.Agent)
                        .ThenInclude(a => a.User)
                .Include(p => p.Offer)
                    .ThenInclude(o => o.ReviewedByAgent)
                        .ThenInclude(a => a.User)
                .Include(p => p.InsuranceType)
                .Include(p => p.Agent)
                    .ThenInclude(a => a.User)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(policyNumber))
            {
                query = query.Where(p => p.PolicyNumber.Contains(policyNumber));
            }
            
            if (startDate.HasValue)
            {
                query = query.Where(p => p.StartDate >= startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                query = query.Where(p => p.EndDate <= endDate.Value);
            }
            
            var policies = await query.OrderBy(p => p.PolicyId).ToListAsync();
            
            return policies.Select(MapToDto).ToList();
        }
        
        // Ödeme sonrası poliçe oluştur
        public async Task<PolicyDto?> CreatePolicyFromPaymentAsync(int offerId, decimal paymentAmount, int userId)
        {
            try
            {
                // Teklifi detaylarıyla birlikte getir
                var offer = await _context.Offers
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.InsuranceType)
                    .Include(o => o.Agent)
                        .ThenInclude(a => a.User)
                    .FirstOrDefaultAsync(o => o.OfferId == offerId);

                if (offer == null)
                {
                    Console.WriteLine($"❌ Offer not found: {offerId}");
                    return null;
                }

                // Bu teklif için zaten poliçe var mı kontrol et
                var existingPolicy = await _context.Policies.FirstOrDefaultAsync(p => p.OfferId == offerId);
                if (existingPolicy != null)
                {
                    Console.WriteLine($"⚠️ Policy already exists for offer: {offerId}");
                    return MapToDto(existingPolicy);
                }

                // Poliçe numarası oluştur
                var policyNumber = GeneratePolicyNumber(offer);

                // Poliçe tarihlerini belirle
                var startDate = offer.RequestedStartDate ?? DateTime.UtcNow.Date;
                var endDate = CalculateEndDate(startDate, offer.InsuranceType?.Name ?? "Genel");

                // Agent ID'sini belirle - önce offer'dan, yoksa offer'ın agent'ından
                var agentId = offer.ReviewedBy ?? offer.AgentId;
                
                Console.WriteLine($"🔍 PolicyService: Offer {offerId} - ReviewedBy: {offer.ReviewedBy}, AgentId: {offer.AgentId}, Final AgentId: {agentId}");

                // Poliçe oluştur
                var policy = new Policy
                {
                    OfferId = offerId,
                    PolicyNumber = policyNumber,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPremium = paymentAmount,
                    Status = "Active",
                    Notes = $"Ödeme ile oluşturulan poliçe - Teklif #{offerId}",
                    UserId = userId,
                    InsuranceTypeId = offer.InsuranceTypeId, // Sigorta türü ID'sini ekle
                    AgentId = agentId, // Agent ID'sini doğru şekilde ata
                    CreatedAt = DateTime.UtcNow
                };

                _context.Policies.Add(policy);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Policy created successfully: {policyNumber} for offer: {offerId}");

                // Oluşturulan poliçeyi detaylarıyla birlikte getir
                var createdPolicy = await _context.Policies
                    .Include(p => p.Offer)
                        .ThenInclude(o => o.Customer)
                            .ThenInclude(c => c.User)
                    .Include(p => p.Offer)
                        .ThenInclude(o => o.InsuranceType)
                    .Include(p => p.Offer)
                        .ThenInclude(o => o.Agent)
                            .ThenInclude(a => a.User)
                    .Include(p => p.Offer)
                        .ThenInclude(o => o.Agent)
                            .ThenInclude(a => a.User)
                    .FirstOrDefaultAsync(p => p.PolicyId == policy.PolicyId);

                return createdPolicy != null ? MapToDto(createdPolicy) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating policy from payment: {ex.Message}");
                return null;
            }
        }

        // Poliçe numarası oluştur
        private string GeneratePolicyNumber(Offer offer)
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month.ToString("00");
            var day = DateTime.UtcNow.Day.ToString("00");
            var offerId = offer.OfferId.ToString("0000");
            var insuranceTypeCode = GetInsuranceTypeCode(offer.InsuranceType?.Name ?? "GEN");
            
            return $"POL-{year}{month}{day}-{insuranceTypeCode}-{offerId}";
        }

        // Sigorta türü kodunu al
        private string GetInsuranceTypeCode(string insuranceTypeName)
        {
            return insuranceTypeName.ToUpper() switch
            {
                "ARAÇ SİGORTASI" or "ARAÇ" => "ARC",
                "SEYAHAT SİGORTASI" or "SEYAHAT" => "SYH",
                "KONUT SİGORTASI" or "KONUT" => "KNT",
                "SAĞLIK SİGORTASI" or "SAĞLIK" => "SGL",
                "HAYAT SİGORTASI" or "HAYAT" => "HYT",
                _ => "GEN"
            };
        }

        // Poliçe bitiş tarihini hesapla
        private DateTime CalculateEndDate(DateTime startDate, string insuranceTypeName)
        {
            return insuranceTypeName.ToUpper() switch
            {
                "ARAÇ SİGORTASI" => startDate.AddYears(1),
                "SEYAHAT SİGORTASI" => startDate.AddDays(30), // Seyahat için 30 gün
                "KONUT SİGORTASI" => startDate.AddYears(1),
                "SAĞLIK SİGORTASI" => startDate.AddYears(1),
                "HAYAT SİGORTASI" => startDate.AddYears(10), // Hayat sigortası için 10 yıl
                _ => startDate.AddYears(1) // Varsayılan 1 yıl
            };
        }

        // Müşterinin poliçelerini getir
        public async Task<List<PolicyDto>> GetPoliciesByCustomerAsync(int userId)
        {
            try
            {
                var policies = await _context.Policies
                    .Include(p => p.Offer)
                        .ThenInclude(o => o.Customer)
                            .ThenInclude(c => c.User)
                    .Include(p => p.InsuranceType)
                    .Include(p => p.Agent)
                        .ThenInclude(a => a.User)
                    .Include(p => p.Offer)
                        .ThenInclude(o => o.Agent)
                            .ThenInclude(a => a.User)
                    .Include(p => p.Offer)
                        .ThenInclude(o => o.Agent)
                            .ThenInclude(a => a.User)
                    .Where(p => p.Offer != null && p.Offer.Customer != null && p.Offer.Customer.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                Console.WriteLine($"✅ Found {policies.Count} policies for customer userId: {userId}");

                return policies.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting policies for customer: {ex.Message}");
                return new List<PolicyDto>();
            }
        }

        // Policy entity'sini PolicyDto'ya dönüştür
        private static PolicyDto MapToDto(Policy policy)
        {
            return new PolicyDto
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
                
                // Teklif formunu onaylayan yetkili bilgileri (doğrudan policy.Agent'tan)
                ApprovedByAgentName = policy.Agent?.User?.Name,
                ApprovedByAgentPhone = policy.Agent?.Phone,
                ApprovedByAgentEmail = policy.Agent?.User?.Email,
                
                Offer = policy.Offer != null ? new OfferDto
                {
                    OfferId = policy.Offer.OfferId,
                    CustomerId = policy.Offer.CustomerId,
                    AgentId = policy.Offer.AgentId ?? 0, // Null ise 0 olarak dönüştür
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
        }
    }
}
