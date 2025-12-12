using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Interfaces;

public interface IRealtimeSampleRepository
{
    Task AddAsync(RealtimeSample sample);

    Task<IReadOnlyList<RealtimeSample>> GetSamplesAsync(
        int talhaoId,
        DateTime startInclusive,
        DateTime endInclusive);
}
