using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Services;
using MultiSoil_EdgeAI.Utils;

namespace MultiSoil_EdgeAI.Views;

[QueryProperty(nameof(Mode), "mode")]
public partial class TalhoesPage : ContentPage
{
    private readonly ITalhaoRepository _repo = ServiceHelper.GetService<ITalhaoRepository>();
    private readonly ITalhaoSelectionService _sel = ServiceHelper.GetService<ITalhaoSelectionService>();
    private string _mode = "list"; // "list" | "select"
    public string Mode { get => _mode; set => _mode = value ?? "list"; }

    private List<Talhao> _items = new();

    public TalhoesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _items = (await _repo.GetAllAsync()).ToList();
        ListView.ItemsSource = _items;
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(TalhaoFormPage));
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is not Talhao t) return;

        if (Mode == "select")
        {
            await _sel.SetSelectedAsync(t);
            await Shell.Current.GoToAsync(".."); // volta ao dashboard
        }
        else
        {
            await Shell.Current.GoToAsync($"{nameof(TalhaoFormPage)}?id={t.Id}");
        }
    }

    private async void OnEditSwipe(object sender, EventArgs e)
    {
        if ((sender as Element)?.BindingContext is Talhao t)
            await Shell.Current.GoToAsync($"{nameof(TalhaoFormPage)}?id={t.Id}");
    }

    private async void OnDeleteSwipe(object sender, EventArgs e)
    {
        if ((sender as Element)?.BindingContext is not Talhao t) return;

        var ok = await DisplayAlert("Excluir", $"Excluir \"{t.Nome}\"?", "Sim", "Não");
        if (!ok) return;

        await _repo.DeleteAsync(t.Id);
        // Recarrega a lista
        var itens = await _repo.GetAllAsync();
        ListView.ItemsSource = itens;
    }


}
