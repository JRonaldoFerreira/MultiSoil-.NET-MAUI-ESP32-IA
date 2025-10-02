using MultiSoil_EdgeAI.Services;
using MultiSoil_EdgeAI.Utils;

namespace MultiSoil_EdgeAI.Behaviors;

public class AuthGuardBehavior : Behavior<Page>
{
    private Page? _page;
    private CancellationTokenSource? _cts;
    private ISessionService Session => ServiceHelper.GetService<ISessionService>();

    protected override void OnAttachedTo(Page bindable)
    {
        base.OnAttachedTo(bindable);
        _page = bindable;
        bindable.Appearing += OnAppearing;
        bindable.Disappearing += OnDisappearing;
    }

    protected override void OnDetachingFrom(Page bindable)
    {
        bindable.Appearing -= OnAppearing;
        bindable.Disappearing -= OnDisappearing;
        StopWatcher();
        _page = null;
        base.OnDetachingFrom(bindable);
    }

    private async void OnAppearing(object? sender, EventArgs e)
    {
        // Checa imediatamente ao aparecer
        if (await Session.IsExpiredAsync())
        {
            await OpenReauthModalAsync();
            return;
        }

        // Se não expirou, começa a vigiar (1s)
        StartWatcher();

        // Se for expiração deslizante, renova ao entrar na página
        await Session.TouchAsync();
    }

    private void OnDisappearing(object? sender, EventArgs e) => StopWatcher();

    private void StartWatcher()
    {
        StopWatcher();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        // checa a cada 1 segundo enquanto a página estiver visível
        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (token.IsCancellationRequested) return false;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // evita abrir várias modais
                    var modalOpen = Shell.Current?.Navigation?.ModalStack?.Any(p => p is Views.ReauthPage) == true;
                    if (modalOpen) return;

                    if (await Session.IsExpiredAsync())
                    {
                        await OpenReauthModalAsync();
                    }
                }
                catch
                {
                    // fallback seguro: vai pro login se algo der ruim
                    await Shell.Current.GoToAsync("///login");
                }
            });

            return !token.IsCancellationRequested;
        });
    }

    private void StopWatcher()
    {
        _cts?.Cancel();
        _cts = null;
    }

    private static async Task OpenReauthModalAsync()
    {
        var page = ServiceHelper.GetService<Views.ReauthPage>();
        await Shell.Current.Navigation.PushModalAsync(page);
    }
}
