// Repositories/SqliteTalhaoRepository.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SQLite;
using MultiSoil_EdgeAI.Data;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Services; // IAuthService

namespace MultiSoil_EdgeAI.Repositories
{
    public class SqliteTalhaoRepository : ITalhaoRepository
    {
        private readonly LocalDb _db;
        private readonly IAuthService _auth;
        private SQLiteAsyncConnection Conn => _db.Connection;

        public SqliteTalhaoRepository(LocalDb db, IAuthService auth)
        {
            _db = db;
            _auth = auth;
        }

        private bool _ensured;
        private async Task EnsureAsync()
        {
            if (_ensured) return;
            await Conn.CreateTableAsync<Talhao>();
            await EnsureUserColumnsAsync();
            _ensured = true;
        }

        private class TableInfo
        {
            public int cid { get; set; }
            public string name { get; set; } = "";
            public string type { get; set; } = "";
            public int notnull { get; set; }
            public string dflt_value { get; set; } = "";
            public int pk { get; set; }
        }

        private async Task EnsureUserColumnsAsync()
        {
            // Garante coluna UserId (INTEGER NOT NULL DEFAULT 0) e �ndices
            var cols = await Conn.QueryAsync<TableInfo>("PRAGMA table_info(Talhoes)");
            if (!cols.Any(c => c.name == "UserId"))
                await Conn.ExecuteAsync("ALTER TABLE Talhoes ADD COLUMN UserId INTEGER NOT NULL DEFAULT 0");

            await Conn.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_talhoes_user ON Talhoes(UserId)");
            // Opcional: �nico por usu�rio+nome
            await Conn.ExecuteAsync("CREATE UNIQUE INDEX IF NOT EXISTS idx_talhoes_user_nome ON Talhoes(UserId, Nome)");

            // (DEV) Backfill de registros antigos (UserId=0) para o usu�rio atual, se houver
            var uid = await _auth.GetCurrentUserIdAsync() ?? 0;
            if (uid != 0)
                await Conn.ExecuteAsync("UPDATE Talhoes SET UserId = ? WHERE IFNULL(UserId,0) = 0", uid);
        }

        private async Task<int> RequireUserIdAsync()
        {
            var uid = await _auth.GetCurrentUserIdAsync();
            if (uid is null || uid.Value == 0)
                throw new UnauthorizedAccessException("Nenhum usu�rio logado.");
            return uid.Value;
        }

        public async Task<IReadOnlyList<Talhao>> GetAllAsync()
        {
            await EnsureAsync();
            var uid = await RequireUserIdAsync();
            var list = await Conn.Table<Talhao>()
                                 .Where(t => t.UserId == uid)
                                 .OrderBy(t => t.Nome)
                                 .ToListAsync();
            return list;
        }

        public async Task<Talhao?> GetByIdAsync(int id)
        {
            await EnsureAsync();
            var uid = await RequireUserIdAsync();
            return await Conn.Table<Talhao>()
                             .Where(t => t.Id == id && t.UserId == uid)
                             .FirstOrDefaultAsync();
        }

        public async Task<Talhao> CreateAsync(Talhao entity)
        {
            await EnsureAsync();
            var uid = await RequireUserIdAsync();
            entity.UserId = uid;
            entity.CreatedUtc = DateTime.UtcNow;
            await Conn.InsertAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(Talhao entity)
        {
            await EnsureAsync();
            var uid = await RequireUserIdAsync();

            var owned = await Conn.Table<Talhao>()
                                  .Where(t => t.Id == entity.Id && t.UserId == uid)
                                  .CountAsync();
            if (owned == 0)
                throw new UnauthorizedAccessException("Talh�o n�o pertence ao usu�rio atual.");

            entity.UserId = uid;
            entity.UpdatedUtc = DateTime.UtcNow;
            await Conn.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await EnsureAsync();
            var uid = await RequireUserIdAsync();
            await Conn.ExecuteAsync("DELETE FROM Talhoes WHERE Id = ? AND UserId = ?", id, uid);
        }
    }
}
