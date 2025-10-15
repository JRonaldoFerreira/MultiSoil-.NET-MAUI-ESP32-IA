using SQLite;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Data;

public sealed class LocalDb
{
    public const string FileName = "multisoil_edgeai.db3";
    private readonly SQLiteAsyncConnection _conn;

    public LocalDb()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, FileName);
        _conn = new SQLiteAsyncConnection(path);
    }

    public SQLiteAsyncConnection Connection => _conn;

    public async Task InitializeAsync()
    {
        await _conn.CreateTableAsync<User>();
        await _conn.CreateTableAsync<Models.Talhao>();
        await SeedAsync();
    }

    // Usuário de demonstração (para first run)
    private async Task SeedAsync()
    {
        var count = await _conn.Table<User>().CountAsync();
        if (count == 0)
        {
            var (hash, salt) = Security.PasswordHasher.HashPassword("123456");
            var user = new User { Email = "ana@example.com", PasswordHash = hash, Salt = salt };
            await _conn.InsertAsync(user);
        }
    }
}
