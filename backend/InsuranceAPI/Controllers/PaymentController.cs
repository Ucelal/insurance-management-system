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
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        
        // Tüm ödemeleri getir (admin ve agent)
        [HttpGet]
        [Authorize(Roles = "admin,agent")] // Basit role-based authorization
        public async Task<ActionResult<List<PaymentDto>>> GetAllPayments()
        {
            try
            {
                var payments = await _paymentService.GetAllPaymentsAsync();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ödeme listesi alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // ID'ye göre ödeme getir
        [HttpGet("{id}")]
        [Authorize(Roles = "admin,agent")] // Basit role-based authorization
        public async Task<ActionResult<PaymentDto>> GetPaymentById(int id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new { message = "Ödeme bulunamadı" });
                }
                
                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ödeme bilgisi alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Poliçeye göre ödemeleri getir
        [HttpGet("policy/{policyId}")]
        [Authorize(Roles = "admin,agent")] // Admin ve Agent erişimi
        public async Task<ActionResult<List<PaymentDto>>> GetPaymentsByPolicy(int policyId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByPolicyAsync(policyId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Poliçe ödemeleri alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Duruma göre ödemeleri getir
        [HttpGet("status/{status}")]
        [Authorize(Roles = "admin,agent")] // Admin ve Agent erişimi
        public async Task<ActionResult<List<PaymentDto>>> GetPaymentsByStatus(string status)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByStatusAsync(status);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Durum ödemeleri alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Yeni ödeme oluştur
        [HttpPost]
        [Authorize(Roles = "admin,agent")] // Admin ve Agent erişimi
        public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var payment = await _paymentService.CreatePaymentAsync(createDto);
                return CreatedAtAction(nameof(GetPaymentById), new { id = payment.UserId }, payment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ödeme oluşturulurken hata oluştu", error = ex.Message });
            }
        }
        
        // Ödeme simülasyonu
        [HttpPost("simulate")]
        [Authorize(Roles = "admin,agent")] // Admin ve Agent erişimi
        public async Task<ActionResult<PaymentDto>> SimulatePayment([FromBody] CreatePaymentDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var payment = await _paymentService.SimulatePaymentAsync(createDto);
                return Ok(payment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ödeme simülasyonu yapılırken hata oluştu", error = ex.Message });
            }
        }
        
        // Ödeme güncelle (admin ve agent)
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<PaymentDto>> UpdatePayment(int id, [FromBody] UpdatePaymentDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var payment = await _paymentService.UpdatePaymentAsync(id, updateDto);
                return Ok(payment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ödeme güncellenirken hata oluştu", error = ex.Message });
            }
        }
        
        // Ödeme sil (admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            try
            {
                var result = await _paymentService.DeletePaymentAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Ödeme bulunamadı" });
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ödeme silinirken hata oluştu", error = ex.Message });
            }
        }
        
        // Ödeme işleme (admin ve agent)
        [HttpPost("{id}/process")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<PaymentDto>> ProcessPayment(int id, [FromBody] ProcessPaymentDto processDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var payment = await _paymentService.ProcessPaymentAsync(id, processDto);
                return Ok(payment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ödeme işlenirken hata oluştu", error = ex.Message });
            }
        }
        
        // Ödeme istatistikleri (admin ve agent)
        [HttpGet("statistics")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<PaymentStatisticsDto>> GetPaymentStatistics()
        {
            try
            {
                var statistics = await _paymentService.GetPaymentStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ödeme istatistikleri alınırken hata oluştu", error = ex.Message });
            }
        }
        
        // Ödeme arama (admin ve agent)
        [HttpGet("search")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<PaymentDto>>> SearchPayments([FromQuery] PaymentSearchDto searchDto)
        {
            try
            {
                var payments = await _paymentService.SearchPaymentsAsync(searchDto);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ödeme arama yapılırken hata oluştu", error = ex.Message });
            }
        }
        
        // Ödeme yöntemleri
        [HttpGet("methods")]
        [Authorize(Roles = "admin,agent")] // Admin ve Agent erişimi
        public ActionResult<List<string>> GetPaymentMethods()
        {
            var methods = Enum.GetNames(typeof(PaymentMethod));
            return Ok(methods);
        }
        
        // Ödeme durumları
        [HttpGet("statuses")]
        [Authorize(Roles = "admin,agent")] // Admin ve Agent erişimi
        public ActionResult<List<string>> GetPaymentStatuses()
        {
            var statuses = Enum.GetNames(typeof(PaymentStatus));
            return Ok(statuses);
        }
    }
}
