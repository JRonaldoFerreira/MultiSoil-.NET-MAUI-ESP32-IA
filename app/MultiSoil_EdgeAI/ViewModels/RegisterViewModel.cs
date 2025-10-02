using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiSoil_EdgeAI.Services;

namespace MultiSoil_EdgeAI.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string confirmPassword = string.Empty;
    [ObservableProperty] private string? errorMessage;

    public RegisterViewModel(IAuthService auth)
    {
        _auth = auth;
        Title = "Criar conta";
    }

    [RelayCommand]
    private async Task RegistrarAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Informe e-mail e senha.";
            return;
        }
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "As senhas não coincidem.";
            return;
        }

        try
        {
            IsBusy = true;
            var res = await _auth.RegisterAsync(Email, Password);
            if (!res.Success)
            {
                ErrorMessage = res.Message;
                return;
            }

            await Shell.Current.GoToAsync("//dashboard");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
