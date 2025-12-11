// ViewModels/HistoricosViewModel.cs
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Services;

namespace MultiSoil_EdgeAI.ViewModels;

public partial class HistoricosViewModel : ObservableObject
{
    private readonly IHistoricoRepository _repo;
    private readonly ITalhaoRepository _talhoesRepo;
    private readonly ITalhaoSelectionService _talhaoSelection;

    private Talhao? _talhaoAtual;

    [ObservableProperty] private DateTime _startDate;
    [ObservableProperty] private DateTime _endDate;
    [ObservableProperty] private string _talhaoDescricao = "-";

    public ObservableCollection<HistoricoRow> Items { get; } = new();

    public HistoricosViewModel(
        IHistoricoRepository repo,
        ITalhaoRepository talhoesRepo,
        ITalhaoSelectionService talhaoSelection)
    {
        _repo = repo;
        _talhoesRepo = talhoesRepo;
        _talhaoSelection = talhaoSelection;

        var hoje = DateTime.Today;
        _startDate = hoje.AddDays(-30);
        _endDate = hoje;
    }

    public async Task OnAppearing()
    {
        await LoadTalhaoAsync();
        await LoadAsync();
    }

    private async Task LoadTalhaoAsync()
    {
        _talhaoAtual = await _talhaoSelection.GetSelectedAsync();
        if (_talhaoAtual is null)
        {
            TalhaoDescricao = "Nenhum talhão selecionado.";
            await Shell.Current.DisplayAlert("Talhão", "Selecione um talhão para ver o histórico.", "OK");
            await Shell.Current.GoToAsync($"{nameof(Views.TalhoesPage)}?mode=select");
            _talhaoAtual = await _talhaoSelection.GetSelectedAsync();
        }

        TalhaoDescricao = _talhaoAtual is null
            ? "-"
            : $"{_talhaoAtual.Nome} ({_talhaoAtual.Cultura})";
    }

    [RelayCommand]
    private async Task ChangeTalhao()
    {
        await Shell.Current.GoToAsync($"{nameof(Views.TalhoesPage)}?mode=select");
        await LoadTalhaoAsync();
        await LoadAsync();
    }

    [RelayCommand]
    private Task Load() => LoadAsync();

    private async Task LoadAsync()
    {
        Items.Clear();
        if (_talhaoAtual is null) return;

        var list = await _repo.GetAllAsync(_talhaoAtual.Id, StartDate, EndDate);
        foreach (var h in list)
        {
            Items.Add(HistoricoRow.From(h, _talhaoAtual.Nome));
        }
    }

    [RelayCommand]
    private async Task Add()
    {
        var talhaoId = _talhaoAtual?.Id ?? 0;
        await Shell.Current.GoToAsync($"{nameof(Views.HistoricoFormPage)}?talhaoId={talhaoId}");
    }

    [RelayCommand]
    private async Task Edit(HistoricoRow? item)
    {
        if (item is null) return;

        await Shell.Current.GoToAsync($"{nameof(Views.HistoricoFormPage)}?id={item.Id}");
    }

    [RelayCommand]
    private async Task Delete(HistoricoRow? item)
    {
        if (item is null) return;

        var ok = await Shell.Current.DisplayAlert(
            "Excluir",
            $"Excluir registro de {item.DataColeta:dd/MM/yyyy}?",
            "Sim", "Não");

        if (!ok) return;

        await _repo.DeleteAsync(item.Id);
        await LoadAsync();
    }
}
