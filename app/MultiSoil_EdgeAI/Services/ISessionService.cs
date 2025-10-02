namespace MultiSoil_EdgeAI.Services;

public interface ISessionService
{
    Task<bool> HasActiveSessionAsync();
    Task SetCurrentUserIdAsync(int userId);
    Task<int?> GetCurrentUserIdAsync();
    Task LogoutAsync();
}
