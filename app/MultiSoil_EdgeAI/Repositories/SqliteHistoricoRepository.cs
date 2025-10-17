// Repositories/SqliteHistoricoRepository.cs
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SQLite;
using MultiSoil_EdgeAI.Data;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Services;

namespace MultiSoil_EdgeAI.Repositories
{
    public class SqliteHistoricoRepository : IHistoricoRepository
    {
        private readonly LocalDb _db;
        private readonly IAuthService _auth;
        private SQLiteAsyncConnection Conn => _db.Connection;
        private bool _ensured;

        public SqliteHistoricoRepository(LocalDb db, IAuthService auth)
        {
            _db = db;
            _auth = auth;
        }

        private async Task EnsureAsync()
        {
            if (_ensured) return;
            await Conn.CreateTableAsync<Historico>();
            await Conn.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_hist_user ON Historicos(UserId)");
            await Conn.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_hist_talhao ON Historicos(TalhaoId)");
            await Conn.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_hist_data ON Historicos(DataColeta)");
            await Conn.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_hist_user_talhao_data ON Historicos(UserId, TalhaoId, DataColeta)");
            _ensured = true;
        }

        private async Task<int> RequireUserIdAsync()
        {
            var uid = await _auth.GetCurrentUserIdAsync();
            if (uid is null || uid.Value == 0)
                throw new UnauthorizedAccessException("Nenhum usuário logado.");
            return uid.Value;
        }

        public async Task<IReadOnlyList<Historico>> GetAllAsync(int talhaoId, DateTime? startDate = null, DateTime? endDate = null)
        {
            await EnsureAsync();
            var uid = await RequireUserIdAsync();

            var q = Conn.Table<Historico>()
                        .Where(h => h.UserId == uid && h.TalhaoId == talhaoId);

            if (startDate is not null) q = q.Where(h => h.DataColeta >= startDate.Value.Date);
            if (endDate is not null) q = q.Where(h => h.DataColeta <= endDate.Value.Date);

            return await q.OrderByDescending(h => h.DataColeta)
                          .ThenByDescending(h => h.HoraInicioMin)
                          .ToListAsync();
        }

        public async Task<Historico?> GetByIdAsync(int id)
        {
            await EnsureAsync();
            var uid = await RequireUserIdAsync();
            return await Conn.Table<Historico>()
                             .Where(h => h.Id == id && h.UserId == uid)
                             .FirstOrDefaultAsync();
        }

        public async Task<Historico> CreateAsync(Historico entity)
        {
            await EnsureAsync();
            var uid = await RequireUserIdAsync();
            entity.UserId = uid;
            entity.CreatedUtc = DateTime.UtcNow;
            await Conn.InsertAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(Historico entity)
        {
            await EnsureAsync();
            var uid = await RequireUserIdAsync();
            if (entity.UserId == 0) entity.UserId = uid;
            entity.UpdatedUtc = DateTime.UtcNow;
            await Conn.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await EnsureAsync();
            var uid = await RequireUserIdAsync();
            await Conn.ExecuteAsync("DELETE FROM Historicos WHERE Id = ? AND UserId = ?", id, uid);
        }
    }
}
