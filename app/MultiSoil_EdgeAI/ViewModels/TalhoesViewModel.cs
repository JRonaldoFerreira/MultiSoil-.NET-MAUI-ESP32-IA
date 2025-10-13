using System.Collections.ObjectModel;
using System.Windows.Input;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Services;
using MultiSoil_EdgeAI.Views;
using MultiSoil_EdgeAI.ViewModels;


namespace MultiSoil_EdgeAI.ViewModels;


public class TalhoesViewModel : BaseViewModel
{
    private readonly ITalhaoService _service;


    public ObservableCollection<Talhao> Itens { get; } = new();
    public Talhao? Selecionado { get => _selecionado; set => SetProperty(ref _selecionado, value); }
    private Talhao? _selecionado;


    public string? Busca { get => _busca; set { if (SetProperty(ref _busca, value)) _ = CarregarAsync(); } }
    private string? _busca;


    public ICommand NovoCmd { get; }
    public ICommand EditarCmd { get; }
    public ICommand ExcluirCmd { get; }
    public ICommand AtivarCmd { get; }


    public TalhoesViewModel(ITalhaoService service)
    {
        _service = service;
        NovoCmd = new Command(async () => await Shell.Current.GoToAsync(nameof(TalhaoFormPage)));
        EditarCmd = new Command<Talhao>(async t => await Shell.Current.GoToAsync(nameof(TalhaoFormPage) + "?id=" + t.Id));
        ExcluirCmd = new Command<Talhao>(async t => await ExcluirAsync(t));
        AtivarCmd = new Command<Talhao>(async t => await AtivarAsync(t));
    }


    public async Task AppearingAsync() => await CarregarAsync();


    private async Task CarregarAsync()
    {
        if (IsBusy) return; IsBusy = true;
        try
        {
            Itens.Clear();
            foreach (var t in await _service.ListAsync(Busca)) Itens.Add(t);
            var activeId = _service.GetActiveId();
            Selecionado = activeId.HasValue ? Itens.FirstOrDefault(x => x.Id == activeId) : Itens.FirstOrDefault();
        }
        finally { IsBusy = false; }
    }




    private async Task ExcluirAsync(Talhao t)
    {
        if (await App.Current.MainPage.DisplayAlert("Excluir", $"Excluir '{t.Nome}'?", "Sim", "Não"))
        {
            await _service.DeleteAsync(t.Id);
            await CarregarAsync();
        }
    }

    public async Task AtivarAsync(Talhao? t)
    {
        if (t is null) return;

        if (IsBusy) return;
        IsBusy = true;

        try
        {
            await _service.SetActiveAsync(t.Id);   // persiste em Preferences
            Selecionado = t;                        // reflete na UI

            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlert("Talhão ativo",
                    $"'{t.Nome}' definido como ativo.", "OK"));
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlert("Erro",
                    $"Não foi possível ativar o talhão.\n{ex.Message}", "OK"));
        }
        finally
        {
            IsBusy = false;
            (AtivarCmd as Command)?.ChangeCanExecute();
        }
    }


}