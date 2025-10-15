// ViewModels/DashboardViewModel.cs
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using MultiSoil_EdgeAI.Services;
using MultiSoil_EdgeAI.Views;

namespace MultiSoil_EdgeAI.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ISessionService _session;
    private readonly IAuthService _auth;
    private readonly ITalhaoSelectionService _sel;

    public DashboardViewModel(
        ISessionService session,
        IAuthService auth,
        ITalhaoSelectionService sel)
    {
        _session = session;
        _auth = auth;
        _sel = sel;

        Title = "Dashboard";
    }

    // Estado exibido na UI
    [ObservableProperty]
    private string selectedTalhaoTitle = "(nenhum)";

    [ObservableProperty]
    private bool isRealtimeEnabled;

    // O Toolkit gerará propriedades ICommand com o sufixo "Command"
    // Ex.: SelectTalhaoCommand, RealtimeCommand, KeepAliveCommand, LogoutCommand

    public async Task OnAppearingAsync() => await RefreshSelectedTalhaoAsync();

    private async Task RefreshSelectedTalhaoAsync()
    {
        var t = await _sel.GetSelectedAsync();
        if (t is null)
        {
            SelectedTalhaoTitle = "(nenhum)";
            IsRealtimeEnabled = false;
        }
        else
        {
            SelectedTalhaoTitle = $"{t.Nome} · {t.Cultura}";
            IsRealtimeEnabled = true;
        }
    }

    // --- Commands ---

    [RelayCommand]
    private async Task SelectTalhao()
    {
        KeepAlive();
        await Shell.Current.GoToAsync($"{nameof(TalhoesPage)}?mode=select");
        await RefreshSelectedTalhaoAsync();
    }

    // CanExecute baseado em IsRealtimeEnabled
    private bool CanRealtime() => IsRealtimeEnabled;

    [RelayCommand(CanExecute = nameof(CanRealtime))]
    private async Task Realtime()
    {
        KeepAlive();
        var t = await _sel.GetSelectedAsync();
        if (t is null)
        {
            await Application.Current.MainPage.DisplayAlert("Talhão", "Selecione um talhão antes.", "OK");
            return;
        }

        await Shell.Current.GoToAsync(nameof(RealtimePage));
        if (Shell.Current.CurrentPage is RealtimePage page)
            page.Load(t);
    }

    // Quando IsRealtimeEnabled mudar, atualize o CanExecute do comando
    partial void OnIsRealtimeEnabledChanged(bool value)
        => RealtimeCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private Task KeepAlive() => _session.TouchAsync();

    [RelayCommand]
    private async Task Logout()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("///login");
    }
}
