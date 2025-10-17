using MultiSoil_EdgeAI.ViewModels;

namespace MultiSoil_EdgeAI.Views;

public partial class HistoricoFormPage : ContentPage, IQueryAttributable
{
    private readonly HistoricoFormViewModel _vm;

    public HistoricoFormPage(HistoricoFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is IQueryAttributableVm q)
            q.ApplyQueryAttributes(query);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.OnAppearing();
    }
}
