using System.Net.Http.Json;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Services;

public class SensorReadingService : ISensorReadingService
{
    private readonly HttpClient _http;

    public SensorReadingService(HttpClient http) => _http = http;

    public async Task<SensorReadings?> GetReadingsAsync(
        int talhaoId,
        DateTime date,
        TimeSpan inicio,
        TimeSpan fim,
        SensorMetric metrics,
        CancellationToken ct = default)
    {
        // ESP32 IGNORA query string, então vamos direto em /api/readings.
        // Mantive parâmetros no método para continuar compatível com o resto do app.
        const string url = "api/readings";

        var dto = await _http.GetFromJsonAsync<ReadingDto>(url, ct);
        if (dto is null)
            return null;

        // Aplica o filtro de métricas (o ViewModel espera isso)
        double? n = metrics.HasFlag(SensorMetric.N) ? dto.N : null;
        double? p = metrics.HasFlag(SensorMetric.P) ? dto.P : null;
        double? k = metrics.HasFlag(SensorMetric.K) ? dto.K : null;
        double? ph = metrics.HasFlag(SensorMetric.PH) ? dto.PH : null;
        double? ce = metrics.HasFlag(SensorMetric.CE) ? dto.CE : null;
        double? t = metrics.HasFlag(SensorMetric.Temp) ? dto.Temp : null;
        double? u = metrics.HasFlag(SensorMetric.Umid) ? dto.Umid : null;

        return new SensorReadings(n, p, k, ph, ce, t, u);
    }

    // DTO que casa com o JSON do ESP32:
    // { "N":..., "P":..., "K":..., "PH":..., "CE":..., "Temp":..., "Umid":... }
    private sealed class ReadingDto
    {
        public double? N { get; set; }
        public double? P { get; set; }
        public double? K { get; set; }
        public double? PH { get; set; }
        public double? CE { get; set; }
        public double? Temp { get; set; }
        public double? Umid { get; set; }
    }
}
