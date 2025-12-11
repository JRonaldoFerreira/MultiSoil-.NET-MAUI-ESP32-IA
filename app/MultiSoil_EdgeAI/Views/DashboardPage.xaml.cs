// Views/DashboardPage.xaml.cs
using MultiSoil_EdgeAI.Utils;
using MultiSoil_EdgeAI.ViewModels;

namespace MultiSoil_EdgeAI.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _vm = ServiceHelper.GetService<DashboardViewModel>();

    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.OnAppearingAsync();
    }

    private async void OnIAPredictionClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(IAPredictionPage));
    }
}
