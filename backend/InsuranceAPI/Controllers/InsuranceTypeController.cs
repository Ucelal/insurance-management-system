using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.DTOs;
using InsuranceAPI.Data;
using InsuranceAPI.Models;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // JWT authentication aktif
    public class InsuranceTypeController : ControllerBase
    {
        private readonly InsuranceDbContext _context;
        
        public InsuranceTypeController(InsuranceDbContext context)
        {
            _context = context;
        }
        
        // Tüm sigorta türlerini getir
        [HttpGet]
        public async Task<ActionResult<List<InsuranceTypeDto>>> GetAllInsuranceTypes()
        {
            var insuranceTypes = await _context.InsuranceTypes
                .Include(it => it.Coverages)
                .Where(it => it.IsActive)
                .OrderBy(it => it.Category)
                .ThenBy(it => it.Name)
                .ToListAsync();
                
            var dtos = insuranceTypes.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        
        // ID'ye göre sigorta türü getir
        [HttpGet("{id}")]
        public async Task<ActionResult<InsuranceTypeDto>> GetInsuranceTypeById(int id)
        {
            var insuranceType = await _context.InsuranceTypes
                .Include(it => it.Coverages)
                .FirstOrDefaultAsync(it => it.UserId == id && it.IsActive);
                
            if (insuranceType == null)
            {
                return NotFound(new { message = "Sigorta türü bulunamadı" });
            }
            
            return Ok(MapToDto(insuranceType));
        }
        
        // Kategoriye göre sigorta türlerini getir
        [HttpGet("category/{category}")]
        public async Task<ActionResult<List<InsuranceTypeDto>>> GetInsuranceTypesByCategory(string category)
        {
            var insuranceTypes = await _context.InsuranceTypes
                .Include(it => it.Coverages)
                .Where(it => it.Category.ToLower() == category.ToLower() && it.IsActive)
                .OrderBy(it => it.Name)
                .ToListAsync();
                
            var dtos = insuranceTypes.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        
        // Yeni sigorta türü oluştur (sadece admin)
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<InsuranceTypeDto>> CreateInsuranceType([FromBody] CreateInsuranceTypeDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var insuranceType = new InsuranceType
            {
                Name = createDto.Name,
                Category = createDto.Category,
                Description = createDto.Description ?? string.Empty,
                BasePrice = createDto.BasePrice,
                CoverageDetails = createDto.CoverageDetails ?? string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.InsuranceTypes.Add(insuranceType);
            await _context.SaveChangesAsync();
            
            var createdType = await _context.InsuranceTypes
                .Include(it => it.Coverages)
                .FirstOrDefaultAsync(it => it.UserId == insuranceType.UserId);
                
            return CreatedAtAction(nameof(GetInsuranceTypeById), new { id = insuranceType.UserId }, MapToDto(createdType!));
        }
        
        // Sigorta türü güncelle (sadece admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<InsuranceTypeDto>> UpdateInsuranceType(int id, [FromBody] UpdateInsuranceTypeDto updateDto)
        {
            var insuranceType = await _context.InsuranceTypes.FindAsync(id);
            
            if (insuranceType == null)
            {
                return NotFound(new { message = "Sigorta türü bulunamadı" });
            }
            
            if (!string.IsNullOrEmpty(updateDto.Name))
                insuranceType.Name = updateDto.Name;
                
            if (!string.IsNullOrEmpty(updateDto.Category))
                insuranceType.Category = updateDto.Category;
                
            if (!string.IsNullOrEmpty(updateDto.Description))
                insuranceType.Description = updateDto.Description;
                
            if (updateDto.BasePrice.HasValue)
                insuranceType.BasePrice = updateDto.BasePrice.Value;
                
            if (!string.IsNullOrEmpty(updateDto.CoverageDetails))
                insuranceType.CoverageDetails = updateDto.CoverageDetails;
                
            if (updateDto.IsActive.HasValue)
                insuranceType.IsActive = updateDto.IsActive.Value;
                
            insuranceType.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            var updatedType = await _context.InsuranceTypes
                .Include(it => it.Coverages)
                .FirstOrDefaultAsync(it => it.InsuranceTypeId == id);
                
            if (updatedType == null)
            {
                return NotFound(new { message = "Sigorta türü bulunamadı" });
            }
                
            return Ok(MapToDto(updatedType));
        }
        
        // Sigorta türü sil (sadece admin) - soft delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteInsuranceType(int id)
        {
            var insuranceType = await _context.InsuranceTypes.FindAsync(id);
            
            if (insuranceType == null)
            {
                return NotFound(new { message = "Sigorta türü bulunamadı" });
            }
            
            // Soft delete - sadece IsActive'ı false yap
            insuranceType.IsActive = false;
            insuranceType.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        
        // Sigorta türü arama
        [HttpGet("search")]
        public async Task<ActionResult<List<InsuranceTypeDto>>> SearchInsuranceTypes([FromQuery] string? name, [FromQuery] string? category)
        {
            var query = _context.InsuranceTypes.Include(it => it.Coverages).Where(it => it.IsActive).AsQueryable();
            
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(it => it.Name.Contains(name));
            }
            
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(it => it.Category.ToLower() == category.ToLower());
            }
            
            var insuranceTypes = await query.OrderBy(it => it.Category).ThenBy(it => it.Name).ToListAsync();
            var dtos = insuranceTypes.Select(MapToDto).ToList();
            
            return Ok(dtos);
        }
        
        // Kategorileri getir
        [HttpGet("categories")]
        public async Task<ActionResult<List<string>>> GetCategories()
        {
            var categories = await _context.InsuranceTypes
                .Where(it => it.IsActive)
                .Select(it => it.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
                
            return Ok(categories);
        }
        
        // InsuranceType entity'sini InsuranceTypeDto'ya dönüştür
        private static InsuranceTypeDto MapToDto(InsuranceType insuranceType)
        {
            return new InsuranceTypeDto
            {
                Id = insuranceType.InsuranceTypeId,
                Name = insuranceType.Name ?? string.Empty,
                Category = insuranceType.Category ?? string.Empty,
                Description = insuranceType.Description ?? string.Empty,
                IsActive = insuranceType.IsActive,
                BasePrice = insuranceType.BasePrice,
                CoverageDetails = insuranceType.CoverageDetails ?? string.Empty,
                CreatedAt = insuranceType.CreatedAt,
                UpdatedAt = insuranceType.UpdatedAt,
                Coverages = insuranceType.Coverages?.Select(c => new CoverageDto
                {
                    Id = c.CoverageId,
                    Name = c.Name ?? string.Empty,
                    Description = c.Description ?? string.Empty,
                    Limit = c.Limit,
                    Premium = c.Premium,
                    IsOptional = c.IsOptional,
                    IsActive = c.IsActive,
                    InsuranceTypeId = c.InsuranceTypeId,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList() ?? new List<CoverageDto>()
            };
        }
    }
}
