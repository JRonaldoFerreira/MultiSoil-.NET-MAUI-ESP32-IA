using MultiSoil_EdgeAI.Data;
using MultiSoil_EdgeAI.Models;
using SQLite;


namespace MultiSoil_EdgeAI.Repositories;


public class SqliteTalhaoRepository : ITalhaoRepository
{
    private readonly LocalDb _db;
    private SQLiteAsyncConnection Conn => _db.Connection;


    public SqliteTalhaoRepository(LocalDb db) => _db = db;


    public async Task<Talhao?> GetAsync(int id)
    => await Conn.Table<Talhao>().Where(x => x.Id == id).FirstOrDefaultAsync();


    public async Task<List<Talhao>> ListAsync(string? search = null)
    {
        if (string.IsNullOrWhiteSpace(search))
            return await Conn.Table<Talhao>().OrderBy(x => x.Nome).ToListAsync();
        return await Conn.Table<Talhao>()
        .Where(x => x.Nome.Contains(search) || x.Cultura.Contains(search))
        .OrderBy(x => x.Nome)
        .ToListAsync();
    }


    public async Task<int> CreateAsync(Talhao t)
    {
        t.CreatedAtUtc = DateTime.UtcNow;
        await Conn.InsertAsync(t);
        return t.Id;
    }


    public async Task UpdateAsync(Talhao t)
    {
        t.UpdatedAtUtc = DateTime.UtcNow;
        await Conn.UpdateAsync(t);
    }


    public async Task DeleteAsync(int id)
    => await Conn.DeleteAsync<Talhao>(id);
}