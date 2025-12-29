using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using InsuranceAPI.Models;
using SecurityClaim = System.Security.Claims.Claim;

namespace InsuranceAPI.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        
        public JwtService(IConfiguration configuration, ITokenBlacklistService tokenBlacklistService)
        {
            _configuration = configuration;
            _tokenBlacklistService = tokenBlacklistService;
        }
        
        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "your-super-secret-key-with-at-least-32-characters");
            
            var claims = new List<SecurityClaim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };
            
            // Role'e göre token süresini belirle
            var tokenExpirationHours = user.Role.ToLower() switch
            {
                "admin" => 4,    // Admin için 4 saat
                "agent" => 4,    // Agent için 4 saat
                "customer" => 1, // Customer için 1 saat
                _ => 1           // Varsayılan 1 saat
            };
            
            // Debug log'ları
            Console.WriteLine($"🔐 JWT Token generated for user: {user.Email} (Role: {user.Role})");
            Console.WriteLine($"⏰ Token expiration: {tokenExpirationHours} hours from now");
            Console.WriteLine($"📅 Token expires at: {DateTime.UtcNow.AddHours(tokenExpirationHours):yyyy-MM-dd HH:mm:ss} UTC");
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(tokenExpirationHours),
                Issuer = _configuration["Jwt:Issuer"] ?? "InsuranceAPI",
                Audience = _configuration["Jwt:Audience"] ?? "InsuranceClient",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            Console.WriteLine($"✅ JWT Token generated successfully. Length: {tokenString.Length} characters");
            
            return tokenString;
        }
        
        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
        
        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            // Check if token is blacklisted
            if (await _tokenBlacklistService.IsTokenBlacklistedAsync(token))
            {
                Console.WriteLine($"🚫 Token is blacklisted: {token.Substring(0, Math.Min(20, token.Length))}...");
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "your-super-secret-key-with-at-least-32-characters");
            
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? "InsuranceAPI",
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"] ?? "InsuranceClient",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                
                Console.WriteLine($"✅ Token validated successfully for user: {principal?.FindFirst(ClaimTypes.Email)?.Value}");
                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Token validation failed: {ex.Message}");
                return null;
            }
        }

        // Synchronous version for backward compatibility
        public ClaimsPrincipal? ValidateToken(string token)
        {
            // This is not recommended for production - use ValidateTokenAsync instead
            return ValidateTokenAsync(token).GetAwaiter().GetResult();
        }
    }
} 