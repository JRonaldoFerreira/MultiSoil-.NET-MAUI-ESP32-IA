// Services/IMetricPredictionService.cs
using System;
using System.Threading.Tasks;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Services;

public class MetricPredictionResult
{
    public double? Nitrogenio { get; set; }
    public double? Fosforo { get; set; }
    public double? Potassio { get; set; }
    public double? PH { get; set; }
    public double? CondutividadeEletrica { get; set; }
    public double? TemperaturaC { get; set; }
    public double? Umidade { get; set; }
}

public interface IMetricPredictionService
{
    /// <summary>
    /// Prediz os valores atuais das métricas de solo para um talhão,
    /// usando as últimas amostras de tempo real até o instante alvo.
    /// </summary>
    Task<MetricPredictionResult?> PredictAsync(int talhaoId, DateTime targetTime);
}
