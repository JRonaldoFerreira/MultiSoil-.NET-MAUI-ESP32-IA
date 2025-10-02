using CommunityToolkit.Mvvm.ComponentModel;

namespace MultiSoil_EdgeAI.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? title;
}
