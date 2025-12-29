using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;
using System.Security.Claims;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // Kullanıcı profil bilgilerini getir
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
                }

                var profile = await _profileService.GetUserProfileAsync(userId.Value);
                if (profile == null)
                {
                    return NotFound(new { message = "Profil bulunamadı" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProfileController.GetMyProfile error: {ex.Message}");
                return StatusCode(500, new { message = "Profil bilgileri getirilemedi", error = ex.Message });
            }
        }

        // Kullanıcı profil bilgilerini güncelle (ad, email)
        [HttpPut("me")]
        public async Task<ActionResult<ProfileUpdateResponseDto>> UpdateMyProfile([FromBody] UpdateProfileDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Geçersiz veri", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
                }

                var result = await _profileService.UpdateUserProfileAsync(userId.Value, updateDto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProfileController.UpdateMyProfile error: {ex.Message}");
                return StatusCode(500, new { message = "Profil güncellenemedi", error = ex.Message });
            }
        }

        // Şifre değiştir
        [HttpPut("me/password")]
        public async Task<ActionResult<ProfileUpdateResponseDto>> ChangeMyPassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Geçersiz veri", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
                }

                var result = await _profileService.ChangePasswordAsync(userId.Value, changePasswordDto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProfileController.ChangeMyPassword error: {ex.Message}");
                return StatusCode(500, new { message = "Şifre değiştirilemedi", error = ex.Message });
            }
        }

        // Müşteri profil bilgilerini güncelle (adres, telefon)
        [HttpPut("me/customer")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<ProfileUpdateResponseDto>> UpdateMyCustomerProfile([FromBody] UpdateCustomerProfileDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Geçersiz veri", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
                }

                var result = await _profileService.UpdateCustomerProfileAsync(userId.Value, updateDto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProfileController.UpdateMyCustomerProfile error: {ex.Message}");
                return StatusCode(500, new { message = "Müşteri bilgileri güncellenemedi", error = ex.Message });
            }
        }

        // Agent profil bilgilerini güncelle (adres, telefon)
        [HttpPut("me/agent")]
        [Authorize(Roles = "agent")]
        public async Task<ActionResult<ProfileUpdateResponseDto>> UpdateMyAgentProfile([FromBody] UpdateAgentProfileDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Geçersiz veri", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
                }

                var result = await _profileService.UpdateAgentProfileAsync(userId.Value, updateDto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProfileController.UpdateMyAgentProfile error: {ex.Message}");
                return StatusCode(500, new { message = "Agent bilgileri güncellenemedi", error = ex.Message });
            }
        }

        // Mevcut kullanıcının ID'sini al
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
