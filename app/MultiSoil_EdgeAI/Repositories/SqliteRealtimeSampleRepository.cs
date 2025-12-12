using SQLite;
using MultiSoil_EdgeAI.Data;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Services;

namespace MultiSoil_EdgeAI.Repositories;

public class SqliteRealtimeSampleRepository : IRealtimeSampleRepository
{
    private readonly LocalDb _db;
    private readonly IAuthService _auth;
    private SQLiteAsyncConnection Conn => _db.Connection;
    private bool _ensured;

    public SqliteRealtimeSampleRepository(LocalDb db, IAuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    private async Task EnsureAsync()
    {
        if (_ensured) return;

        await Conn.CreateTableAsync<RealtimeSample>();
        await Conn.ExecuteAsync(
            "CREATE INDEX IF NOT EXISTS idx_rt_user_talhao_time ON RealtimeSamples(UserId, TalhaoId, Timestamp)");
        _ensured = true;
    }

    private async Task<int> RequireUserIdAsync()
    {
        var uid = await _auth.GetCurrentUserIdAsync();
        if (uid is null || uid.Value == 0)
            throw new UnauthorizedAccessException("Nenhum usuário logado.");
        return uid.Value;
    }

    public async Task AddAsync(RealtimeSample sample)
    {
        await EnsureAsync();
        var uid = await RequireUserIdAsync();
        sample.UserId = uid;

        if (sample.Timestamp == default)
            sample.Timestamp = DateTime.Now;

        await Conn.InsertAsync(sample);
    }

    public async Task<IReadOnlyList<RealtimeSample>> GetSamplesAsync(
        int talhaoId,
        DateTime startInclusive,
        DateTime endInclusive)
    {
        await EnsureAsync();
        var uid = await RequireUserIdAsync();

        var query = Conn.Table<RealtimeSample>()
                        .Where(s => s.UserId == uid &&
                                    s.TalhaoId == talhaoId &&
                                    s.Timestamp >= startInclusive &&
                                    s.Timestamp <= endInclusive)
                        .OrderBy(s => s.Timestamp);

        return await query.ToListAsync();
    }
}
