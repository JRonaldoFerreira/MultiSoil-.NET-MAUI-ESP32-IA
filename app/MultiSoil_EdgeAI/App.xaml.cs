using MultiSoil_EdgeAI.Data;


namespace MultiSoil_EdgeAI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }

    protected override async void OnStart()
    {
        // Garantir DB criado ao abrir
        var db = Utils.ServiceHelper.GetService<LocalDb>();
        await db.InitializeAsync();

        // Mantemos a rota inicial como "login"; o login bem-sucedido navega para "//dashboard".
        base.OnStart();
    }
}
