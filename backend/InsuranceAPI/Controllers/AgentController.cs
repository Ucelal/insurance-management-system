using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;
        private readonly ILogger<AgentController> _logger;

        public AgentController(IAgentService agentService, ILogger<AgentController> logger)
        {
            _agentService = agentService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm acentaları listeler
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<AgentDto>>> GetAllAgents()
        {
            try
            {
                var agents = await _agentService.GetAllAgentsAsync();
                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all agents");
                return StatusCode(500, new { message = "Internal server error occurred while retrieving agents" });
            }
        }

        /// <summary>
        /// ID'ye göre acenta getirir
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<AgentDto>> GetAgentById(int id)
        {
            try
            {
                var agent = await _agentService.GetAgentByIdAsync(id);
                if (agent == null)
                {
                    return NotFound(new { message = $"Agent with ID {id} not found" });
                }

                return Ok(agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving agent with ID {Id}", id);
                return StatusCode(500, new { message = "Internal server error occurred while retrieving agent" });
            }
        }

        /// <summary>
        /// User ID'ye göre acenta getirir
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<AgentDto>> GetAgentByUserId(int userId)
        {
            try
            {
                var agent = await _agentService.GetAgentByUserIdAsync(userId);
                if (agent == null)
                {
                    return NotFound(new { message = $"Agent with User ID {userId} not found" });
                }

                return Ok(agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving agent with User ID {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error occurred while retrieving agent" });
            }
        }

        /// <summary>
        /// Departmana göre acentaları listeler
        /// </summary>
        [HttpGet("department/{department}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<AgentDto>>> GetAgentsByDepartment(string department)
        {
            try
            {
                var agents = await _agentService.GetAgentsByDepartmentAsync(department);
                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving agents from department {Department}", department);
                return StatusCode(500, new { message = "Internal server error occurred while retrieving agents by department" });
            }
        }

        /// <summary>
        /// Yeni acenta oluşturur
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<AgentDto>> CreateAgent([FromBody] CreateAgentDto createAgentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var agent = await _agentService.CreateAgentAsync(createAgentDto);
                return CreatedAtAction(nameof(GetAgentById), new { id = agent.UserId }, agent);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating agent");
                return StatusCode(500, new { message = "Internal server error occurred while creating agent" });
            }
        }

        /// <summary>
        /// Acenta bilgilerini günceller
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<AgentDto>> UpdateAgent(int id, [FromBody] UpdateAgentDto updateAgentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var agent = await _agentService.UpdateAgentAsync(id, updateAgentDto);
                if (agent == null)
                {
                    return NotFound(new { message = $"Agent with ID {id} not found" });
                }

                return Ok(agent);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating agent with ID {Id}", id);
                return StatusCode(500, new { message = "Internal server error occurred while updating agent" });
            }
        }

        /// <summary>
        /// Acenta siler
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteAgent(int id)
        {
            try
            {
                var result = await _agentService.DeleteAgentAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Agent with ID {id} not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting agent with ID {Id}", id);
                return StatusCode(500, new { message = "Internal server error occurred while deleting agent" });
            }
        }

        /// <summary>
        /// Acenta kodunun benzersiz olup olmadığını kontrol eder
        /// </summary>
        [HttpGet("check-code/{agentCode}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<bool>> CheckAgentCodeUnique(string agentCode, [FromQuery] int? excludeId = null)
        {
            try
            {
                var isUnique = await _agentService.IsAgentCodeUniqueAsync(agentCode, excludeId);
                return Ok(isUnique);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking agent code uniqueness for {AgentCode}", agentCode);
                return StatusCode(500, new { message = "Internal server error occurred while checking agent code uniqueness" });
            }
        }

        /// <summary>
        /// Acentanın departmanına göre teklifleri getirir
        /// </summary>
        [HttpGet("{agentId}/offers")]
        [Authorize(Roles = "agent")]
        public async Task<ActionResult<IEnumerable<OfferDto>>> GetOffersByAgentDepartment(int agentId)
        {
            try
            {
                var offers = await _agentService.GetOffersByAgentDepartmentAsync(agentId);
                return Ok(offers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving offers for agent {AgentId}", agentId);
                return StatusCode(500, new { message = "Internal server error occurred while retrieving offers" });
            }
        }

        /// <summary>
        /// Acentanın departmanına göre bekleyen teklifleri getirir
        /// </summary>
        [HttpGet("{agentId}/offers/pending")]
        [Authorize(Roles = "agent")]
        public async Task<ActionResult<IEnumerable<OfferDto>>> GetPendingOffersByAgentDepartment(int agentId)
        {
            try
            {
                var offers = await _agentService.GetPendingOffersByAgentDepartmentAsync(agentId);
                return Ok(offers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving pending offers for agent {AgentId}", agentId);
                return StatusCode(500, new { message = "Internal server error occurred while retrieving pending offers" });
            }
        }

        /// <summary>
        /// Teklifi günceller (durum değişikliği, not ekleme vb.)
        /// </summary>
        [HttpPut("{agentId}/offers/{offerId}")]
        [Authorize(Roles = "agent")]
        public async Task<ActionResult<OfferDto>> UpdateOffer(int agentId, int offerId, [FromBody] UpdateOfferDto updateOfferDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var offer = await _agentService.UpdateOfferAsync(agentId, offerId, updateOfferDto);
                if (offer == null)
                {
                    return NotFound(new { message = $"Offer with ID {offerId} not found or not assigned to agent {agentId}" });
                }

                return Ok(offer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating offer {OfferId} for agent {AgentId}", offerId, agentId);
                return StatusCode(500, new { message = "Internal server error occurred while updating offer" });
            }
        }

        /// <summary>
        /// Agent'ın departmanına ait claim'leri getirir
        /// </summary>
        [HttpGet("{agentId}/department-claims")]
        [Authorize(Roles = "agent")]
        public async Task<ActionResult<IEnumerable<ClaimDto>>> GetClaimsByAgentDepartment(int agentId)
        {
            try
            {
                _logger.LogInformation("Getting claims for agent {AgentId}", agentId);
                var claims = await _agentService.GetClaimsByAgentDepartmentAsync(agentId);
                return Ok(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving claims for agent {AgentId}", agentId);
                return StatusCode(500, new { message = "Internal server error occurred while retrieving claims" });
            }
        }
    }
}
