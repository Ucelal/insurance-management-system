using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly InsuranceDbContext _context;

        public HealthController(InsuranceDbContext context)
        {
            _context = context;
        }

        // Health check endpoint
        [HttpGet]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                // Check database connection
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    return Ok(new
                    {
                        status = "healthy",
                        timestamp = DateTime.UtcNow,
                        database = "connected",
                        message = "API is running and database is accessible"
                    });
                }
                else
                {
                    return StatusCode(503, new
                    {
                        status = "unhealthy",
                        timestamp = DateTime.UtcNow,
                        database = "disconnected",
                        message = "API is running but database is not accessible"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    timestamp = DateTime.UtcNow,
                    database = "error",
                    message = "Health check failed",
                    error = ex.Message
                });
            }
        }

        // Detailed health check
        [HttpGet("detailed")]
        public async Task<IActionResult> DetailedHealthCheck()
        {
            try
            {
                var healthStatus = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    services = new
                    {
                        database = await CheckDatabaseHealth(),
                        memory = CheckMemoryHealth(),
                        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
                    }
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }

        private async Task<object> CheckDatabaseHealth()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                
                return new
                {
                    status = canConnect ? "connected" : "disconnected",
                    pendingMigrations = pendingMigrations.Count(),
                    connectionString = _context.Database.GetConnectionString() != null ? "configured" : "not configured"
                };
            }
            catch
            {
                return new
                {
                    status = "error",
                    pendingMigrations = 0,
                    connectionString = "error"
                };
            }
        }

        private object CheckMemoryHealth()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var memoryUsage = process.WorkingSet64 / 1024 / 1024; // MB
            
            return new
            {
                memoryUsageMB = memoryUsage,
                maxMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
                gcCollections = GC.CollectionCount(0)
            };
        }
    }
}
