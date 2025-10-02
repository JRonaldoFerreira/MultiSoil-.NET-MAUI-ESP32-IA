namespace MultiSoil_EdgeAI.Services;

public interface ISessionService
{
    TimeSpan DefaultTtl { get; }

    Task StartAsync(int userId, string email, TimeSpan? ttl = null, bool sliding = true);
    Task<bool> HasActiveSessionAsync();
    Task<bool> IsExpiredAsync();
    Task TouchAsync();
    Task<int?> GetCurrentUserIdAsync();
    Task<string?> GetCurrentEmailAsync();
    Task<DateTime?> GetExpiryUtcAsync();
    Task<TimeSpan?> GetRemainingAsync();
    Task LogoutAsync();

    // opcional, se você usa o botão "Simular Expiração"
    Task ExpireNowAsync();
}
