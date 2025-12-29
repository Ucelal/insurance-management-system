using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.Models;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly InsuranceDbContext _context;

        public AdminController(InsuranceDbContext context)
        {
            _context = context;
        }

        // Mevcut admin'leri Agents tablosuna ekle
        [HttpPost("add-admins-to-agents")]
        public async Task<ActionResult> AddAdminsToAgents()
        {
            try
            {
                var adminsWithoutAgents = await _context.Users
                    .Where(u => u.Role == "admin")
                    .Where(u => !_context.Agents.Any(a => a.UserId == u.UserId))
                    .ToListAsync();

                var addedAgents = new List<Agent>();

                foreach (var admin in adminsWithoutAgents)
                {
                    var agent = new Agent
                    {
                        UserId = admin.UserId,
                        AgentCode = "ADM",
                        Department = "Admin",
                        Address = "Admin Adresi",
                        Phone = "0555-000-0000"
                    };

                    _context.Agents.Add(agent);
                    addedAgents.Add(agent);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"{addedAgents.Count} admin kullanıcısı Agents tablosuna eklendi.",
                    addedAgents = addedAgents.Select(a => new
                    {
                        AgentId = a.AgentId,
                        UserId = a.UserId,
                        AgentCode = a.AgentCode,
                        Department = a.Department
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hata oluştu", error = ex.Message });
            }
        }

        // Admin'lerin Agent durumunu kontrol et
        [HttpGet("check-admin-agents")]
        public async Task<ActionResult> CheckAdminAgents()
        {
            try
            {
                var admins = await _context.Users
                    .Where(u => u.Role == "admin")
                    .Select(u => new
                    {
                        UserId = u.UserId,
                        Name = u.Name,
                        Email = u.Email,
                        HasAgent = _context.Agents.Any(a => a.UserId == u.UserId),
                        AgentId = _context.Agents
                            .Where(a => a.UserId == u.UserId)
                            .Select(a => a.AgentId)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    admins = admins,
                    totalAdmins = admins.Count,
                    adminsWithAgents = admins.Count(a => a.HasAgent),
                    adminsWithoutAgents = admins.Count(a => !a.HasAgent)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hata oluştu", error = ex.Message });
            }
        }
    }
}


