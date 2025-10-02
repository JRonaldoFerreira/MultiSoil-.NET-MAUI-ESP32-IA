using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiSoil_EdgeAI.Services;

namespace MultiSoil_EdgeAI.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    [ObservableProperty] private string email = "ana@example.com";
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string? errorMessage;

    public LoginViewModel(IAuthService auth)
    {
        _auth = auth;
        Title = "Entrar";
    }

    [RelayCommand]
    private async Task EntrarAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Preencha e-mail e senha.";
            return;
        }

        try
        {
            IsBusy = true;
            var res = await _auth.LoginAsync(Email, Password);
            if (!res.Success)
            {
                ErrorMessage = res.Message;
                return;
            }

            // Critério de aceitação: sessão persistente + navegação para área logada (dashboard)
            await Shell.Current.GoToAsync("//dashboard");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task IrParaCadastroAsync()
        => Shell.Current.GoToAsync(nameof(Views.RegisterPage));
}
