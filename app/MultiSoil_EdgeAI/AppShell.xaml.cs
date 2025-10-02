namespace MultiSoil_EdgeAI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rotas para navegação
        Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
    }
}
