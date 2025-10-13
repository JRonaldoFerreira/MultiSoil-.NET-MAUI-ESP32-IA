using MultiSoil_EdgeAI.Models;


namespace MultiSoil_EdgeAI.Services;


public interface ITalhaoService
{
    Task<List<Talhao>> ListAsync(string? search = null);
    Task<Talhao?> GetAsync(int id);
    Task<int> CreateAsync(Talhao t);
    Task UpdateAsync(Talhao t);
    Task DeleteAsync(int id);
    int? GetActiveId();
    Task SetActiveAsync(int? id);
}