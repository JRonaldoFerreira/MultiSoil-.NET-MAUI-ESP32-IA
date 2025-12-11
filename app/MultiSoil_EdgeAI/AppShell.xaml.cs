namespace MultiSoil_EdgeAI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rotas para navegação
        Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
        Routing.RegisterRoute(nameof(Views.TalhoesPage), typeof(Views.TalhoesPage));
        Routing.RegisterRoute(nameof(Views.TalhaoFormPage), typeof(Views.TalhaoFormPage));
        Routing.RegisterRoute(nameof(Views.RealtimePage), typeof(Views.RealtimePage));
        Routing.RegisterRoute(nameof(Views.HistoricosPage), typeof(Views.HistoricosPage));
        Routing.RegisterRoute(nameof(Views.HistoricoFormPage), typeof(Views.HistoricoFormPage));
        Routing.RegisterRoute(nameof(Views.HistoricoDetalhePage), typeof(Views.HistoricoDetalhePage));
        Routing.RegisterRoute(nameof(Views.IAPredictionPage), typeof(Views.IAPredictionPage));





    }
}
