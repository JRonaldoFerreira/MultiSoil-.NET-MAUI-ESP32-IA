using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiSoil_EdgeAI.Services;

namespace MultiSoil_EdgeAI.ViewModels;

public partial class ReauthViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly ISessionService _session;

    [ObservableProperty] private string emailMasked = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string? errorMessage;

    public ReauthViewModel(IAuthService auth, ISessionService session)
    {
        _auth = auth;
        _session = session;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var email = await _session.GetCurrentEmailAsync() ?? "";
        EmailMasked = Mask(email);
    }

    private static string Mask(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return email;
        var parts = email.Split('@');
        var name = parts[0];
        var masked = name.Length <= 2 ? new string('*', name.Length) : $"{name[0]}***{name[^1]}";
        return $"{masked}@{parts[1]}";
    }

    [RelayCommand]
    private async Task ReauthAsync()
    {
        ErrorMessage = null;

        var email = await _session.GetCurrentEmailAsync();
        if (string.IsNullOrEmpty(email))
        {
            ErrorMessage = "Sessão inválida. Faça login novamente.";
            await Shell.Current.GoToAsync("///login");
            return;
        }

        var res = await _auth.LoginAsync(email, Password);
        if (!res.Success)
        {
            ErrorMessage = "Senha incorreta.";
            return;
        }

        Password = string.Empty;
        if (Shell.Current.Navigation.ModalStack.Count > 0)
            await Shell.Current.Navigation.PopModalAsync(); // desbloqueia
    }

    [RelayCommand]
    private async Task TrocarUsuarioAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("///login");
    }
}
