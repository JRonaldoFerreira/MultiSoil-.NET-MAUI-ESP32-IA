namespace MultiSoil_EdgeAI.Services;

public class SessionService : ISessionService
{
    private const string Key = "multisoil_session_userid";

    public async Task<bool> HasActiveSessionAsync()
        => await GetCurrentUserIdAsync() is not null;

    public async Task SetCurrentUserIdAsync(int userId)
        => await SecureStorage.SetAsync(Key, userId.ToString());

    public async Task<int?> GetCurrentUserIdAsync()
    {
        var s = await SecureStorage.GetAsync(Key);
        return int.TryParse(s, out var id) ? id : null;
    }

    public Task LogoutAsync()
    {
        SecureStorage.Remove(Key);
        return Task.CompletedTask;
    }
}
