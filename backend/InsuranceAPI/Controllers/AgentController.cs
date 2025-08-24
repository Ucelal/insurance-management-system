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
                return CreatedAtAction(nameof(GetAgentById), new { id = agent.Id }, agent);
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
    }
}
