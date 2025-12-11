using MultiSoil_EdgeAI.ViewModels;

namespace MultiSoil_EdgeAI.Views;

public partial class HistoricoDetalhePage : ContentPage, IQueryAttributable
{
    private readonly HistoricoDetalheViewModel _vm;

    public HistoricoDetalhePage(HistoricoDetalheViewModel vm)
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
