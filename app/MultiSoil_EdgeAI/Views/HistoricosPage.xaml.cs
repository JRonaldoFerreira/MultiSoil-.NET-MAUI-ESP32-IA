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
}
