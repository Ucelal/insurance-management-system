using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // JWT authentication aktif
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _policyService;
        
        // Policy service dependency injection
        public PolicyController(IPolicyService policyService)
        {
            _policyService = policyService;
        }
        
        // Tüm poliçeleri getir
        [HttpGet]
        public async Task<ActionResult<List<PolicyDto>>> GetAllPolicies()
        {
            var policies = await _policyService.GetAllPoliciesAsync();
            return Ok(policies);
        }
        
        // ID'ye göre poliçe getir
        [HttpGet("{id}")]
        public async Task<ActionResult<PolicyDto>> GetPolicyById(int id)
        {
            var policy = await _policyService.GetPolicyByIdAsync(id);
            
            if (policy == null)
            {
                return NotFound(new { message = "Poliçe bulunamadı" });
            }
            
            return Ok(policy);
        }
        
        // Yeni poliçe oluştur
        [HttpPost]
        public async Task<ActionResult<PolicyDto>> CreatePolicy([FromBody] CreatePolicyDto createPolicyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _policyService.CreatePolicyAsync(createPolicyDto);
            
            if (result == null)
            {
                return BadRequest(new { message = "Poliçe oluşturulamadı. Teklif bulunamadı veya poliçe numarası zaten kullanımda." });
            }
            
            return CreatedAtAction(nameof(GetPolicyById), new { id = result.Id }, result);
        }
        
        // Poliçe güncelle
        [HttpPut("{id}")]
        public async Task<ActionResult<PolicyDto>> UpdatePolicy(int id, [FromBody] UpdatePolicyDto updatePolicyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _policyService.UpdatePolicyAsync(id, updatePolicyDto);
            
            if (result == null)
            {
                return NotFound(new { message = "Poliçe bulunamadı" });
            }
            
            return Ok(result);
        }
        
        // Poliçe sil
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePolicy(int id)
        {
            var success = await _policyService.DeletePolicyAsync(id);
            
            if (!success)
            {
                return NotFound(new { message = "Poliçe bulunamadı" });
            }
            
            return NoContent();
        }
        
        // Teklif ID'sine göre poliçe getir
        [HttpGet("offer/{offerId}")]
        public async Task<ActionResult<PolicyDto>> GetPolicyByOffer(int offerId)
        {
            var policy = await _policyService.GetPolicyByOfferAsync(offerId);
            
            if (policy == null)
            {
                return NotFound(new { message = "Bu teklif için poliçe bulunamadı" });
            }
            
            return Ok(policy);
        }
        
        // Poliçe numarasına göre getir
        [HttpGet("number/{policyNumber}")]
        public async Task<ActionResult<PolicyDto>> GetPolicyByNumber(string policyNumber)
        {
            var policy = await _policyService.GetPolicyByNumberAsync(policyNumber);
            
            if (policy == null)
            {
                return NotFound(new { message = "Poliçe bulunamadı" });
            }
            
            return Ok(policy);
        }
        
        // Poliçe arama
        [HttpGet("search")]
        public async Task<ActionResult<List<PolicyDto>>> SearchPolicies([FromQuery] string? policyNumber, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var policies = await _policyService.SearchPoliciesAsync(policyNumber, startDate, endDate);
            return Ok(policies);
        }
    }
}
