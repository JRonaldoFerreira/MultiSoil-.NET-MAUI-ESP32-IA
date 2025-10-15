using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.Views;

[QueryProperty(nameof(Id), "id")]
public partial class RealtimePage : ContentPage
{
    public string? Id { get; set; }

    public RealtimePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        TitleLabel.Text = "Sem talhão definido";
    }

    public void Load(Talhao t)
    {
        TitleLabel.Text = $"Talhão: {t.Nome} ({t.Cultura})";
        CoordsLabel.Text = $"Lat/Lon: {t.Latitude}, {t.Longitude}";
        MicroLabel.Text = $"Microcontrolador: {t.Microcontrolador}";
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
