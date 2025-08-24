using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;
using InsuranceAPI.Models;

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
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
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
            
            return CreatedAtAction(nameof(GetCustomerById), new { id = result.Id }, result);
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
        
        // Müşteri istatistikleri - dashboard için
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetCustomerStatistics()
        {
            var stats = await _customerService.GetCustomerStatisticsAsync();
            return Ok(stats);
        }
        
        // Müşteri sayısına göre gruplandırma
        [HttpGet("grouped")]
        public async Task<ActionResult<object>> GetCustomersGrouped()
        {
            var grouped = await _customerService.GetCustomersGroupedAsync();
            return Ok(grouped);
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
