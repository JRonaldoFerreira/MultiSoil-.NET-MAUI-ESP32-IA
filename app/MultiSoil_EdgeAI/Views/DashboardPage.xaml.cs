using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Services;
using MultiSoil_EdgeAI.Utils;

namespace MultiSoil_EdgeAI.Views;

public partial class DashboardPage : ContentPage
{
    private ITalhaoService Talhoes => ServiceHelper.GetService<ITalhaoService>();

    public DashboardPage()
    {
        InitializeComponent(); // <-- s� funciona se XAML + namespace/classe estiverem id�nticos
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var lista = await Talhoes.ListAsync();
        TalhaoPicker.ItemsSource = lista;
        TalhaoPicker.ItemDisplayBinding = new Binding(nameof(Talhao.Nome));

        var activeId = Talhoes.GetActiveId();
        var active = activeId is int id ? lista.FirstOrDefault(x => x.Id == id) : null;
        TalhaoPicker.SelectedItem = active ?? lista.FirstOrDefault();

        TalhaoPicker.SelectedIndexChanged -= OnTalhaoChanged;
        TalhaoPicker.SelectedIndexChanged += OnTalhaoChanged;
    }

    private async void OnTalhaoChanged(object? sender, EventArgs e)
    {
        if (TalhaoPicker.SelectedItem is Talhao t)
            await Talhoes.SetActiveAsync(t.Id);
    }

    private async void OnNovoTalhaoClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("talhoes");

    private async void OnLeiturasClicked(object sender, EventArgs e)
        => await DisplayAlert("Leituras", "Abrir tela de leituras em tempo real�", "OK");

    private async void OnHistoricoClicked(object sender, EventArgs e)
        => await DisplayAlert("Hist�rico", "Abrir hist�rico do talh�o ativo�", "OK");

    private async void OnPrevisoesClicked(object sender, EventArgs e)
        => await DisplayAlert("Previs�es", "Abrir previs�es�", "OK");
}
