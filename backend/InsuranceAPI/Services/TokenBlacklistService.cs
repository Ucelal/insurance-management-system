using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.Models;
using System.IdentityModel.Tokens.Jwt;

namespace InsuranceAPI.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly InsuranceDbContext _context;

        public TokenBlacklistService(InsuranceDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return true;

            var blacklistedToken = await _context.TokenBlacklist
                .FirstOrDefaultAsync(t => t.Token == token);

            return blacklistedToken != null;
        }

        public async Task BlacklistTokenAsync(string token, string userEmail, string reason = "logout")
        {
            if (string.IsNullOrEmpty(token))
                return;

            try
            {
                // Parse JWT token to get expiration
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var expiration = jwtToken.ValidTo;

                // Check if token is already blacklisted
                var existingToken = await _context.TokenBlacklist
                    .FirstOrDefaultAsync(t => t.Token == token);

                if (existingToken == null)
                {
                    var blacklistedToken = new TokenBlacklist
                    {
                        Token = token,
                        BlacklistedAt = DateTime.UtcNow,
                        ExpiresAt = expiration,
                        UserEmail = userEmail,
                        Reason = reason
                    };

                    _context.TokenBlacklist.Add(blacklistedToken);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ Token blacklisted for user: {userEmail}, reason: {reason}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error blacklisting token: {ex.Message}");
                // Don't throw - we don't want logout to fail if blacklisting fails
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                // 4 saatten eski token'ları temizle
                var cutoffTime = DateTime.UtcNow.AddHours(-4);
                var expiredTokens = await _context.TokenBlacklist
                    .Where(t => t.BlacklistedAt < cutoffTime)
                    .ToListAsync();

                if (expiredTokens.Any())
                {
                    _context.TokenBlacklist.RemoveRange(expiredTokens);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"🧹 Cleaned up {expiredTokens.Count} tokens older than 4 hours (cutoff: {cutoffTime:yyyy-MM-dd HH:mm:ss} UTC)");
                }
                else
                {
                    Console.WriteLine($"✅ No tokens older than 4 hours found for cleanup");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cleaning up expired tokens: {ex.Message}");
            }
        }

        public async Task<int> GetBlacklistedTokensCountAsync()
        {
            return await _context.TokenBlacklist.CountAsync();
        }

        public async Task InvalidateAllTokensAsync()
        {
            try
            {
                // Tüm aktif token'ları iptal et (backend kapatıldığında)
                var allTokens = await _context.TokenBlacklist.ToListAsync();
                
                if (allTokens.Any())
                {
                    // Tüm token'ları "server_shutdown" sebebiyle iptal et
                    foreach (var token in allTokens)
                    {
                        token.Reason = "server_shutdown";
                        token.BlacklistedAt = DateTime.UtcNow;
                    }
                    
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"🔒 Invalidated {allTokens.Count} tokens due to server shutdown");
                }
                else
                {
                    Console.WriteLine($"✅ No active tokens to invalidate");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error invalidating all tokens: {ex.Message}");
            }
        }
    }
}
