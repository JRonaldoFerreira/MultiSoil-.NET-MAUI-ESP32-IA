// ViewModels/RealtimeViewModel.cs
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Services; // ITalhaoSelectionService, IMetricPredictionService

namespace MultiSoil_EdgeAI.ViewModels;

public partial class RealtimeViewModel : ObservableObject
{
    private readonly ISensorReadingService _sensorService;
    private readonly ITalhaoSelectionService _talhaoSelection;
    private readonly IRealtimeSampleRepository _realtimeRepo;
    private readonly IMetricPredictionService _predictionService;

    // Leitura atual do sensor
    [ObservableProperty] private double? n;
    [ObservableProperty] private double? p;
    [ObservableProperty] private double? k;
    [ObservableProperty] private double? ph;
    [ObservableProperty] private double? ce;
    [ObservableProperty] private double? temp;
    [ObservableProperty] private double? umid;

    // PREVISÃO (IA)
    [ObservableProperty] private double? predN;
    [ObservableProperty] private double? predP;
    [ObservableProperty] private double? predK;
    [ObservableProperty] private double? predPH;
    [ObservableProperty] private double? predCE;
    [ObservableProperty] private double? predT;
    [ObservableProperty] private double? predU;

    public RealtimeViewModel(
        ISensorReadingService sensorService,
        ITalhaoSelectionService talhaoSelection,
        IRealtimeSampleRepository realtimeRepo,
        IMetricPredictionService predictionService)
    {
        _sensorService = sensorService;
        _talhaoSelection = talhaoSelection;
        _realtimeRepo = realtimeRepo;
        _predictionService = predictionService;
    }

    // ===== Leitura real do sensor =====
    [RelayCommand]
    private async Task AtualizarAsync()
    {
        var talhao = await _talhaoSelection.GetSelectedAsync();
        if (talhao is null)
        {
            await Shell.Current.DisplayAlert("Tempo real", "Selecione um talhão.", "OK");
            return;
        }

        try
        {
            var readings = await _sensorService.GetReadingsAsync(
                talhao.Id,
                DateTime.Now,
                TimeSpan.Zero,
                TimeSpan.Zero,
                SensorMetric.N | SensorMetric.P | SensorMetric.K |
                SensorMetric.PH | SensorMetric.CE |
                SensorMetric.Temp | SensorMetric.Umid);

            if (readings is null)
            {
                await Shell.Current.DisplayAlert("Tempo real", "Não foi possível obter leituras do ESP32.", "OK");
                return;
            }

            // Atualiza propriedades ligadas à tela
            N = readings.N;
            P = readings.P;
            K = readings.K;
            Ph = readings.PH;
            Ce = readings.CE;
            Temp = readings.Temp;
            Umid = readings.Umid;

            // Salva como amostra no banco local (RealtimeSamples)
            var sample = new RealtimeSample
            {
                TalhaoId = talhao.Id,
                Timestamp = DateTime.Now,
                Nitrogenio = readings.N,
                Fosforo = readings.P,
                Potassio = readings.K,
                PH = readings.PH,
                CondutividadeEletrica = readings.CE,
                TemperaturaC = readings.Temp,
                Umidade = readings.Umid
            };

            await _realtimeRepo.AddAsync(sample);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Falha ao comunicar com o ESP32: {ex.Message}", "OK");
        }
    }

    // ===== PREVISÃO (IA) =====
    [RelayCommand]
    private async Task PreverAsync()
    {
        var talhao = await _talhaoSelection.GetSelectedAsync();
        if (talhao is null)
        {
            await Shell.Current.DisplayAlert("IA", "Selecione um talhão.", "OK");
            return;
        }

        try
        {
            var result = await _predictionService.PredictAsync(talhao.Id, DateTime.Now);
            if (result is null)
            {
                await Shell.Current.DisplayAlert(
                    "IA",
                    "Não há dados suficientes de tempo real para fazer a previsão.",
                    "OK");
                return;
            }

            PredN = result.Nitrogenio;
            PredP = result.Fosforo;
            PredK = result.Potassio;
            PredPH = result.PH;
            PredCE = result.CondutividadeEletrica;
            PredT = result.TemperaturaC;
            PredU = result.Umidade;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro IA", $"Falha ao calcular previsão: {ex.Message}", "OK");
        }
    }
}
