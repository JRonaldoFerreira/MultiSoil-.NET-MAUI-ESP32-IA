using MultiSoil_EdgeAI.ViewModels;


namespace MultiSoil_EdgeAI.Views;


public partial class TalhoesPage : ContentPage
{
    public TalhoesPage() { InitializeComponent(); }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TalhoesViewModel vm) await vm.AppearingAsync();
    }
}