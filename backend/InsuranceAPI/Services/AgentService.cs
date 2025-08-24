using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAPI.Services
{
    public class AgentService : IAgentService
    {
        private readonly InsuranceDbContext _context;
        private readonly ILogger<AgentService> _logger;

        public AgentService(InsuranceDbContext context, ILogger<AgentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AgentDto>> GetAllAgentsAsync()
        {
            try
            {
                var agents = await _context.Agents
                    .Include(a => a.User)
                    .Select(a => new AgentDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        AgentCode = a.AgentCode,
                        Department = a.Department,
                        Address = a.Address,
                        Phone = a.Phone,
                        User = a.User != null ? new UserDto
                        {
                            Id = a.User.Id,
                            Name = a.User.Name,
                            Email = a.User.Email,
                            Role = a.User.Role,
                            CreatedAt = a.User.CreatedAt
                        } : null
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} agents", agents.Count());
                return agents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all agents");
                throw;
            }
        }

        public async Task<AgentDto?> GetAgentByIdAsync(int id)
        {
            try
            {
                var agent = await _context.Agents
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (agent == null)
                {
                    _logger.LogWarning("Agent with ID {Id} not found", id);
                    return null;
                }

                return new AgentDto
                {
                    Id = agent.Id,
                    UserId = agent.UserId,
                    AgentCode = agent.AgentCode,
                    Department = agent.Department,
                    Address = agent.Address,
                    Phone = agent.Phone,
                    User = agent.User != null ? new UserDto
                    {
                        Id = agent.User.Id,
                        Name = agent.User.Name,
                        Email = agent.User.Email,
                        Role = agent.User.Role,
                        CreatedAt = agent.User.CreatedAt
                    } : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving agent with ID {Id}", id);
                throw;
            }
        }

        public async Task<AgentDto?> GetAgentByUserIdAsync(int userId)
        {
            try
            {
                var agent = await _context.Agents
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (agent == null)
                {
                    _logger.LogWarning("Agent with User ID {UserId} not found", userId);
                    return null;
                }

                return new AgentDto
                {
                    Id = agent.Id,
                    UserId = agent.UserId,
                    AgentCode = agent.AgentCode,
                    Department = agent.Department,
                    Address = agent.Address,
                    Phone = agent.Phone,
                    User = agent.User != null ? new UserDto
                    {
                        Id = agent.User.Id,
                        Name = agent.User.Name,
                        Email = agent.User.Email,
                        Role = agent.User.Role,
                        CreatedAt = agent.User.CreatedAt
                    } : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving agent with User ID {UserId}", userId);
                throw;
            }
        }

        public async Task<AgentDto> CreateAgentAsync(CreateAgentDto createAgentDto)
        {
            try
            {
                // Check if agent code is unique
                if (!await IsAgentCodeUniqueAsync(createAgentDto.AgentCode))
                {
                    throw new InvalidOperationException($"Agent code '{createAgentDto.AgentCode}' is already in use.");
                }

                // Check if user exists and has agent role
                var user = await _context.Users.FindAsync(createAgentDto.UserId);
                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {createAgentDto.UserId} not found.");
                }

                if (user.Role != "agent")
                {
                    throw new InvalidOperationException($"User must have Agent role to be assigned as an agent.");
                }

                var agent = new Agent
                {
                    UserId = createAgentDto.UserId,
                    AgentCode = createAgentDto.AgentCode,
                    Department = createAgentDto.Department,
                    Address = createAgentDto.Address,
                    Phone = createAgentDto.Phone
                };

                _context.Agents.Add(agent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new agent with ID {Id} and code {AgentCode}", agent.Id, agent.AgentCode);

                // Return the created agent with user details
                return await GetAgentByIdAsync(agent.Id) ?? throw new InvalidOperationException("Failed to retrieve created agent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating agent");
                throw;
            }
        }

        public async Task<AgentDto?> UpdateAgentAsync(int id, UpdateAgentDto updateAgentDto)
        {
            try
            {
                var agent = await _context.Agents.FindAsync(id);
                if (agent == null)
                {
                    _logger.LogWarning("Agent with ID {Id} not found for update", id);
                    return null;
                }

                // Check if agent code is unique (excluding current agent)
                if (!string.IsNullOrEmpty(updateAgentDto.AgentCode) && 
                    !await IsAgentCodeUniqueAsync(updateAgentDto.AgentCode, id))
                {
                    throw new InvalidOperationException($"Agent code '{updateAgentDto.AgentCode}' is already in use.");
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(updateAgentDto.AgentCode))
                    agent.AgentCode = updateAgentDto.AgentCode;
                if (!string.IsNullOrEmpty(updateAgentDto.Department))
                    agent.Department = updateAgentDto.Department;
                if (!string.IsNullOrEmpty(updateAgentDto.Address))
                    agent.Address = updateAgentDto.Address;
                if (!string.IsNullOrEmpty(updateAgentDto.Phone))
                    agent.Phone = updateAgentDto.Phone;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated agent with ID {Id}", id);

                return await GetAgentByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating agent with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAgentAsync(int id)
        {
            try
            {
                var agent = await _context.Agents.FindAsync(id);
                if (agent == null)
                {
                    _logger.LogWarning("Agent with ID {Id} not found for deletion", id);
                    return false;
                }

                // Check if agent has any offers
                var hasOffers = await _context.Offers.AnyAsync(o => o.AgentId == id);
                if (hasOffers)
                {
                    throw new InvalidOperationException("Cannot delete agent with existing offers. Please reassign or delete offers first.");
                }

                _context.Agents.Remove(agent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted agent with ID {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting agent with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<AgentDto>> GetAgentsByDepartmentAsync(string department)
        {
            try
            {
                var agents = await _context.Agents
                    .Include(a => a.User)
                    .Where(a => a.Department == department)
                    .Select(a => new AgentDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        AgentCode = a.AgentCode,
                        Department = a.Department,
                        Address = a.Address,
                        Phone = a.Phone,
                        User = a.User != null ? new UserDto
                        {
                            Id = a.User.Id,
                            Name = a.User.Name,
                            Email = a.User.Email,
                            Role = a.User.Role,
                            CreatedAt = a.User.CreatedAt
                        } : null
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} agents from department {Department}", agents.Count(), department);
                return agents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving agents from department {Department}", department);
                throw;
            }
        }

        public async Task<bool> IsAgentCodeUniqueAsync(string agentCode, int? excludeId = null)
        {
            try
            {
                var query = _context.Agents.Where(a => a.AgentCode == agentCode);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(a => a.Id != excludeId.Value);
                }

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking agent code uniqueness for {AgentCode}", agentCode);
                throw;
            }
        }
    }
}
