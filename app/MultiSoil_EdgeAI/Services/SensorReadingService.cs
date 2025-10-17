using System.Net.Http.Json;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Services;

public class SensorReadingService : ISensorReadingService
{
    private readonly HttpClient _http;
    public SensorReadingService(HttpClient http) => _http = http;

    public async Task<SensorReadings?> GetReadingsAsync(
        int talhaoId, DateTime date, TimeSpan inicio, TimeSpan fim, SensorMetric metrics, CancellationToken ct = default)
    {
        var url = $"api/readings?talhaoId={talhaoId}&date={date:yyyy-MM-dd}&start={inicio:hh\\:mm}&end={fim:hh\\:mm}&metrics={MetricsToQuery(metrics)}";
        var dto = await _http.GetFromJsonAsync<ReadingDto>(url, ct);
        if (dto is null) return null;

        return new SensorReadings(
            Has(metrics, SensorMetric.N) ? dto.N : null,
            Has(metrics, SensorMetric.P) ? dto.P : null,
            Has(metrics, SensorMetric.K) ? dto.K : null,
            Has(metrics, SensorMetric.PH) ? dto.PH : null,
            Has(metrics, SensorMetric.CE) ? dto.CE : null,
            Has(metrics, SensorMetric.Temp) ? dto.Temp : null,
            Has(metrics, SensorMetric.Umid) ? dto.Umid : null
        );
    }

    private static bool Has(SensorMetric all, SensorMetric m) => (all & m) == m;

    private static string MetricsToQuery(SensorMetric m)
    {
        var parts = new List<string>();
        if (Has(m, SensorMetric.N)) parts.Add("N");
        if (Has(m, SensorMetric.P)) parts.Add("P");
        if (Has(m, SensorMetric.K)) parts.Add("K");
        if (Has(m, SensorMetric.PH)) parts.Add("PH");
        if (Has(m, SensorMetric.CE)) parts.Add("CE");
        if (Has(m, SensorMetric.Temp)) parts.Add("Temp");
        if (Has(m, SensorMetric.Umid)) parts.Add("Umid");
        return string.Join(",", parts);
    }

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
