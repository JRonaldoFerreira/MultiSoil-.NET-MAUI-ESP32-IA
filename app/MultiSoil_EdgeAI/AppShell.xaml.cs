using MultiSoil_EdgeAI.Views;

namespace MultiSoil_EdgeAI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rotas fora das abas (CRUD Talhão)
        Routing.RegisterRoute("talhoes", typeof(TalhoesPage));                 // lista/ativar/editar/excluir
        Routing.RegisterRoute(nameof(TalhaoFormPage), typeof(TalhaoFormPage)); // formulário
    }
}
