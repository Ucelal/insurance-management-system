using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;
using InsuranceAPI.Data;
using System.Security.Claims;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly InsuranceDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        
        // Auth service dependency injection
        public AuthController(IAuthService authService, InsuranceDbContext context, IConfiguration configuration, ITokenBlacklistService tokenBlacklistService)
        {
            _authService = authService;
            _context = context;
            _configuration = configuration;
            _tokenBlacklistService = tokenBlacklistService;
        }
        
        // Kullanıcı giriş işlemi - JWT Authentication
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
        
        // Kullanıcı kayıt işlemi (genel) - SADECE ADMIN
        [HttpPost("register")]
        [Authorize(Roles = "admin")]
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
        
        // Customer kayıt işlemi - özel endpoint (PUBLIC)
        [HttpPost("register/customer")]
        public async Task<ActionResult<AuthResponseDto>> RegisterCustomer([FromBody] CustomerRegisterDto customerRegisterDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.RegisterCustomerAsync(customerRegisterDto);
            
            if (result == null)
            {
                return BadRequest(new { message = "Email or TC No already exists" });
            }
            
            return CreatedAtAction(nameof(Login), result);
        }
        
        // Agent kayıt işlemi - özel endpoint (PUBLIC)
        [HttpPost("register/agent")]
        public async Task<ActionResult<AuthResponseDto>> RegisterAgent([FromBody] AgentRegisterDto agentRegisterDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.RegisterAgentAsync(agentRegisterDto);
            
            if (result == null)
            {
                return BadRequest(new { message = "Email already exists" });
            }
            
            return CreatedAtAction(nameof(Login), result);
        }
        
        // Admin kayıt işlemi - özel endpoint (ADMIN ONLY)
        [HttpPost("register/admin")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<AuthResponseDto>> RegisterAdmin([FromBody] AdminRegisterDto adminRegisterDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.RegisterAdminAsync(adminRegisterDto);
            
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
        
        // Debug endpoint - admin user'ın role bilgisini kontrol et
        [HttpGet("debug/admin")]
        public async Task<ActionResult> DebugAdminUser()
        {
            var adminUser = await _context.Users
                .Where(u => u.Email == "admin@insurance.com")
                .Select(u => new { u.UserId, u.Name, u.Email, u.Role, u.CreatedAt })
                .FirstOrDefaultAsync();
                
            if (adminUser == null)
            {
                return NotFound(new { message = "Admin user not found" });
            }
            
            return Ok(new { 
                message = "Admin user found", 
                user = adminUser,
                allUsers = await _context.Users.Select(u => new { u.UserId, u.Name, u.Email, u.Role }).ToListAsync()
            });
        }
        
        // Test endpoint - JWT authentication aktif
        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return Ok("API çalışıyor! JWT authentication aktif.");
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
        
        // Debug endpoint - hash test
        [HttpPost("debug-hash")]
        [AllowAnonymous]
        public IActionResult DebugHash([FromBody] string password)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
            
            return Ok(new { 
                Password = password, 
                Hash = hash, 
                IsValid = isValid,
                TestHash = "$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj4J/8j8qKqC",
                TestValid = BCrypt.Net.BCrypt.Verify(password, "$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj4J/8j8qKqC")
            });
        }
        
        // Debug endpoint - login test
        [HttpPost("debug-login")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugLogin([FromBody] LoginDto loginDto)
        {
            try
            {
                // Email kontrolü
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
                if (user == null)
                {
                    return Ok(new { Error = "User not found", Email = loginDto.Email });
                }

                // Password hash kontrolü
                var passwordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
                
                return Ok(new { 
                    UserFound = true,
                    Email = user.Email,
                    Role = user.Role,
                    PasswordHash = user.PasswordHash,
                    PasswordValid = passwordValid,
                    InputPassword = loginDto.Password
                });
            }
            catch (Exception ex)
            {
                return Ok(new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }
        
        // Admin paneli - Tüm kullanıcıları getir
        [HttpGet("admin/users")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Customer)
                    .OrderBy(u => u.CreatedAt)
                    .Select(u => new UserDto
                    {
                        Id = u.UserId,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role,
                        CreatedAt = u.CreatedAt,
                        Customer = u.Customer != null ? new CustomerDto
                        {
                            CustomerId = u.Customer.CustomerId,
                            UserId = u.Customer.UserId,
                            IdNo = u.Customer.IdNo,
                            Address = u.Customer.Address,
                            Phone = u.Customer.Phone
                        } : null
                    })
                    .ToListAsync();
                
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Kullanıcılar getirilemedi", error = ex.Message });
            }
        }
        
        // Admin paneli - Kullanıcı sil
        [HttpDelete("admin/users/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }
                
                // Admin kendini silemesin
                if (user.Role.ToLower() == "admin")
                {
                    return BadRequest(new { message = "Admin kullanıcısı silinemez" });
                }
                
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                
                return Ok(new { message = "Kullanıcı başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Kullanıcı silinemedi", error = ex.Message });
            }
        }
        
        // Admin paneli - Kullanıcı rolünü güncelle
        [HttpPut("admin/users/{id}/role")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto updateRoleDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }
                
                // Admin rolü değiştirilemesin
                if (user.Role.ToLower() == "admin")
                {
                    return BadRequest(new { message = "Admin kullanıcısının rolü değiştirilemez" });
                }
                
                user.Role = updateRoleDto.Role;
                await _context.SaveChangesAsync();
                
                return Ok(new { message = "Kullanıcı rolü güncellendi", user = new { Id = user.UserId, Name = user.Name, Role = user.Role } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Rol güncellenemedi", error = ex.Message });
            }
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

        // Debug endpoint - hash oluştur
        [HttpPost("debug-create-hash")]
        [AllowAnonymous]
        public IActionResult DebugCreateHash([FromBody] string password)
        {
            try
            {
                var hash = BCrypt.Net.BCrypt.HashPassword(password);
                var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
                
                return Ok(new { 
                    Password = password, 
                    Hash = hash, 
                    IsValid = isValid
                });
            }
            catch (Exception ex)
            {
                return Ok(new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        // Debug endpoint - JWT token'ı test et
        [HttpGet("debug")]
        [Authorize]
        public ActionResult<object> DebugToken()
        {
            var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
            var identity = User.Identity;
            
            return Ok(new
            {
                IsAuthenticated = identity?.IsAuthenticated,
                AuthenticationType = identity?.AuthenticationType,
                Name = identity?.Name,
                Claims = claims,
                Roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
            });
        }
        
        // Logout endpoint - Token'ı blacklist'e ekle
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return BadRequest(new { message = "Invalid authorization header" });
                }
                
                var token = authHeader.Substring("Bearer ".Length);
                
                // Token'ı blacklist'e ekle
                await _tokenBlacklistService.BlacklistTokenAsync(token, userEmail ?? "unknown", "logout");
                
                Console.WriteLine($"✅ User logged out successfully: {userEmail}");
                
                return Ok(new { 
                    message = "Logout successful",
                    userEmail = userEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Logout error: {ex.Message}");
                return StatusCode(500, new { message = "Logout failed", error = ex.Message });
            }
        }

        // Admin endpoint - Token cleanup
        [HttpPost("admin/cleanup-tokens")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> CleanupTokens()
        {
            try
            {
                await _tokenBlacklistService.CleanupExpiredTokensAsync();
                var tokenCount = await _tokenBlacklistService.GetBlacklistedTokensCountAsync();
                
                return Ok(new { 
                    message = "Token cleanup completed successfully",
                    remainingTokens = tokenCount,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Token cleanup error: {ex.Message}");
                return StatusCode(500, new { message = "Token cleanup failed", error = ex.Message });
            }
        }

        // Admin endpoint - Token statistics
        [HttpGet("admin/token-stats")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> GetTokenStats()
        {
            try
            {
                var totalTokens = await _tokenBlacklistService.GetBlacklistedTokensCountAsync();
                var cutoffTime = DateTime.UtcNow.AddHours(-4);
                
                return Ok(new { 
                    totalBlacklistedTokens = totalTokens,
                    cleanupCutoffTime = cutoffTime,
                    tokensOlderThan4Hours = "Will be cleaned up automatically",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Token stats error: {ex.Message}");
                return StatusCode(500, new { message = "Failed to get token stats", error = ex.Message });
            }
        }

        // Geçici endpoint - Admin şifresini güncelle
        [HttpPost("fix-admin-password")]
        [AllowAnonymous]
        public async Task<ActionResult> FixAdminPassword()
        {
            try
            {
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@insurance.com");
                if (adminUser == null)
                {
                    return NotFound("Admin user not found");
                }
                
                // Doğru hash ile güncelle
                adminUser.PasswordHash = "$2a$11$q4GekRA437jGViXGEMUInOXsoLMRC80R.aKYMdJVuqsXe0s2Lg.aO";
                await _context.SaveChangesAsync();
                
                // Test et
                var isValid = BCrypt.Net.BCrypt.Verify("Admin123!", adminUser.PasswordHash);
                
                return Ok(new { 
                    message = "Admin password updated successfully",
                    email = adminUser.Email,
                    newHash = adminUser.PasswordHash,
                    testPassword = "Admin123!",
                    isValid = isValid
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
} 