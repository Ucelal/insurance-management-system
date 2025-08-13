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
                .OrderBy(o => o.Id)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // ID'ye göre teklif getir
        public async Task<OfferDto?> GetOfferByIdAsync(int id)
        {
            var offer = await _context.Offers
                .Include(o => o.Customer)
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
            
            var offer = new Offer
            {
                CustomerId = createOfferDto.CustomerId,
                InsuranceType = createOfferDto.InsuranceType,
                Price = createOfferDto.Price,
                Status = createOfferDto.Status
            };
            
            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();
            
            // Oluşturulan teklifi müşteri bilgisiyle birlikte getir
            var createdOffer = await _context.Offers
                .Include(o => o.Customer)
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
            
            offer.InsuranceType = updateOfferDto.InsuranceType;
            offer.Price = updateOfferDto.Price;
            offer.Status = updateOfferDto.Status;
            
            await _context.SaveChangesAsync();
            
            // Güncellenmiş teklifi müşteri bilgisiyle birlikte getir
            var updatedOffer = await _context.Offers
                .Include(o => o.Customer)
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
                .Where(o => o.Status == status)
                .OrderBy(o => o.Id)
                .ToListAsync();
                
            return offers.Select(MapToDto).ToList();
        }
        
        // Teklif arama
        public async Task<List<OfferDto>> SearchOffersAsync(string? insuranceType, string? status, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Offers.Include(o => o.Customer).AsQueryable();
            
            if (!string.IsNullOrEmpty(insuranceType))
            {
                query = query.Where(o => o.InsuranceType.Contains(insuranceType));
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }
            
            if (minPrice.HasValue)
            {
                query = query.Where(o => o.Price >= minPrice.Value);
            }
            
            if (maxPrice.HasValue)
            {
                query = query.Where(o => o.Price <= maxPrice.Value);
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
                InsuranceType = offer.InsuranceType,
                Price = offer.Price,
                Status = offer.Status,
                Customer = offer.Customer != null ? new CustomerDto
                {
                    Id = offer.Customer.Id,
                    UserId = offer.Customer.UserId,
                    Type = offer.Customer.Type,
                    IdNo = offer.Customer.IdNo,
                    Address = offer.Customer.Address,
                    Phone = offer.Customer.Phone
                } : null
            };
        }
    }
}
