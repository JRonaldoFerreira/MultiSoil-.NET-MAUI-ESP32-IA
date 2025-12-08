// Interfaces/ISensorReadingService.cs
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Interfaces;

public interface ISensorReadingService
{
    Task<SensorReadings?> GetReadingsAsync(
        int talhaoId,
        DateTime date,
        TimeSpan inicio,
        TimeSpan fim,
        SensorMetric metrics,
        CancellationToken ct = default);
}
