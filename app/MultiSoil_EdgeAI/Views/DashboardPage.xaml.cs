using MultiSoil_EdgeAI.Services;
using MultiSoil_EdgeAI.Utils;

namespace MultiSoil_EdgeAI.Views;

public partial class DashboardPage : ContentPage //resolvndo a issue do login 15
{
    ISessionService Session => ServiceHelper.GetService<ISessionService>();
    IAuthService Auth => ServiceHelper.GetService<IAuthService>();

    CancellationTokenSource? _hudCts;

    public DashboardPage()
    {
        InitializeComponent();
       
        Disappearing += OnDisappearing;
    }

    
    void OnDisappearing(object? s, EventArgs e) { _hudCts?.Cancel(); _hudCts = null; }

    // Botão: renova o TTL agora
    private async void OnKeepAliveClicked(object sender, EventArgs e)
    {
        await Session.TouchAsync();

        
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Auth.LogoutAsync();
        await Shell.Current.GoToAsync("///login");
    }

    // HUD: mostra o tempo restante atualizando a cada 1s
 
}
