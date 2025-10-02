using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ISessionService _session;

    public AuthService(IUserRepository users, ISessionService session)
    {
        _users = users;
        _session = session;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var res = await _users.AuthenticateAsync(email.Trim().ToLowerInvariant(), password);
        if (res.Success && res.User is not null)
            await _session.SetCurrentUserIdAsync(res.User.Id);

        return res;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password)
    {
        try
        {
            var user = await _users.CreateAsync(email.Trim().ToLowerInvariant(), password);
            await _session.SetCurrentUserIdAsync(user.Id);
            return AuthResult.Ok(user);
        }
        catch (Exception ex)
        {
            return AuthResult.Error(ex.Message);
        }
    }

    public Task LogoutAsync() => _session.LogoutAsync();
}
