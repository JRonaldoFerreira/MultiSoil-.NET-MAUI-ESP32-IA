using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Utils;
using System.Globalization;

namespace MultiSoil_EdgeAI.Views;

[QueryProperty(nameof(Id), "id")]
public partial class TalhaoFormPage : ContentPage
{
    private readonly ITalhaoRepository _repo = ServiceHelper.GetService<ITalhaoRepository>();
    public string? Id { get; set; }
    private Talhao? _model;

    public TalhaoFormPage()
    {
        InitializeComponent();
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (int.TryParse(Id, out var id))
        {
            _model = await _repo.GetByIdAsync(id);
            if (_model is not null)
            {
                AreaEntry.Text = _model.AreaHa.ToString(CultureInfo.InvariantCulture);
                NomeEntry.Text = _model.Nome;
                CulturaEntry.Text = _model.Cultura;
                LatEntry.Text = _model.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                LonEntry.Text = _model.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                MicroEntry.Text = _model.Microcontrolador;
            }
        }
        else
        {
            _model = new Talhao();
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_model is null) return;
        _model.Nome = NomeEntry.Text?.Trim() ?? string.Empty;
        _model.Cultura = CulturaEntry.Text?.Trim() ?? string.Empty;
        _model.Microcontrolador = MicroEntry.Text?.Trim() ?? string.Empty;

        if (double.TryParse(AreaEntry.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var area))
            _model.AreaHa = area;
        else
            _model.AreaHa = 0d; // garante valor para NOT NULL

        if (double.TryParse(LatEntry.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat))
            _model.Latitude = lat;
        if (double.TryParse(LonEntry.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon))
            _model.Longitude = lon;

        if (_model.Id == 0)
            await _repo.CreateAsync(_model);
        else
            await _repo.UpdateAsync(_model);

        await Shell.Current.GoToAsync("..");
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
