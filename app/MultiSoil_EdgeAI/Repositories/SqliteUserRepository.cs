using MultiSoil_EdgeAI.Data;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Security;
using SQLite;

namespace MultiSoil_EdgeAI.Repositories;

public class SqliteUserRepository : IUserRepository
{
    private readonly LocalDb _db;
    private SQLiteAsyncConnection Conn => _db.Connection;

    public SqliteUserRepository(LocalDb db) => _db = db;

    public async Task<User?> GetByEmailAsync(string email)
        => await Conn.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();

    public async Task<User> CreateAsync(string email, string password)
    {
        if (await GetByEmailAsync(email) is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        var (hash, salt) = PasswordHasher.HashPassword(password);
        var user = new User { Email = email, PasswordHash = hash, Salt = salt, CreatedAtUtc = DateTime.UtcNow };
        await Conn.InsertAsync(user);
        return user;
    }

    public async Task<AuthResult> AuthenticateAsync(string email, string password)
    {
        var user = await GetByEmailAsync(email);
        if (user is null)
            return AuthResult.Error("Usuário não encontrado.");

        var ok = PasswordHasher.Verify(password, user.PasswordHash, user.Salt);
        if (!ok) return AuthResult.Error("E-mail ou senha inválidos.");

        await UpdateLastLoginAsync(user.Id);
        return AuthResult.Ok(user);
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await Conn.Table<User>().Where(u => u.Id == userId).FirstAsync();
        user.LastLoginUtc = DateTime.UtcNow;
        await Conn.UpdateAsync(user);
    }
}
