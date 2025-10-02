using MultiSoil_EdgeAI.Utils;
using MultiSoil_EdgeAI.ViewModels;

namespace MultiSoil_EdgeAI.Views;

public partial class ReauthPage : ContentPage
{
    public ReauthPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<ReauthViewModel>();
    }
}
