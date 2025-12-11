// ViewModels/RealtimeViewModel.cs
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Services; // para ITalhaoSelectionService

namespace MultiSoil_EdgeAI.ViewModels;

public partial class RealtimeViewModel : ObservableObject
{
    private readonly ISensorReadingService _sensorService;
    private readonly ITalhaoSelectionService _talhaoSelection;
    private readonly IRealtimeSampleRepository _realtimeRepo;

    [ObservableProperty] private double? n;
    [ObservableProperty] private double? p;
    [ObservableProperty] private double? k;

    // IMPORTANTE:
    // [ObservableProperty] private double? ph; -> gera "Ph"
    // [ObservableProperty] private double? ce; -> gera "Ce"
    [ObservableProperty] private double? ph;
    [ObservableProperty] private double? ce;

    [ObservableProperty] private double? temp;
    [ObservableProperty] private double? umid;

    public RealtimeViewModel(
        ISensorReadingService sensorService,
        ITalhaoSelectionService talhaoSelection,
        IRealtimeSampleRepository realtimeRepo)
    {
        _sensorService = sensorService;
        _talhaoSelection = talhaoSelection;
        _realtimeRepo = realtimeRepo;
    }

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
            Ph = readings.PH;   // propriedade é "Ph"
            Ce = readings.CE;   // propriedade é "Ce"
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
}
