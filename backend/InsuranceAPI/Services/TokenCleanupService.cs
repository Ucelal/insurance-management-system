using InsuranceAPI.Services;

namespace InsuranceAPI.Services
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1); // Her saat temizle

        public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Token Cleanup Service started");

            // Shutdown event handler - backend kapatıldığında tüm tokenları iptal et
            AppDomain.CurrentDomain.ProcessExit += async (sender, e) => await InvalidateAllTokensAsync();
            Console.CancelKeyPress += async (sender, e) => await InvalidateAllTokensAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tokenBlacklistService = scope.ServiceProvider.GetRequiredService<ITokenBlacklistService>();
                        
                        _logger.LogInformation("🧹 Starting token cleanup process...");
                        await tokenBlacklistService.CleanupExpiredTokensAsync();
                        
                        var tokenCount = await tokenBlacklistService.GetBlacklistedTokensCountAsync();
                        _logger.LogInformation($"📊 Current blacklisted tokens count: {tokenCount}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error during token cleanup process");
                }

                // Bir sonraki temizleme için bekle
                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            // Service durdurulurken tüm tokenları iptal et
            await InvalidateAllTokensAsync();
            _logger.LogInformation("🛑 Token Cleanup Service stopped");
        }

        private async Task InvalidateAllTokensAsync()
        {
            try
            {
                _logger.LogInformation("🔒 Backend shutting down - Invalidating all active tokens...");
                
                using (var scope = _serviceProvider.CreateScope())
                {
                    var tokenBlacklistService = scope.ServiceProvider.GetRequiredService<ITokenBlacklistService>();
                    await tokenBlacklistService.InvalidateAllTokensAsync();
                }
                
                _logger.LogInformation("✅ All tokens invalidated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error invalidating tokens during shutdown");
            }
        }
    }
}



