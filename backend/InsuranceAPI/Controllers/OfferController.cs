using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;
using InsuranceAPI.Data;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // JWT authentication aktif
    public class OfferController : ControllerBase
    {
        private readonly IOfferService _offerService;
        private readonly InsuranceDbContext _context;
        
        // Offer service dependency injection
        public OfferController(IOfferService offerService, InsuranceDbContext context)
        {
            _offerService = offerService;
            _context = context;
        }
        
        // Tüm teklifleri getir
        [HttpGet]
        public async Task<ActionResult<List<OfferDto>>> GetAllOffers()
        {
            var offers = await _offerService.GetAllOffersAsync();
            return Ok(offers);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _offerService.CreateOfferAsync(createOfferDto);
            
            if (result == null)
            {
                return BadRequest(new { message = "Teklif oluşturulamadı. Müşteri bulunamadı." });
            }
            
            return CreatedAtAction(nameof(GetOfferById), new { id = result.Id }, result);
        }
        
        // Teklif güncelle
        [HttpPut("{id}")]
        public async Task<ActionResult<OfferDto>> UpdateOffer(int id, [FromBody] UpdateOfferDto updateOfferDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _offerService.UpdateOfferAsync(id, updateOfferDto);
            
            if (result == null)
            {
                return NotFound(new { message = "Teklif bulunamadı" });
            }
            
            return Ok(result);
        }
        
        // Teklif sil
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOffer(int id)
        {
            var success = await _offerService.DeleteOfferAsync(id);
            
            if (!success)
            {
                return NotFound(new { message = "Teklif bulunamadı" });
            }
            
            return NoContent();
        }
        
        // Müşteriye göre teklifleri getir
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<OfferDto>>> GetOffersByCustomer(int customerId)
        {
            var offers = await _offerService.GetOffersByCustomerAsync(customerId);
            return Ok(offers);
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
                    Id = it.Id,
                    Name = it.Name,
                    Category = it.Category,
                    Description = it.Description,
                    IsActive = it.IsActive,
                    BasePrice = it.BasePrice,
                    CoverageDetails = it.CoverageDetails,
                    CreatedAt = it.CreatedAt,
                    UpdatedAt = it.UpdatedAt
                })
                .ToListAsync();
                
            return Ok(insuranceTypes);
        }
        
        // Teklif durumlarını getir
        [HttpGet("statuses")]
        public ActionResult GetOfferStatuses()
        {
            var statuses = new[]
            {
                new { Value = "pending", Label = "Beklemede" },
                new { Value = "approved", Label = "Onaylandı" },
                new { Value = "cancelled", Label = "İptal Edildi" }
            };
            
            return Ok(statuses);
        }
    }
}
