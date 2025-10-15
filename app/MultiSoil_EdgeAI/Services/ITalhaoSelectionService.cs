using MultiSoil_EdgeAI.Models;
using System.Threading.Tasks;

namespace MultiSoil_EdgeAI.Services
{
    public interface ITalhaoSelectionService
    {
        Task<Talhao?> GetSelectedAsync();
        Task<int?> GetSelectedIdAsync();
        Task SetSelectedAsync(Talhao? talhao);
        Task ClearAsync();
    }
}
