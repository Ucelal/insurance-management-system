using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;

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
                .OrderBy(p => p.Id)
                .ToListAsync();
                
            return policies.Select(MapToDto).ToList();
        }
        
        // ID'ye göre poliçe getir
        public async Task<PolicyDto?> GetPolicyByIdAsync(int id)
        {
            var policy = await _context.Policies
                .Include(p => p.Offer)
                .FirstOrDefaultAsync(p => p.Id == id);
                
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
                PolicyNumber = createPolicyDto.PolicyNumber
            };
            
            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();
            
            // Oluşturulan poliçeyi teklif bilgisiyle birlikte getir
            var createdPolicy = await _context.Policies
                .Include(p => p.Offer)
                .FirstOrDefaultAsync(p => p.Id == policy.Id);
                
            return createdPolicy != null ? MapToDto(createdPolicy) : null;
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
            
            policy.StartDate = updatePolicyDto.StartDate;
            policy.EndDate = updatePolicyDto.EndDate;
            policy.PolicyNumber = updatePolicyDto.PolicyNumber;
            
            await _context.SaveChangesAsync();
            
            // Güncellenmiş poliçeyi teklif bilgisiyle birlikte getir
            var updatedPolicy = await _context.Policies
                .Include(p => p.Offer)
                .FirstOrDefaultAsync(p => p.Id == id);
                
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
                .FirstOrDefaultAsync(p => p.OfferId == offerId);
                
            return policy != null ? MapToDto(policy) : null;
        }
        
        // Poliçe numarasına göre getir
        public async Task<PolicyDto?> GetPolicyByNumberAsync(string policyNumber)
        {
            var policy = await _context.Policies
                .Include(p => p.Offer)
                .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
                
            return policy != null ? MapToDto(policy) : null;
        }
        
        // Poliçe arama
        public async Task<List<PolicyDto>> SearchPoliciesAsync(string? policyNumber, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Policies.Include(p => p.Offer).AsQueryable();
            
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
            
            var policies = await query.OrderBy(p => p.Id).ToListAsync();
            
            return policies.Select(MapToDto).ToList();
        }
        
        // Policy entity'sini PolicyDto'ya dönüştür
        private static PolicyDto MapToDto(Policy policy)
        {
            return new PolicyDto
            {
                Id = policy.Id,
                OfferId = policy.OfferId,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                PolicyNumber = policy.PolicyNumber,
                Offer = policy.Offer != null ? new OfferDto
                {
                    Id = policy.Offer.Id,
                    CustomerId = policy.Offer.CustomerId,
                    InsuranceType = policy.Offer.InsuranceType,
                    Price = policy.Offer.Price,
                    Status = policy.Offer.Status
                } : null
            };
        }
    }
}
