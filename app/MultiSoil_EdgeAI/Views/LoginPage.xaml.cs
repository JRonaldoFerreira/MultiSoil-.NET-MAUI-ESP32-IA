using MultiSoil_EdgeAI.Utils;
using MultiSoil_EdgeAI.ViewModels;

namespace MultiSoil_EdgeAI.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent(); // <- este m�todo � gerado pelo build do XAML
        BindingContext = ServiceHelper.GetService<LoginViewModel>();
    }
}
