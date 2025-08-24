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
            var offers = await _context.Offers
                .Include(o => o.Customer)
                .Include(o => o.Agent)
                .Include(o => o.InsuranceType)
                .OrderBy(o => o.Id)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // ID'ye göre teklif getir
        public async Task<OfferDto?> GetOfferByIdAsync(int id)
        {
            var offer = await _context.Offers
                .Include(o => o.Customer)
                .Include(o => o.Agent)
                .Include(o => o.InsuranceType)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            return offer != null ? MapToDto(offer) : null;
        }
        
        // Yeni teklif oluştur
        public async Task<OfferDto?> CreateOfferAsync(CreateOfferDto createOfferDto)
        {
            // Müşteri kontrolü
            var customer = await _context.Customers.FindAsync(createOfferDto.CustomerId);
            if (customer == null)
            {
                return null;
            }
            
            // Agent kontrolü
            var agent = await _context.Agents.FindAsync(createOfferDto.AgentId);
            if (agent == null)
            {
                return null;
            }
            
            // InsuranceType kontrolü
            var insuranceType = await _context.InsuranceTypes.FindAsync(createOfferDto.InsuranceTypeId);
            if (insuranceType == null)
            {
                return null;
            }
            
            var offer = new Offer
            {
                CustomerId = createOfferDto.CustomerId,
                AgentId = createOfferDto.AgentId,
                InsuranceTypeId = createOfferDto.InsuranceTypeId,
                Description = createOfferDto.Description ?? string.Empty,
                BasePrice = insuranceType.BasePrice,
                DiscountRate = createOfferDto.DiscountRate ?? 0,
                FinalPrice = insuranceType.BasePrice * (1 - (createOfferDto.DiscountRate ?? 0) / 100),
                Status = "pending",
                ValidUntil = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();
            
            // Oluşturulan teklifi müşteri bilgisiyle birlikte getir
            var createdOffer = await _context.Offers
                .Include(o => o.Customer)
                .Include(o => o.Agent)
                .Include(o => o.InsuranceType)
                .FirstOrDefaultAsync(o => o.Id == offer.Id);
                
            return createdOffer != null ? MapToDto(createdOffer) : null;
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
            
            if (!string.IsNullOrEmpty(updateOfferDto.Description))
            {
                offer.Description = updateOfferDto.Description;
            }
            
            if (updateOfferDto.DiscountRate.HasValue)
            {
                offer.DiscountRate = updateOfferDto.DiscountRate.Value;
                offer.FinalPrice = offer.BasePrice * (1 - offer.DiscountRate / 100);
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
                .Include(o => o.Agent)
                .Include(o => o.InsuranceType)
                .FirstOrDefaultAsync(o => o.Id == id);
                
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
            var offers = await _context.Offers
                .Include(o => o.Customer)
                .Include(o => o.Agent)
                .Include(o => o.InsuranceType)
                .Where(o => o.CustomerId == customerId)
                .OrderBy(o => o.Id)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // Teklif durumuna göre getir
        public async Task<List<OfferDto>> GetOffersByStatusAsync(string status)
        {
            var offers = await _context.Offers
                .Include(o => o.Customer)
                .Include(o => o.Agent)
                .Include(o => o.InsuranceType)
                .Where(o => o.Status == status)
                .OrderBy(o => o.Id)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // Teklif arama
        public async Task<List<OfferDto>> SearchOffersAsync(string? insuranceType, string? status, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Offers
                .Include(o => o.Customer)
                .Include(o => o.Agent)
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
            
            var offers = await query.OrderBy(o => o.Id).ToListAsync();
            
            return offers.Select(MapToDto).ToList();
        }
        
        // Offer entity'sini OfferDto'ya dönüştür
        private static OfferDto MapToDto(Offer offer)
        {
            return new OfferDto
            {
                Id = offer.Id,
                CustomerId = offer.CustomerId,
                AgentId = offer.AgentId,
                InsuranceTypeId = offer.InsuranceTypeId,
                Description = offer.Description,
                BasePrice = offer.BasePrice,
                DiscountRate = offer.DiscountRate,
                FinalPrice = offer.FinalPrice,
                Status = offer.Status,
                ValidUntil = offer.ValidUntil,
                CreatedAt = offer.CreatedAt,
                UpdatedAt = offer.UpdatedAt,
                
                // Navigation properties
                Customer = offer.Customer != null ? new CustomerDto
                {
                    Id = offer.Customer.Id,
                    UserId = offer.Customer.UserId,
                    Type = offer.Customer.Type,
                    IdNo = offer.Customer.IdNo,
                    Address = offer.Customer.Address,
                    Phone = offer.Customer.Phone
                } : null,
                
                Agent = offer.Agent != null ? new AgentDto
                {
                    Id = offer.Agent.Id,
                    UserId = offer.Agent.UserId,
                    AgentCode = offer.Agent.AgentCode,
                    Department = offer.Agent.Department,
                    Address = offer.Agent.Address,
                    Phone = offer.Agent.Phone
                } : null,
                
                InsuranceType = offer.InsuranceType != null ? new InsuranceTypeDto
                {
                    Id = offer.InsuranceType.Id,
                    Name = offer.InsuranceType.Name,
                    Category = offer.InsuranceType.Category,
                    Description = offer.InsuranceType.Description,
                    IsActive = offer.InsuranceType.IsActive,
                    BasePrice = offer.InsuranceType.BasePrice,
                    CoverageDetails = offer.InsuranceType.CoverageDetails,
                    CreatedAt = offer.InsuranceType.CreatedAt,
                    UpdatedAt = offer.InsuranceType.UpdatedAt
                } : null,
                
                SelectedCoverages = offer.SelectedCoverages?.Select(sc => new SelectedCoverageDto
                {
                    Id = sc.Id,
                    OfferId = sc.OfferId,
                    CoverageId = sc.CoverageId,
                    SelectedLimit = sc.SelectedLimit,
                    Premium = sc.Premium,
                    IsSelected = sc.IsSelected,
                    CreatedAt = sc.CreatedAt
                }).ToList() ?? new List<SelectedCoverageDto>()
            };
        }
    }
}
