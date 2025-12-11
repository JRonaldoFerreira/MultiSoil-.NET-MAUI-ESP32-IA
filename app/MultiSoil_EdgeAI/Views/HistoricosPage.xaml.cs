// Views/HistoricosPage.xaml.cs
using MultiSoil_EdgeAI.ViewModels;

namespace MultiSoil_EdgeAI.Views;

public partial class HistoricosPage : ContentPage
{
    private readonly HistoricosViewModel _vm;

    public HistoricosPage(HistoricosViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.OnAppearing();
    }

    // Chamado pelo TapGestureRecognizer do cartão
    private async void OnItemTapped(object sender, TappedEventArgs e)
    {
        if (sender is not BindableObject bo)
            return;

        if (bo.BindingContext is not HistoricoRow row)
            return;

        // Navega para a página de detalhe com o ID do histórico
        await Shell.Current.GoToAsync(
            $"{nameof(HistoricoDetalhePage)}?id={row.Id}");
    }
}
