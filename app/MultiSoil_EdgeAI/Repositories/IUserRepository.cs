using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(string email, string password);
    Task<AuthResult> AuthenticateAsync(string email, string password);
    Task UpdateLastLoginAsync(int userId);
}
