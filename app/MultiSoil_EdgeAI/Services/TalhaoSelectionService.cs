// Services/TalhaoSelectionService.cs
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using MultiSoil_EdgeAI.Data;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Services
{
    public class TalhaoSelectionService : ITalhaoSelectionService
    {
        private readonly LocalDb _db;
        private readonly IAuthService _auth;

        public TalhaoSelectionService(LocalDb db, IAuthService auth)
        {
            _db = db;
            _auth = auth;
        }

        private static string Key(int uid) => $"talhao_selected_id_{uid}";

        public async Task<Talhao?> GetSelectedAsync()
        {
            var id = await GetSelectedIdAsync();
            if (id is null) return null;

            var uid = await _auth.GetCurrentUserIdAsync() ?? 0;
            if (uid == 0) return null;

            return await _db.Connection.Table<Talhao>()
                .Where(t => t.Id == id.Value && t.UserId == uid)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetSelectedIdAsync()
        {
            var uid = await _auth.GetCurrentUserIdAsync() ?? 0;
            if (uid == 0) return null;

            var s = Preferences.Get(Key(uid), (string?)null);
            return int.TryParse(s, out var id) ? id : null;
        }

        public async Task SetSelectedAsync(Talhao? talhao)
        {
            var uid = await _auth.GetCurrentUserIdAsync() ?? 0;
            if (uid == 0) return;

            if (talhao is null)
                Preferences.Remove(Key(uid));
            else
                Preferences.Set(Key(uid), talhao.Id.ToString());
        }

        public async Task ClearAsync()
        {
            var uid = await _auth.GetCurrentUserIdAsync() ?? 0;
            if (uid == 0) return;

            Preferences.Remove(Key(uid));
            await Task.CompletedTask;
        }
    }
}
