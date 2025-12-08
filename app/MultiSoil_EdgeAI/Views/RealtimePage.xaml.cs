using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Utils;

namespace MultiSoil_EdgeAI.Views;

[QueryProperty(nameof(Id), "id")]
public partial class RealtimePage : ContentPage
{
    public string? Id { get; set; }

    private readonly ISensorReadingService _sensorService;
    private CancellationTokenSource? _cts;
    private Talhao? _talhao;

    public RealtimePage()
    {
        InitializeComponent();

        // Resolve o serviço via ServiceHelper (já inicializado em MauiProgram)
        _sensorService = ServiceHelper.GetService<ISensorReadingService>();

        TitleLabel.Text = "Dados em tempo real";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        // Inicia o loop de leitura em tempo real
        _ = StartRealtimeLoopAsync(_cts.Token);
    }

    protected override void OnDisappearing()
    {
        _cts?.Cancel();
        base.OnDisappearing();
    }

    /// <summary>
    /// Chamado pelo DashboardViewModel depois da navegação:
    /// if (Shell.Current.CurrentPage is RealtimePage page)
    ///     page.Load(t);
    /// </summary>
    public void Load(Talhao t)
    {
        _talhao = t;

        TitleLabel.Text = $"Talhão: {t.Nome} ({t.Cultura})";
        CoordsLabel.Text = $"Lat/Lon: {t.Latitude}, {t.Longitude}";
        MicroLabel.Text = $"Microcontrolador: {t.Microcontrolador}";
    }

    private async Task StartRealtimeLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Esses parâmetros são exigidos pela interface,
                // mas o ESP32 ignora a query string, então tanto faz.
                var now = DateTime.Now;
                var talhaoId = _talhao?.Id ?? 0;

                var readings = await _sensorService.GetReadingsAsync(
                    talhaoId,
                    now.Date,
                    now.TimeOfDay,
                    now.TimeOfDay,
                    SensorMetric.All,
                    ct);

                if (readings is not null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UmidLabel.Text = readings.Umid.HasValue
                            ? $"{readings.Umid:0.0} %"
                            : "--";

                        TempLabel.Text = readings.Temp.HasValue
                            ? $"{readings.Temp:0.0} °C"
                            : "--";

                        CeLabel.Text = readings.CE.HasValue
                            ? $"{readings.CE:0} µS/cm"
                            : "--";

                        PhLabel.Text = readings.PH.HasValue
                            ? $"{readings.PH:0.0}"
                            : "--";

                        NLabel.Text = readings.N.HasValue
                            ? $"{readings.N:0} mg/kg"
                            : "--";

                        PLabel.Text = readings.P.HasValue
                            ? $"{readings.P:0} mg/kg"
                            : "--";

                        KLabel.Text = readings.K.HasValue
                            ? $"{readings.K:0} mg/kg"
                            : "--";
                    });
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro leitura realtime: {ex.Message}");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        _cts?.Cancel();
        await Shell.Current.GoToAsync("..");
    }
}
