// Services/SimpleMetricPredictionService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Services;

public class SimpleMetricPredictionService : IMetricPredictionService
{
    private readonly IRealtimeSampleRepository _samplesRepo;

    public SimpleMetricPredictionService(IRealtimeSampleRepository samplesRepo)
    {
        _samplesRepo = samplesRepo;
    }

    public async Task<MetricPredictionResult?> PredictAsync(int talhaoId, DateTime targetTime)
    {
        // ================================
        // JANELA DE TEMPO: ÚLTIMA 1 HORA
        // ================================
        var end = targetTime;           // normalmente: agora
        var start = end.AddHours(-1);   // de agora - 1h até agora

        var samples = await _samplesRepo.GetSamplesAsync(talhaoId, start, end);
        if (samples.Count == 0)
            return null;

        var ordered = samples.OrderBy(s => s.Timestamp).ToList();
        var firstTime = ordered.First().Timestamp;

        double ToMinutes(DateTime t) => (t - firstTime).TotalMinutes;

        // Helper de regressão linear y = a + b*x
        static double? PredictLinear(List<(double x, double y)> points, double xTarget)
        {
            var n = points.Count;
            if (n == 0) return null;
            if (n == 1) return points[0].y;

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            foreach (var p in points)
            {
                sumX += p.x;
                sumY += p.y;
                sumXY += p.x * p.y;
                sumX2 += p.x * p.x;
            }

            var denom = n * sumX2 - sumX * sumX;
            if (Math.Abs(denom) < 1e-8)
            {
                // x quase constantes -> não dá pra ajustar bem -> usa média
                return sumY / n;
            }

            var b = (n * sumXY - sumX * sumY) / denom;
            var a = (sumY - b * sumX) / n;

            return a + b * xTarget;
        }

        double xTarget = ToMinutes(targetTime);

        double? Pred(Func<RealtimeSample, double?> selector)
        {
            var pts = ordered
                .Select(s => (x: ToMinutes(s.Timestamp), v: selector(s)))
                .Where(p => p.v.HasValue)
                .Select(p => (p.x, p.v!.Value))
                .ToList();

            if (pts.Count == 0) return null;
            return PredictLinear(pts, xTarget);
        }

        return new MetricPredictionResult
        {
            Nitrogenio = Pred(s => s.Nitrogenio),
            Fosforo = Pred(s => s.Fosforo),
            Potassio = Pred(s => s.Potassio),
            PH = Pred(s => s.PH),
            CondutividadeEletrica = Pred(s => s.CondutividadeEletrica),
            TemperaturaC = Pred(s => s.TemperaturaC),
            Umidade = Pred(s => s.Umidade)
        };
    }
}
