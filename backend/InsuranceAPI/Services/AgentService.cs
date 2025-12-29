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
                        Id = a.AgentId,
                        UserId = a.UserId,
                        AgentCode = a.AgentCode ?? string.Empty,
                        Department = a.Department ?? string.Empty,
                        Address = a.Address ?? string.Empty,
                        Phone = a.Phone ?? string.Empty,
                        User = a.User != null ? new UserDto
                        {
                            Id = a.User.UserId,
                            Name = a.User.Name ?? string.Empty,
                            Email = a.User.Email ?? string.Empty,
                            Role = a.User.Role ?? string.Empty,
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
                    .FirstOrDefaultAsync(a => a.AgentId == id);

                if (agent == null)
                {
                    _logger.LogWarning("Agent with ID {Id} not found", id);
                    return null;
                }

                return new AgentDto
                {
                    Id = agent.AgentId,
                    UserId = agent.UserId,
                    AgentCode = agent.AgentCode ?? string.Empty,
                    Department = agent.Department ?? string.Empty,
                    Address = agent.Address ?? string.Empty,
                    Phone = agent.Phone ?? string.Empty,
                    User = agent.User != null ? new UserDto
                    {
                        Id = agent.User.UserId,
                        Name = agent.User.Name ?? string.Empty,
                        Email = agent.User.Email ?? string.Empty,
                        Role = agent.User.Role ?? string.Empty,
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
                    Id = agent.AgentId,
                    UserId = agent.UserId,
                    AgentCode = agent.AgentCode ?? string.Empty,
                    Department = agent.Department ?? string.Empty,
                    Address = agent.Address ?? string.Empty,
                    Phone = agent.Phone ?? string.Empty,
                    User = agent.User != null ? new UserDto
                    {
                        Id = agent.User.UserId,
                        Name = agent.User.Name ?? string.Empty,
                        Email = agent.User.Email ?? string.Empty,
                        Role = agent.User.Role ?? string.Empty,
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

        /// <summary>
        /// Departmana göre otomatik acenta kodu oluşturur
        /// </summary>
        private string GenerateAgentCodeByDepartment(string department, string userRole)
        {
            // Admin için özel kod
            if (userRole == "Admin" || userRole == "admin")
            {
                return "ADM";
            }

            // Departman kodları mapping
            var departmentCodeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Konut Sigortası", "KON" },
                { "Seyahat Sigortası", "SEY" },
                { "İşyeri Sigortası", "İŞ" },
                { "İş Yeri Sigortası", "İŞ" },
                { "Trafik Sigortası", "TRA" },
                { "Sağlık Sigortası", "SAĞ" },
                { "Hayat Sigortası", "HAY" }
            };

            if (departmentCodeMap.TryGetValue(department, out var code))
            {
                return code;
            }

            // Eğer mapping'de yoksa, departman isminin ilk 3 harfini al
            return department.Length >= 3 
                ? department.Substring(0, 3).ToUpper() 
                : department.ToUpper();
        }

        public async Task<AgentDto> CreateAgentAsync(CreateAgentDto createAgentDto)
        {
            try
            {
                // Check if user exists and has agent role
                var user = await _context.Users.FindAsync(createAgentDto.UserId);
                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {createAgentDto.UserId} not found.");
                }

                if (user.Role != "agent" && user.Role != "Admin")
                {
                    throw new InvalidOperationException($"User must have Agent or Admin role to be assigned as an agent.");
                }

                // Otomatik acenta kodu oluştur (departmana göre)
                string agentCode = GenerateAgentCodeByDepartment(createAgentDto.Department, user.Role);
                
                _logger.LogInformation("Generated agent code '{AgentCode}' for department '{Department}'", 
                    agentCode, createAgentDto.Department);

                var agent = new Agent
                {
                    UserId = createAgentDto.UserId,
                    AgentCode = agentCode,
                    Department = createAgentDto.Department,
                    Address = createAgentDto.Address,
                    Phone = createAgentDto.Phone
                };

                _context.Agents.Add(agent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new agent with ID {Id} and code {AgentCode}", agent.AgentId, agent.AgentCode);

                // Return the created agent with user details
                return await GetAgentByIdAsync(agent.AgentId) ?? throw new InvalidOperationException("Failed to retrieve created agent");
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
                var agent = await _context.Agents
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AgentId == id);
                    
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

                // Update User fields if provided
                if (agent.User != null)
                {
                    if (!string.IsNullOrEmpty(updateAgentDto.Name))
                    {
                        agent.User.Name = updateAgentDto.Name;
                        _logger.LogInformation("Updated agent user name to '{Name}'", updateAgentDto.Name);
                    }
                    
                    if (!string.IsNullOrEmpty(updateAgentDto.Email))
                    {
                        // Only check if email is unique if it's actually changing
                        if (agent.User.Email != updateAgentDto.Email)
                        {
                            var emailExists = await _context.Users
                                .AnyAsync(u => u.Email == updateAgentDto.Email && u.UserId != agent.UserId);
                            if (emailExists)
                            {
                                throw new InvalidOperationException($"Email '{updateAgentDto.Email}' is already in use.");
                            }
                            agent.User.Email = updateAgentDto.Email;
                            _logger.LogInformation("Updated agent user email to '{Email}'", updateAgentDto.Email);
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(updateAgentDto.Password))
                    {
                        agent.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateAgentDto.Password);
                        _logger.LogInformation("Updated agent user password");
                    }
                }

                // Update Agent fields if provided
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
                var agent = await _context.Agents
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AgentId == id);
                
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

                // Store the user ID before deleting the agent
                var userId = agent.UserId;

                // Delete the agent first
                _context.Agents.Remove(agent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted agent with ID {Id}", id);

                // Delete the associated user if exists
                if (userId.HasValue)
                {
                    var user = await _context.Users.FindAsync(userId.Value);
                    if (user != null)
                    {
                        _context.Users.Remove(user);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Deleted associated user with ID {UserId}", userId.Value);
                    }
                }

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
                        Id = a.AgentId,
                        UserId = a.UserId,
                        AgentCode = a.AgentCode,
                        Department = a.Department,
                        Address = a.Address,
                        Phone = a.Phone,
                        User = a.User != null ? new UserDto
                        {
                            Id = a.User.UserId,
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
                    query = query.Where(a => a.AgentId != excludeId.Value);
                }

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking agent code uniqueness for {AgentCode}", agentCode);
                throw;
            }
        }

        public async Task<IEnumerable<OfferDto>> GetOffersByAgentDepartmentAsync(int agentId)
        {
            try
            {
                Console.WriteLine($"🔍 AgentService: Getting offers for agent ID: {agentId}");
                
                // Önce acentanın departmanını bul
                var agent = await _context.Agents.FindAsync(agentId);
                if (agent == null)
                {
                    Console.WriteLine($"❌ AgentService: Agent with ID {agentId} not found");
                    throw new InvalidOperationException($"Agent with ID {agentId} not found.");
                }

                var department = agent.Department;
                Console.WriteLine($"🏢 AgentService: Agent department: {department}");
                
                if (string.IsNullOrEmpty(department))
                {
                    Console.WriteLine($"⚠️ AgentService: Agent {agentId} has no department assigned");
                    _logger.LogWarning("Agent {AgentId} has no department assigned", agentId);
                    return new List<OfferDto>();
                }

                // Departmana göre teklifleri getir
                Console.WriteLine($"🔍 AgentService: Looking for offers with department: '{department}'");
                
                // Tüm teklifleri listele (debug için)
                var allOffers = await _context.Offers.ToListAsync();
                Console.WriteLine($"📊 AgentService: Total offers in DB: {allOffers.Count}");
                foreach (var o in allOffers)
                {
                    Console.WriteLine($"   Offer ID: {o.OfferId}, Department: '{o.Department}', Status: {o.Status}");
                }
                
                var offers = await _context.Offers
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.InsuranceType)
                    .Where(o => o.Department == department)
                    .Select(o => new OfferDto
                    {
                        OfferId = o.OfferId,
                        CustomerId = o.CustomerId,
                        AgentId = o.AgentId,
                        InsuranceTypeId = o.InsuranceTypeId,
                        Department = o.Department ?? string.Empty,
                        BasePrice = o.BasePrice,
                        DiscountRate = o.DiscountRate,
                        FinalPrice = o.FinalPrice,
                        Status = o.Status ?? string.Empty,
                        CoverageAmount = o.CoverageAmount,
                        RequestedStartDate = o.RequestedStartDate,
                        CustomerAdditionalInfo = o.CustomerAdditionalInfo ?? string.Empty,
                        IsCustomerApproved = o.IsCustomerApproved,
                        ValidUntil = o.ValidUntil,
                        CreatedAt = o.CreatedAt,
                        UpdatedAt = o.UpdatedAt,
                        Customer = o.Customer != null ? new CustomerDto
                        {
                            CustomerId = o.Customer.CustomerId,
                            UserId = o.Customer.UserId,
                            IdNo = o.Customer.IdNo ?? string.Empty,
                            Address = o.Customer.Address ?? string.Empty,
                            Phone = o.Customer.Phone ?? string.Empty,
                            User = o.Customer.User != null ? new UserDto
                            {
                                UserId = o.Customer.User.UserId,
                                Name = o.Customer.User.Name ?? string.Empty,
                                Email = o.Customer.User.Email ?? string.Empty,
                                Role = o.Customer.User.Role ?? string.Empty,
                                CreatedAt = o.Customer.User.CreatedAt
                            } : null
                        } : null,
                        InsuranceType = o.InsuranceType != null ? new InsuranceTypeDto
                        {
                            InsuranceTypeId = o.InsuranceType.InsuranceTypeId,
                            Name = o.InsuranceType.Name ?? string.Empty,
                            Description = o.InsuranceType.Description ?? string.Empty,
                            Category = o.InsuranceType.Category ?? string.Empty,
                            BasePrice = o.InsuranceType.BasePrice,
                            IsActive = o.InsuranceType.IsActive
                        } : null
                    })
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} offers for agent {AgentId} in department {Department}", 
                    offers.Count(), agentId, department);
                return offers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving offers for agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<IEnumerable<OfferDto>> GetPendingOffersByAgentDepartmentAsync(int agentId)
        {
            try
            {
                // Önce acentanın departmanını bul
                var agent = await _context.Agents.FindAsync(agentId);
                if (agent == null)
                {
                    throw new InvalidOperationException($"Agent with ID {agentId} not found.");
                }

                var department = agent.Department;
                if (string.IsNullOrEmpty(department))
                {
                    _logger.LogWarning("Agent {AgentId} has no department assigned", agentId);
                    return new List<OfferDto>();
                }

                // Departmana göre bekleyen teklifleri getir
                var offers = await _context.Offers
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.InsuranceType)
                    .Where(o => o.Department == department && o.Status == "Pending")
                    .Select(o => new OfferDto
                    {
                        OfferId = o.OfferId,
                        CustomerId = o.CustomerId,
                        AgentId = o.AgentId,
                        InsuranceTypeId = o.InsuranceTypeId,
                        Department = o.Department ?? string.Empty,
                        BasePrice = o.BasePrice,
                        DiscountRate = o.DiscountRate,
                        FinalPrice = o.FinalPrice,
                        Status = o.Status ?? string.Empty,
                        CoverageAmount = o.CoverageAmount,
                        RequestedStartDate = o.RequestedStartDate,
                        CustomerAdditionalInfo = o.CustomerAdditionalInfo ?? string.Empty,
                        IsCustomerApproved = o.IsCustomerApproved,
                        ValidUntil = o.ValidUntil,
                        CreatedAt = o.CreatedAt,
                        UpdatedAt = o.UpdatedAt,
                        Customer = o.Customer != null ? new CustomerDto
                        {
                            CustomerId = o.Customer.CustomerId,
                            UserId = o.Customer.UserId,
                            IdNo = o.Customer.IdNo ?? string.Empty,
                            Address = o.Customer.Address ?? string.Empty,
                            Phone = o.Customer.Phone ?? string.Empty,
                            User = o.Customer.User != null ? new UserDto
                            {
                                UserId = o.Customer.User.UserId,
                                Name = o.Customer.User.Name ?? string.Empty,
                                Email = o.Customer.User.Email ?? string.Empty,
                                Role = o.Customer.User.Role ?? string.Empty,
                                CreatedAt = o.Customer.User.CreatedAt
                            } : null
                        } : null,
                        InsuranceType = o.InsuranceType != null ? new InsuranceTypeDto
                        {
                            InsuranceTypeId = o.InsuranceType.InsuranceTypeId,
                            Name = o.InsuranceType.Name ?? string.Empty,
                            Description = o.InsuranceType.Description ?? string.Empty,
                            Category = o.InsuranceType.Category ?? string.Empty,
                            BasePrice = o.InsuranceType.BasePrice,
                            IsActive = o.InsuranceType.IsActive
                        } : null
                    })
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} pending offers for agent {AgentId} in department {Department}", 
                    offers.Count(), agentId, department);
                return offers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving pending offers for agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<OfferDto?> UpdateOfferAsync(int agentId, int offerId, UpdateOfferDto updateOfferDto)
        {
            try
            {
                // Önce acentanın departmanını bul
                var agent = await _context.Agents.FindAsync(agentId);
                if (agent == null)
                {
                    throw new InvalidOperationException($"Agent with ID {agentId} not found.");
                }

                var department = agent.Department;
                if (string.IsNullOrEmpty(department))
                {
                    throw new InvalidOperationException($"Agent {agentId} has no department assigned.");
                }

                // Teklifi bul ve departman kontrolü yap
                var offer = await _context.Offers
                    .Include(o => o.Customer)
                    .Include(o => o.InsuranceType)
                    .FirstOrDefaultAsync(o => o.OfferId == offerId && o.Department == department);

                if (offer == null)
                {
                    _logger.LogWarning("Offer {OfferId} not found or not assigned to agent {AgentId} department {Department}", 
                        offerId, agentId, department);
                    return null;
                }

                // Teklifi güncelle
                offer.Status = updateOfferDto.Status;
                if (updateOfferDto.FinalPrice.HasValue)
                    offer.FinalPrice = updateOfferDto.FinalPrice.Value;
                if (updateOfferDto.DiscountRate.HasValue)
                    offer.DiscountRate = updateOfferDto.DiscountRate.Value;

                offer.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated offer {OfferId} by agent {AgentId}", offerId, agentId);

                // Güncellenmiş teklifi döndür
                return new OfferDto
                {
                    OfferId = offer.OfferId,
                    CustomerId = offer.CustomerId,
                    AgentId = offer.AgentId,
                    InsuranceTypeId = offer.InsuranceTypeId,
                    Department = offer.Department ?? string.Empty,
                    BasePrice = offer.BasePrice,
                    DiscountRate = offer.DiscountRate,
                    FinalPrice = offer.FinalPrice,
                    Status = offer.Status ?? string.Empty,
                    CoverageAmount = offer.CoverageAmount,
                    RequestedStartDate = offer.RequestedStartDate,
                    CustomerAdditionalInfo = offer.CustomerAdditionalInfo ?? string.Empty,
                    CreatedAt = offer.CreatedAt,
                    UpdatedAt = offer.UpdatedAt,
                    Customer = offer.Customer != null ? new CustomerDto
                    {
                        CustomerId = offer.Customer.CustomerId,
                        UserId = offer.Customer.UserId,
                        IdNo = offer.Customer.IdNo ?? string.Empty,
                        Address = offer.Customer.Address ?? string.Empty,
                        Phone = offer.Customer.Phone ?? string.Empty
                    } : null,
                    InsuranceType = offer.InsuranceType != null ? new InsuranceTypeDto
                    {
                        InsuranceTypeId = offer.InsuranceType.InsuranceTypeId,
                        Name = offer.InsuranceType.Name ?? string.Empty,
                        Description = offer.InsuranceType.Description ?? string.Empty,
                        Category = offer.InsuranceType.Category ?? string.Empty,
                        BasePrice = offer.InsuranceType.BasePrice,
                        IsActive = offer.InsuranceType.IsActive
                    } : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating offer {OfferId} for agent {AgentId}", offerId, agentId);
                throw;
            }
        }


        private AgentDto MapToDto(Agent agent)
        {
            return new AgentDto
            {
                Id = agent.AgentId,
                UserId = agent.UserId,
                AgentCode = agent.AgentCode,
                Department = agent.Department,
                Phone = agent.Phone,
                Address = agent.Address,
                User = agent.User != null ? new UserDto
                {
                    UserId = agent.User.UserId,
                    Name = agent.User.Name,
                    Email = agent.User.Email,
                    Role = agent.User.Role
                } : null
            };
        }

        private OfferDto MapToOfferDto(Offer offer)
        {
            return new OfferDto
            {
                OfferId = offer.OfferId,
                CustomerId = offer.CustomerId,
                AgentId = offer.AgentId,
                InsuranceTypeId = offer.InsuranceTypeId,
                Department = offer.Department ?? string.Empty,
                BasePrice = offer.BasePrice,
                DiscountRate = offer.DiscountRate,
                FinalPrice = offer.FinalPrice,
                Status = offer.Status ?? string.Empty,
                ValidUntil = offer.ValidUntil,
                CreatedAt = offer.CreatedAt,
                UpdatedAt = offer.UpdatedAt,
                IsCustomerApproved = offer.IsCustomerApproved,
                CustomerApprovedAt = offer.CustomerApprovedAt,
                ReviewedAt = offer.ReviewedAt,
                ReviewedBy = offer.ReviewedBy,
                CreatedBy = offer.CreatedBy,
                CustomerAdditionalInfo = offer.CustomerAdditionalInfo,
                CoverageAmount = offer.CoverageAmount,
                RequestedStartDate = offer.RequestedStartDate,
                AdminNotes = offer.AdminNotes,
                RejectionReason = offer.RejectionReason,
                CustomerName = offer.Customer?.User?.Name ?? string.Empty,
                CustomerEmail = offer.Customer?.User?.Email ?? string.Empty,
                CustomerPhone = offer.Customer?.Phone ?? string.Empty,
                CustomerAddress = offer.Customer?.Address ?? string.Empty,
                AgentName = offer.Agent?.User?.Name ?? string.Empty,
                InsuranceType = offer.InsuranceType != null ? new InsuranceTypeDto
                {
                    InsuranceTypeId = offer.InsuranceType.InsuranceTypeId,
                    Name = offer.InsuranceType.Name ?? string.Empty,
                    Description = offer.InsuranceType.Description ?? string.Empty,
                    Category = offer.InsuranceType.Category ?? string.Empty,
                    BasePrice = offer.InsuranceType.BasePrice,
                    IsActive = offer.InsuranceType.IsActive
                } : null
            };
        }

        public async Task<IEnumerable<ClaimDto>> GetClaimsByAgentDepartmentAsync(int agentId)
        {
            try
            {
                // Get agent department
                var agent = await _context.Agents
                    .FirstOrDefaultAsync(a => a.AgentId == agentId);

                if (agent == null)
                {
                    _logger.LogWarning("Agent with ID {AgentId} not found", agentId);
                    return Enumerable.Empty<ClaimDto>();
                }

                var department = agent.Department;
                _logger.LogInformation("Getting claims for agent {AgentId} in department {Department}", agentId, department);

                // Get all claims where the policy's offer belongs to this department
                var claims = await _context.Claims
                    .Include(c => c.Policy)
                        .ThenInclude(p => p.Offer)
                            .ThenInclude(o => o.InsuranceType)
                    .Include(c => c.Policy)
                        .ThenInclude(p => p.Offer)
                            .ThenInclude(o => o.Customer)
                                .ThenInclude(c => c.User)
                    .Include(c => c.CreatedByUser)
                        .ThenInclude(u => u.Agent)
                    .Include(c => c.ProcessedByUser)
                        .ThenInclude(u => u.Agent)
                    .Where(c => c.Policy != null && 
                               c.Policy.Offer != null && 
                               c.Policy.Offer.Department == department)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} claims for department {Department}", claims.Count, department);

                return claims.Select(MapClaimToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting claims for agent {AgentId}", agentId);
                throw;
            }
        }

        private ClaimDto MapClaimToDto(Claim claim)
        {
            return new ClaimDto
            {
                ClaimId = claim.ClaimId,
                PolicyId = claim.PolicyId,
                PolicyNumber = claim.Policy?.PolicyNumber ?? string.Empty,
                CreatedByUserId = claim.CreatedByUserId,
                CreatedByUserName = claim.CreatedByUser?.Name ?? string.Empty,
                CreatedByUserEmail = claim.CreatedByUser?.Email,
                ProcessedByUserId = claim.ProcessedByUserId,
                ProcessedByUserName = claim.ProcessedByUser?.Name ?? string.Empty,
                ProcessedByUserEmail = claim.ProcessedByUser?.Email,
                ProcessedByUserPhone = claim.ProcessedByUser?.Agent?.Phone ?? string.Empty,
                Description = claim.Description,
                Status = claim.Status,
                Type = claim.Type,
                ApprovedAmount = claim.ApprovedAmount,
                IncidentDate = claim.IncidentDate,
                CreatedAt = claim.CreatedAt,
                ProcessedAt = claim.ProcessedAt,
                Notes = claim.Notes
            };
        }
    }
}
