using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Repositories
{
  
    public interface ITalhaoRepository
    {
        Task<Talhao?> GetAsync(int id);
        Task<List<Talhao>> ListAsync(string? search = null);
        Task<int> CreateAsync(Talhao t);
        Task UpdateAsync(Talhao t);
        Task DeleteAsync(int id);
    }
}
