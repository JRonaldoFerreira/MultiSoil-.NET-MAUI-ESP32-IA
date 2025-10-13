using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Repositories;


namespace MultiSoil_EdgeAI.Services;


public class TalhaoService : ITalhaoService
{
    private readonly ITalhaoRepository _repo;
    private const string PrefKey = "active_talhao_id";


    public TalhaoService(ITalhaoRepository repo) => _repo = repo;


    public Task<List<Talhao>> ListAsync(string? search = null) => _repo.ListAsync(search);
    public Task<Talhao?> GetAsync(int id) => _repo.GetAsync(id);
    public Task<int> CreateAsync(Talhao t) => _repo.CreateAsync(t);
    public Task UpdateAsync(Talhao t) => _repo.UpdateAsync(t);
    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);


    public int? GetActiveId()
    {
        if (Preferences.Default.ContainsKey(PrefKey))
        {
            var v = Preferences.Default.Get(PrefKey, -1);
            return v > 0 ? v : null;
        }
        return null;
    }


    public Task SetActiveAsync(int? id)
    {
        if (id is null) Preferences.Default.Remove(PrefKey);
        else Preferences.Default.Set(PrefKey, id.Value);
        return Task.CompletedTask;
    }
}

