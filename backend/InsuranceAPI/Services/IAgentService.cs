using InsuranceAPI.DTOs;
using InsuranceAPI.Models;

namespace InsuranceAPI.Services
{
    public interface IAgentService
    {
        Task<IEnumerable<AgentDto>> GetAllAgentsAsync();
        Task<AgentDto?> GetAgentByIdAsync(int id);
        Task<AgentDto?> GetAgentByUserIdAsync(int userId);
        Task<AgentDto> CreateAgentAsync(CreateAgentDto createAgentDto);
        Task<AgentDto?> UpdateAgentAsync(int id, UpdateAgentDto updateAgentDto);
        Task<bool> DeleteAgentAsync(int id);
        Task<IEnumerable<AgentDto>> GetAgentsByDepartmentAsync(string department);
        Task<bool> IsAgentCodeUniqueAsync(string agentCode, int? excludeId = null);
        Task<IEnumerable<OfferDto>> GetOffersByAgentDepartmentAsync(int agentId);
        Task<IEnumerable<OfferDto>> GetPendingOffersByAgentDepartmentAsync(int agentId);
        Task<OfferDto?> UpdateOfferAsync(int agentId, int offerId, UpdateOfferDto updateOfferDto);
        Task<IEnumerable<ClaimDto>> GetClaimsByAgentDepartmentAsync(int agentId);
    }
}
