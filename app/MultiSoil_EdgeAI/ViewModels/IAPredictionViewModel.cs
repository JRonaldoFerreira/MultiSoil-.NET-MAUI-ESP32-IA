// ViewModels/IAPredictionViewModel.cs
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Services;

namespace MultiSoil_EdgeAI.ViewModels;

public partial class IAPredictionViewModel : ObservableObject
{
    private readonly ITalhaoSelectionService _talhaoSelection;
    private readonly IMetricPredictionService _predictionService;

    private Talhao? _talhaoAtual;

    [ObservableProperty] private string _talhaoDescricao = "Nenhum talhão selecionado.";
    [ObservableProperty] private bool _hasTalhao;

    [ObservableProperty] private double? _predN;
    [ObservableProperty] private double? _predP;
    [ObservableProperty] private double? _predK;
    [ObservableProperty] private double? _predPH;
    [ObservableProperty] private double? _predCE;
    [ObservableProperty] private double? _predT;
    [ObservableProperty] private double? _predU;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public IAPredictionViewModel(
        ITalhaoSelectionService talhaoSelection,
        IMetricPredictionService predictionService)
    {
        _talhaoSelection = talhaoSelection;
        _predictionService = predictionService;
    }

    public async Task OnAppearing()
    {
        await LoadTalhaoAsync();
    }

    private async Task LoadTalhaoAsync()
    {
        _talhaoAtual = await _talhaoSelection.GetSelectedAsync();

        if (_talhaoAtual is null)
        {
            TalhaoDescricao = "Nenhum talhão selecionado.";
            HasTalhao = false;
        }
        else
        {
            TalhaoDescricao = $"{_talhaoAtual.Nome} ({_talhaoAtual.Cultura})";
            HasTalhao = true;
        }
    }

    [RelayCommand]
    private async Task ChangeTalhao()
    {
        await Shell.Current.GoToAsync($"{nameof(Views.TalhoesPage)}?mode=select");
        await LoadTalhaoAsync();
    }

    [RelayCommand]
    private async Task PreverAsync()
    {
        ErrorMessage = null;

        if (_talhaoAtual is null)
        {
            ErrorMessage = "Nenhum talhão selecionado.";
            await Shell.Current.DisplayAlert("IA", ErrorMessage, "OK");
            return;
        }

        try
        {
            IsBusy = true;

            var result = await _predictionService.PredictAsync(_talhaoAtual.Id, DateTime.Now);
            if (result is null)
            {
                ClearPrediction();
                ErrorMessage = "Não há dados suficientes de tempo real para prever.";
                await Shell.Current.DisplayAlert("IA", ErrorMessage, "OK");
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
            ClearPrediction();
            ErrorMessage = $"Erro ao calcular previsão: {ex.Message}";
            await Shell.Current.DisplayAlert("Erro IA", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearPrediction()
    {
        PredN = PredP = PredK = PredPH = PredCE = PredT = PredU = null;
    }
}
