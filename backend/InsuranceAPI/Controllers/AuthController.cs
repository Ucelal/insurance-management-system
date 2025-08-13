using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;
using InsuranceAPI.Data;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly InsuranceDbContext _context;
        
        private readonly IConfiguration _configuration;
        
        // Auth service dependency injection
        public AuthController(IAuthService authService, InsuranceDbContext context, IConfiguration configuration)
        {
            _authService = authService;
            _context = context;
            _configuration = configuration;
        }
        
        // Kullanıcı giriş işlemi
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
        
        // Kullanıcı kayıt işlemi
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
        
        // Token doğrulama işlemi
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
        
        // Mevcut kullanıcı bilgilerini getir
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
        
        // API test endpoint
        [HttpGet("test")]
        public ActionResult Test()
        {
            return Ok(new { message = "API is working!", timestamp = DateTime.UtcNow });
        }
        
        // Şifre debug endpoint
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
        
        // Register debug endpoint
        [HttpPost("debug-register")]
        public async Task<ActionResult> DebugRegister([FromBody] RegisterDto registerDto)
        {
            return Ok(new { 
                receivedData = registerDto,
                modelStateErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(),
                isValid = ModelState.IsValid,
                emailExists = await _authService.GetUserByEmailAsync(registerDto.Email) != null
            });
        }
        
        // Veritabanı bağlantı test endpoint
        [HttpGet("debug-db")]
        public async Task<ActionResult> DebugDatabase()
        {
            try
            {
                var userCount = await _context.Users.CountAsync();
                var allEmails = await _context.Users.Select(u => u.Email).ToListAsync();
                
                return Ok(new { 
                    success = true,
                    userCount = userCount,
                    allEmails = allEmails,
                    connectionString = _configuration.GetConnectionString("DefaultConnection")
                });
            }
            catch (Exception ex)
            {
                return Ok(new { 
                    success = false,
                    error = ex.Message,
                    connectionString = _configuration.GetConnectionString("DefaultConnection")
                });
            }
        }
    }
} 