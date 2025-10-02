using System.Globalization;

namespace MultiSoil_EdgeAI.Services;

public class SessionService : ISessionService
{
    // chaves seguras
    private const string KEY_USERID = "multisoil_session_userid";
    private const string KEY_EMAIL = "multisoil_session_email";
    private const string KEY_EXPIRY = "multisoil_session_expiry_utc_ticks";
    private const string KEY_TTL = "multisoil_session_ttl_seconds";
    private const string KEY_SLID = "multisoil_session_sliding";

    public TimeSpan DefaultTtl { get; } = TimeSpan.FromSeconds(10); // ajuste se quiser
    public async Task<DateTime?> GetExpiryUtcAsync()
    {
        var ticks = await SecureStorage.GetAsync("multisoil_session_expiry_utc_ticks");
        if (!long.TryParse(ticks, out var t)) return null;
        return new DateTime(t, DateTimeKind.Utc);
    }

    public async Task<TimeSpan?> GetRemainingAsync()
    {
        var expiry = await GetExpiryUtcAsync();
        if (expiry is null) return null;
        var rem = expiry.Value - DateTime.UtcNow;
        return rem < TimeSpan.Zero ? TimeSpan.Zero : rem;
    }

    // opcional (se usa o botão de simulação)
    public async Task ExpireNowAsync()
    {
        await SecureStorage.SetAsync("multisoil_session_expiry_utc_ticks",
            DateTime.UtcNow.AddSeconds(-1).Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public async Task StartAsync(int userId, string email, TimeSpan? ttl = null, bool sliding = true)
    {
        var useTtl = ttl ?? DefaultTtl;
        var expiry = DateTime.UtcNow.Add(useTtl);

        await SecureStorage.SetAsync(KEY_USERID, userId.ToString(CultureInfo.InvariantCulture));
        await SecureStorage.SetAsync(KEY_EMAIL, email);
        await SecureStorage.SetAsync(KEY_TTL, ((int)useTtl.TotalSeconds).ToString(CultureInfo.InvariantCulture));
        await SecureStorage.SetAsync(KEY_SLID, sliding ? "1" : "0");
        await SecureStorage.SetAsync(KEY_EXPIRY, expiry.Ticks.ToString(CultureInfo.InvariantCulture));
    }

    public async Task<bool> HasActiveSessionAsync()
        => await GetCurrentUserIdAsync() is not null && !await IsExpiredAsync();

    public async Task<bool> IsExpiredAsync()
    {
        var ticks = await SecureStorage.GetAsync(KEY_EXPIRY);
        if (!long.TryParse(ticks, out var t)) return true;

        var expiry = new DateTime(t, DateTimeKind.Utc);
        return DateTime.UtcNow >= expiry;
    }

    public async Task TouchAsync()
    {
        // só renova se sliding == true
        var slid = await SecureStorage.GetAsync(KEY_SLID);
        if (slid != "1") return;

        var ttlStr = await SecureStorage.GetAsync(KEY_TTL);
        if (!int.TryParse(ttlStr, out var ttlSec)) return;

        var newExpiry = DateTime.UtcNow.Add(TimeSpan.FromSeconds(ttlSec));
        await SecureStorage.SetAsync(KEY_EXPIRY, newExpiry.Ticks.ToString(CultureInfo.InvariantCulture));
    }

    public async Task<int?> GetCurrentUserIdAsync()
    {
        var s = await SecureStorage.GetAsync(KEY_USERID);
        return int.TryParse(s, out var id) ? id : null;
    }

    public Task<string?> GetCurrentEmailAsync()
        => SecureStorage.GetAsync(KEY_EMAIL);

    public Task LogoutAsync()
    {
        SecureStorage.Remove(KEY_USERID);
        SecureStorage.Remove(KEY_EMAIL);
        SecureStorage.Remove(KEY_TTL);
        SecureStorage.Remove(KEY_SLID);
        SecureStorage.Remove(KEY_EXPIRY);
        return Task.CompletedTask;
    }
}
