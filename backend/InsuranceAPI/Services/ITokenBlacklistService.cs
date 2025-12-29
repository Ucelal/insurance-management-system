namespace InsuranceAPI.Services
{
    public interface ITokenBlacklistService
    {
        Task<bool> IsTokenBlacklistedAsync(string token);
        Task BlacklistTokenAsync(string token, string userEmail, string reason = "logout");
        Task CleanupExpiredTokensAsync();
        Task<int> GetBlacklistedTokensCountAsync();
        Task InvalidateAllTokensAsync(); // Backend kapatıldığında tüm tokenları iptal et
    }
}
