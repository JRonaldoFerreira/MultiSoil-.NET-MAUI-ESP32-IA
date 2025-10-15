using MultiSoil_EdgeAI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiSoil_EdgeAI.Interfaces
{
    public interface ITalhaoRepository
    {
        Task<IReadOnlyList<Talhao>> GetAllAsync();
        Task<Talhao?> GetByIdAsync(int id);
        Task<Talhao> CreateAsync(Talhao entity);
        Task UpdateAsync(Talhao entity);
        Task DeleteAsync(int id);
    }
}
