using Microsoft.AspNetCore.Mvc;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.LoginAsync(loginDto);
            
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
            
            return Ok(result);
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.RegisterAsync(registerDto);
            
            if (result == null)
            {
                return BadRequest(new { message = "Email already exists" });
            }
            
            return CreatedAtAction(nameof(Login), result);
        }
        
        [HttpPost("validate")]
        public async Task<ActionResult> ValidateToken([FromBody] string token)
        {
            var isValid = await _authService.ValidateTokenAsync(token);
            
            if (!isValid)
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            
            return Ok(new { message = "Token is valid" });
        }
        
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "No token provided" });
            }
            
            var token = authHeader.Substring("Bearer ".Length);
            var user = await _authService.GetUserFromTokenAsync(token);
            
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            
            return Ok(user);
        }
        
        [HttpGet("test")]
        public ActionResult Test()
        {
            return Ok(new { message = "API is working!", timestamp = DateTime.UtcNow });
        }
        
        [HttpGet("debug-password")]
        public async Task<ActionResult> DebugPassword()
        {
            var user = await _authService.GetUserByEmailAsync("admin@insurance.com");
            if (user == null)
            {
                return NotFound("User not found");
            }
            
            var testPassword = "Admin123!";
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(testPassword, user.PasswordHash);
            
            return Ok(new { 
                userEmail = user.Email,
                passwordHash = user.PasswordHash,
                testPassword = testPassword,
                isPasswordValid = isPasswordValid
            });
        }
    }
} 