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
    public class CoverageController : ControllerBase
    {
        private readonly InsuranceDbContext _context;
        
        public CoverageController(InsuranceDbContext context)
        {
            _context = context;
        }
        
        // Tüm teminatları getir
        [HttpGet]
        public async Task<ActionResult<List<CoverageDto>>> GetAllCoverages()
        {
            var coverages = await _context.Coverages
                .Include(c => c.InsuranceType)
                .Where(c => c.IsActive)
                .OrderBy(c => c.InsuranceType.Category)
                .ThenBy(c => c.Name)
                .ToListAsync();
                
            var dtos = coverages.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        
        // ID'ye göre teminat getir
        [HttpGet("{id}")]
        public async Task<ActionResult<CoverageDto>> GetCoverageById(int id)
        {
            var coverage = await _context.Coverages
                .Include(c => c.InsuranceType)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
                
            if (coverage == null)
            {
                return NotFound(new { message = "Teminat bulunamadı" });
            }
            
            return Ok(MapToDto(coverage));
        }
        
        // Sigorta türüne göre teminatları getir
        [HttpGet("insurance-type/{insuranceTypeId}")]
        public async Task<ActionResult<List<CoverageDto>>> GetCoveragesByInsuranceType(int insuranceTypeId)
        {
            var coverages = await _context.Coverages
                .Include(c => c.InsuranceType)
                .Where(c => c.InsuranceTypeId == insuranceTypeId && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
                
            var dtos = coverages.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        
        // Yeni teminat oluştur (sadece admin)
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<CoverageDto>> CreateCoverage([FromBody] CreateCoverageDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Sigorta türü kontrolü
            var insuranceType = await _context.InsuranceTypes.FindAsync(createDto.InsuranceTypeId);
            if (insuranceType == null)
            {
                return BadRequest(new { message = "Geçersiz sigorta türü" });
            }
            
            var coverage = new Coverage
            {
                Name = createDto.Name,
                Description = createDto.Description ?? string.Empty,
                Limit = createDto.Limit,
                Premium = createDto.Premium,
                IsOptional = createDto.IsOptional,
                IsActive = true,
                InsuranceTypeId = createDto.InsuranceTypeId,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Coverages.Add(coverage);
            await _context.SaveChangesAsync();
            
            var createdCoverage = await _context.Coverages
                .Include(c => c.InsuranceType)
                .FirstOrDefaultAsync(c => c.Id == coverage.Id);
                
            return CreatedAtAction(nameof(GetCoverageById), new { id = coverage.Id }, MapToDto(createdCoverage!));
        }
        
        // Teminat güncelle (sadece admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<CoverageDto>> UpdateCoverage(int id, [FromBody] UpdateCoverageDto updateDto)
        {
            var coverage = await _context.Coverages.FindAsync(id);
            
            if (coverage == null)
            {
                return NotFound(new { message = "Teminat bulunamadı" });
            }
            
            if (!string.IsNullOrEmpty(updateDto.Name))
                coverage.Name = updateDto.Name;
                
            if (!string.IsNullOrEmpty(updateDto.Description))
                coverage.Description = updateDto.Description;
                
            if (updateDto.Limit.HasValue)
                coverage.Limit = updateDto.Limit.Value;
                
            if (updateDto.Premium.HasValue)
                coverage.Premium = updateDto.Premium.Value;
                
            if (updateDto.IsOptional.HasValue)
                coverage.IsOptional = updateDto.IsOptional.Value;
                
            if (updateDto.IsActive.HasValue)
                coverage.IsActive = updateDto.IsActive.Value;
                
            if (updateDto.InsuranceTypeId.HasValue)
            {
                // Sigorta türü kontrolü
                var insuranceType = await _context.InsuranceTypes.FindAsync(updateDto.InsuranceTypeId.Value);
                if (insuranceType == null)
                {
                    return BadRequest(new { message = "Geçersiz sigorta türü" });
                }
                coverage.InsuranceTypeId = updateDto.InsuranceTypeId.Value;
            }
                
            coverage.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            var updatedCoverage = await _context.Coverages
                .Include(c => c.InsuranceType)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            return Ok(MapToDto(updatedCoverage!));
        }
        
        // Teminat sil (sadece admin) - soft delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteCoverage(int id)
        {
            var coverage = await _context.Coverages.FindAsync(id);
            
            if (coverage == null)
            {
                return NotFound(new { message = "Teminat bulunamadı" });
            }
            
            // Soft delete - sadece IsActive'ı false yap
            coverage.IsActive = false;
            coverage.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        
        // Teminat arama
        [HttpGet("search")]
        public async Task<ActionResult<List<CoverageDto>>> SearchCoverages([FromQuery] string? name, [FromQuery] int? insuranceTypeId)
        {
            var query = _context.Coverages.Include(c => c.InsuranceType).Where(c => c.IsActive).AsQueryable();
            
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }
            
            if (insuranceTypeId.HasValue)
            {
                query = query.Where(c => c.InsuranceTypeId == insuranceTypeId.Value);
            }
            
            var coverages = await query.OrderBy(c => c.InsuranceType.Category).ThenBy(c => c.Name).ToListAsync();
            var dtos = coverages.Select(MapToDto).ToList();
            
            return Ok(dtos);
        }
        
        // Zorunlu teminatları getir
        [HttpGet("mandatory")]
        public async Task<ActionResult<List<CoverageDto>>> GetMandatoryCoverages()
        {
            var coverages = await _context.Coverages
                .Include(c => c.InsuranceType)
                .Where(c => !c.IsOptional && c.IsActive)
                .OrderBy(c => c.InsuranceType.Category)
                .ThenBy(c => c.Name)
                .ToListAsync();
                
            var dtos = coverages.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        
        // Opsiyonel teminatları getir
        [HttpGet("optional")]
        public async Task<ActionResult<List<CoverageDto>>> GetOptionalCoverages()
        {
            var coverages = await _context.Coverages
                .Include(c => c.InsuranceType)
                .Where(c => c.IsOptional && c.IsActive)
                .OrderBy(c => c.InsuranceType.Category)
                .ThenBy(c => c.Name)
                .ToListAsync();
                
            var dtos = coverages.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        
        // Coverage entity'sini CoverageDto'ya dönüştür
        private static CoverageDto MapToDto(Coverage coverage)
        {
            return new CoverageDto
            {
                Id = coverage.Id,
                Name = coverage.Name,
                Description = coverage.Description,
                Limit = coverage.Limit,
                Premium = coverage.Premium,
                IsOptional = coverage.IsOptional,
                IsActive = coverage.IsActive,
                InsuranceTypeId = coverage.InsuranceTypeId,
                CreatedAt = coverage.CreatedAt,
                UpdatedAt = coverage.UpdatedAt,
                InsuranceType = coverage.InsuranceType != null ? new InsuranceTypeDto
                {
                    Id = coverage.InsuranceType.Id,
                    Name = coverage.InsuranceType.Name,
                    Category = coverage.InsuranceType.Category,
                    Description = coverage.InsuranceType.Description,
                    IsActive = coverage.InsuranceType.IsActive,
                    BasePrice = coverage.InsuranceType.BasePrice,
                    CoverageDetails = coverage.InsuranceType.CoverageDetails,
                    CreatedAt = coverage.InsuranceType.CreatedAt,
                    UpdatedAt = coverage.InsuranceType.UpdatedAt
                } : null
            };
        }
    }
}
