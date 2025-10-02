using MultiSoil_EdgeAI.Services;
using MultiSoil_EdgeAI.Utils;

namespace MultiSoil_EdgeAI.Views;

public partial class DashboardPage : ContentPage //resolvndo a issue 15
{
    ISessionService Session => ServiceHelper.GetService<ISessionService>();
    IAuthService Auth => ServiceHelper.GetService<IAuthService>();

    CancellationTokenSource? _hudCts;

    public DashboardPage()
    {
        InitializeComponent();
        Appearing += OnAppearing;
        Disappearing += OnDisappearing;
    }

    void OnAppearing(object? s, EventArgs e) => StartHudTimer();
    void OnDisappearing(object? s, EventArgs e) { _hudCts?.Cancel(); _hudCts = null; }

    // Botão: renova o TTL agora
    private async void OnKeepAliveClicked(object sender, EventArgs e)
    {
        await Session.TouchAsync();

        // feedback rápido no HUD
        var rem = await Session.GetRemainingAsync();
        if (rem != null)
            LblRemaining.Text = $"Sessão renovada: ~{(int)Math.Ceiling(rem.Value.TotalSeconds)}s restantes";
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Auth.LogoutAsync();
        await Shell.Current.GoToAsync("///login");
    }

    // HUD: mostra o tempo restante atualizando a cada 1s
    void StartHudTimer()
    {
        _hudCts?.Cancel();
        _hudCts = new CancellationTokenSource();
        var tk = _hudCts.Token;

        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (tk.IsCancellationRequested) return false;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var rem = await Session.GetRemainingAsync();
                LblRemaining.Text = rem is null
                    ? "Sessão: (sem dados)"
                    : $"Sessão: resta ~{(int)Math.Ceiling(rem.Value.TotalSeconds)}s";
            });

            return !tk.IsCancellationRequested;
        });
    }
}
