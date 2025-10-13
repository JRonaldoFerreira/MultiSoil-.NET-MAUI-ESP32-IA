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


    // ADD: expor a Connection
    public SQLiteAsyncConnection Connection => _conn;


    // ADD: inicialização (criação de tabelas)
    public async Task InitializeAsync()
    {
        await _conn.CreateTableAsync<User>();
        await _conn.CreateTableAsync<Talhao>(); // <-- NOVO
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

        // ADD: seed opcional para Talhão (só se tabela estiver vazia)
        var tCount = await _conn.Table<Talhao>().CountAsync();
        if (tCount == 0)
        {
            await _conn.InsertAsync(new Talhao
            {
                Nome = "Campo A",
                Cultura = "Milho",
                AreaHa = 2.5,
                Latitude = -23.56,
                Longitude = -46.64,
                DataPlantio = DateTime.UtcNow.AddDays(-30),
                Status = "Ativo"
            });
        }
    }
}
