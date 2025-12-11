// Views/IAPredictionPage.xaml.cs
using MultiSoil_EdgeAI.ViewModels;

namespace MultiSoil_EdgeAI.Views;

public partial class IAPredictionPage : ContentPage
{
    private readonly IAPredictionViewModel _vm;

    public IAPredictionPage(IAPredictionViewModel vm)
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
