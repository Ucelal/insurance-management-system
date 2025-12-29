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
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        
        // Customer service dependency injection
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        
        // Tüm müşterileri getir
        [HttpGet]
        public async Task<ActionResult<List<CustomerDto>>> GetAllCustomers()
        {
            try
            {
                // JWT token'dan kullanıcı rolünü al
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                Console.WriteLine($"CustomerController.GetAllCustomers - UserRole: {userRole}, UserId: {userId}");
                Console.WriteLine($"All Claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
                
                List<CustomerDto> customers;
                
                if (userRole == "admin")
                {
                    Console.WriteLine("Admin kullanıcısı - Tüm müşteriler getiriliyor");
                    // Admin tüm müşterileri görebilir
                    customers = await _customerService.GetAllCustomersAsync();
                }
                else if (userRole == "agent")
                {
                    Console.WriteLine($"Agent kullanıcısı (ID: {userId}) - Departman müşterileri getiriliyor");
                    // Agent sadece kendi departmanındaki müşterileri görebilir
                    customers = await _customerService.GetCustomersByAgentDepartmentAsync(userId);
                }
                else
                {
                    Console.WriteLine($"Customer kullanıcısı (ID: {userId}) - Kendi bilgileri getiriliyor");
                    // Customer sadece kendi bilgilerini görebilir
                    var customer = await _customerService.GetCustomerByUserIdAsync(userId);
                    customers = customer != null ? new List<CustomerDto> { customer } : new List<CustomerDto>();
                }
                
                Console.WriteLine($"Toplam {customers.Count} müşteri döndürüldü");
                return Ok(customers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CustomerController.GetAllCustomers - Hata: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Müşteri verileri alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // ID'ye göre müşteri getir
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            
            if (customer == null)
            {
                return NotFound(new { message = "Müşteri bulunamadı" });
            }
            
            return Ok(customer);
        }
        
        // Yeni müşteri oluştur
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto createCustomerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _customerService.CreateCustomerAsync(createCustomerDto);
            
            if (result == null)
            {
                return BadRequest(new { message = "Müşteri oluşturulamadı. ID No zaten kullanımda olabilir." });
            }
            
            return CreatedAtAction(nameof(GetCustomerById), new { id = result.CustomerId }, result);
        }
        
        // Müşteri güncelle
        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerDto>> UpdateCustomer(int id, [FromBody] UpdateCustomerDto updateCustomerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _customerService.UpdateCustomerAsync(id, updateCustomerDto);
            
            if (result == null)
            {
                return NotFound(new { message = "Müşteri bulunamadı" });
            }
            
            return Ok(result);
        }
        
        // Müşteri sil
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            var success = await _customerService.DeleteCustomerAsync(id);
            
            if (!success)
            {
                return NotFound(new { message = "Müşteri bulunamadı" });
            }
            
            return NoContent();
        }
        
        // Müşteri türlerini getir
        [HttpGet("types")]
        public ActionResult GetCustomerTypes()
        {
            var types = new[]
            {
                new { Value = "bireysel", Label = "Bireysel" },
                new { Value = "kurumsal", Label = "Kurumsal" }
            };
            
            return Ok(types);
        }
        
        // Müşteri arama
        [HttpGet("search")]
        public async Task<ActionResult<List<CustomerDto>>> SearchCustomers([FromQuery] string? name, [FromQuery] string? type, [FromQuery] string? idNo)
        {
            var customers = await _customerService.SearchCustomersAsync(name, type, idNo);
            return Ok(customers);
        }
        
        // Debug endpoint - validation test
        [HttpPost("debug")]
        public ActionResult DebugCreateCustomer([FromBody] CreateCustomerDto createCustomerDto)
        {
            return Ok(new { 
                receivedData = createCustomerDto,
                modelStateErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(),
                isValid = ModelState.IsValid
            });
        }
        
        // Debug endpoint - JWT token test
        [HttpGet("debug/token")]
        [Authorize]
        public ActionResult DebugToken()
        {
            var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
            return Ok(new { 
                message = "JWT Token doğrulandı",
                claims = claims,
                userRole = userRole,
                userId = userId,
                isAuthenticated = User.Identity?.IsAuthenticated ?? false
            });
        }
        
        // Müşteri istatistikleri - dashboard için
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetCustomerStatistics()
        {
            var stats = await _customerService.GetCustomerStatisticsAsync();
            return Ok(stats);
        }
        
        
        // Müşteri aktivite geçmişi
        [HttpGet("{id}/activity")]
        public async Task<ActionResult<List<object>>> GetCustomerActivity(int id)
        {
            var activities = await _customerService.GetCustomerActivityAsync(id);
            return Ok(activities);
        }
        
        // Toplu müşteri güncelleme
        [HttpPut("bulk")]
        public async Task<ActionResult<object>> BulkUpdateCustomers([FromBody] List<BulkUpdateCustomerDto> updates)
        {
            var result = await _customerService.BulkUpdateCustomersAsync(updates);
            return Ok(result);
        }
        
        // Müşteri export (CSV format)
        [HttpGet("export")]
        public async Task<IActionResult> ExportCustomers([FromQuery] string? format = "csv")
        {
            var fileContent = await _customerService.ExportCustomersAsync(format);
            
            if (format?.ToLower() == "csv")
            {
                return File(System.Text.Encoding.UTF8.GetBytes(fileContent), "text/csv", "customers.csv");
            }
            
            return Ok(fileContent);
        }
        
        // Müşteri import (CSV format)
        [HttpPost("import")]
        public async Task<ActionResult<object>> ImportCustomers(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Dosya yüklenmedi" });
            }
            
            var result = await _customerService.ImportCustomersAsync(file);
            return Ok(result);
        }
    }
}
