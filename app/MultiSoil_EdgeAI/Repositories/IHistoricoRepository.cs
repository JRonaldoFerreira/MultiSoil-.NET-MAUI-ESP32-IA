// Repositories/IHistoricoRepository.cs
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Interfaces
{
    public interface IHistoricoRepository
    {
        Task<IReadOnlyList<Historico>> GetAllAsync(int talhaoId, DateTime? startDate = null, DateTime? endDate = null);
        Task<Historico?> GetByIdAsync(int id);
        Task<Historico> CreateAsync(Historico entity);
        Task UpdateAsync(Historico entity);
        Task DeleteAsync(int id);
    }
}
