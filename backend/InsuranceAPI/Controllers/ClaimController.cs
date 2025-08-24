using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;
using InsuranceAPI.Models;
using System.Security.Claims;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // JWT authentication aktif
    public class ClaimController : ControllerBase
    {
        private readonly IClaimService _claimService;
        
        public ClaimController(IClaimService claimService)
        {
            _claimService = claimService;
        }
        
        // Tüm hasarları getir (admin ve agent)
        [HttpGet]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<ClaimDto>>> GetAllClaims()
        {
            try
            {
                var claims = await _claimService.GetAllClaimsAsync();
                return Ok(claims);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hasar listesi alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // ID'ye göre hasar getir (admin ve agent)
        [HttpGet("{id}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<ClaimDto>> GetClaimById(int id)
        {
            try
            {
                var claim = await _claimService.GetClaimByIdAsync(id);
                if (claim == null)
                {
                    return NotFound(new { message = "Hasar bulunamadı" });
                }
                
                return Ok(claim);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hasar bilgisi alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Poliçeye göre hasarları getir (admin ve agent)
        [HttpGet("policy/{policyId}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<ClaimDto>>> GetClaimsByPolicy(int policyId)
        {
            try
            {
                var claims = await _claimService.GetClaimsByPolicyAsync(policyId);
                return Ok(claims);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Poliçe hasarları alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Kullanıcının hasarlarını getir (admin ve agent)
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<ClaimDto>>> GetClaimsByUser(int userId)
        {
            try
            {
                var claims = await _claimService.GetClaimsByUserAsync(userId);
                return Ok(claims);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Kullanıcı hasarları alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Yeni hasar oluştur
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ClaimDto>> CreateClaim([FromBody] CreateClaimDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                // Kullanıcı ID'sini JWT token'dan al
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }
                
                var claim = await _claimService.CreateClaimAsync(createDto, userId);
                return CreatedAtAction(nameof(GetClaimById), new { id = claim.Id }, claim);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hasar oluşturulurken hata oluştu", error = ex.Message });
            }
        }
        
        // Hasar güncelle (admin ve agent)
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<ClaimDto>> UpdateClaim(int id, [FromBody] UpdateClaimDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                // Kullanıcı ID'sini JWT token'dan al
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }
                
                var claim = await _claimService.UpdateClaimAsync(id, updateDto, userId);
                return Ok(claim);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hasar güncellenirken hata oluştu", error = ex.Message });
            }
        }
        
        // Hasar sil (admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteClaim(int id)
        {
            try
            {
                var result = await _claimService.DeleteClaimAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Hasar bulunamadı" });
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hasar silinirken hata oluştu", error = ex.Message });
            }
        }
        
        // Hasar istatistikleri (admin ve agent)
        [HttpGet("statistics")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<ClaimStatisticsDto>> GetClaimStatistics()
        {
            try
            {
                var statistics = await _claimService.GetClaimStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hasar istatistikleri alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Hasar arama (admin ve agent)
        [HttpGet("search")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<ClaimDto>>> SearchClaims([FromQuery] ClaimSearchDto searchDto)
        {
            try
            {
                var claims = await _claimService.SearchClaimsAsync(searchDto);
                return Ok(claims);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hasar arama yapılırken hata oluştu", error = ex.Message });
            }
        }
        
        // Hasar işleme (admin ve agent)
        [HttpPost("{id}/process")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<ClaimDto>> ProcessClaim(int id, [FromBody] ProcessClaimDto processDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                // Kullanıcı ID'sini JWT token'dan al
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }
                
                var claim = await _claimService.ProcessClaimAsync(id, processDto.Status, processDto.Notes ?? string.Empty, userId);
                return Ok(claim);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hasar işlenirken hata oluştu", error = ex.Message });
            }
        }
        
        // Hasar durumları (admin ve agent)
        [HttpGet("statuses")]
        [Authorize(Roles = "admin,agent")]
        public ActionResult<List<string>> GetClaimStatuses()
        {
            var statuses = Enum.GetNames(typeof(ClaimStatus));
            return Ok(statuses);
        }
        
        // Hasar türleri (admin ve agent)
        [HttpGet("types")]
        [Authorize(Roles = "admin,agent")]
        public ActionResult<List<string>> GetClaimTypes()
        {
            var types = Enum.GetNames(typeof(ClaimType));
            return Ok(types);
        }
        
        // Hasar öncelikleri (admin ve agent)
        [HttpGet("priorities")]
        [Authorize(Roles = "admin,agent")]
        public ActionResult<List<string>> GetClaimPriorities()
        {
            var priorities = Enum.GetNames(typeof(ClaimPriority));
            return Ok(priorities);
        }
    }
    
    // Hasar işleme için DTO
    public class ProcessClaimDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Status { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
    }
}
