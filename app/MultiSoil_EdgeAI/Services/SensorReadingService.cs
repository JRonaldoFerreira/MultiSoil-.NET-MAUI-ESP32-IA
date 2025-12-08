using System;
using System.Net.Http.Json;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Services;

public class SensorReadingService : ISensorReadingService
{
    private readonly HttpClient _http;
    private readonly ITalhaoRepository _talhaoRepository;

    public SensorReadingService(HttpClient http, ITalhaoRepository talhaoRepository)
    {
        _http = http;
        _talhaoRepository = talhaoRepository;
    }

    public async Task<SensorReadings?> GetReadingsAsync(
        int talhaoId,
        DateTime date,
        TimeSpan inicio,
        TimeSpan fim,
        SensorMetric metrics,
        CancellationToken ct = default)
    {
        // 1) Carrega o talhão para descobrir qual servidor usar
        var talhao = await _talhaoRepository.GetByIdAsync(talhaoId);
        if (talhao is null)
            throw new InvalidOperationException("Talhão não encontrado.");

        var baseUrl = (talhao.ServidorUrl ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Nenhuma URL de servidor ESP32 configurada para este talhão.");

        // Se o usuário não colocou http/https, assume http
        if (!baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "http://" + baseUrl;
        }

        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";

        var fullUri = new Uri(new Uri(baseUrl), "api/readings");

        // ESP32 ainda ignora query string, então não usamos date/inicio/fim na URL
        var dto = await _http.GetFromJsonAsync<ReadingDto>(fullUri, ct);
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
