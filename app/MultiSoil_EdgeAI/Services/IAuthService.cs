using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<int?> GetCurrentUserIdAsync();
    Task<AuthResult> RegisterAsync(string email, string password);
    Task LogoutAsync();
}
