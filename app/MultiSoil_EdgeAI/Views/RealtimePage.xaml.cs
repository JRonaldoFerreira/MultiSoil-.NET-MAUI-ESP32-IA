using System;
using System.Globalization;
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
    private readonly IRealtimeSampleRepository _realtimeRepo;
    private CancellationTokenSource? _cts;
    private Talhao? _talhao;

    public RealtimePage()
    {
        InitializeComponent();

        // Resolve serviços via DI (ServiceHelper inicializado no MauiProgram)
        _sensorService = ServiceHelper.GetService<ISensorReadingService>();
        _realtimeRepo = ServiceHelper.GetService<IRealtimeSampleRepository>();

        TitleLabel.Text = "Dados em tempo real";
        CoordsLabel.Text = string.Empty;
        MicroLabel.Text = string.Empty;
        ServerLabel.Text = string.Empty;
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
        CoordsLabel.Text = $"Lat/Lon: {t.Latitude:0.0000}, {t.Longitude:0.0000}";
        MicroLabel.Text = t.Microcontrolador;

        // Mostra a URL configurada para este talhão
        ServerLabel.Text = string.IsNullOrWhiteSpace(t.ServidorUrl)
            ? "-"
            : t.ServidorUrl;
    }

    private async Task StartRealtimeLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Se ainda não temos talhão, só espera e tenta de novo
                if (_talhao is null || _talhao.Id <= 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
                    continue;
                }

                var now = DateTime.Now;

                var readings = await _sensorService.GetReadingsAsync(
                    _talhao.Id,
                    now.Date,
                    now.TimeOfDay,
                    now.TimeOfDay,
                    SensorMetric.All,
                    ct);

                if (readings is not null)
                {
                    // Atualiza UI
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        NValueLabel.Text = FormatValue(readings.N, "0");
                        PValueLabel.Text = FormatValue(readings.P, "0");
                        KValueLabel.Text = FormatValue(readings.K, "0");
                        PhValueLabel.Text = FormatValue(readings.PH, "0.0");
                        CeValueLabel.Text = FormatValue(readings.CE, "0.0");
                        TempValueLabel.Text = FormatValue(readings.Temp, "0.0");
                        UmidValueLabel.Text = FormatValue(readings.Umid, "0.0");
                    });

                    // === NOVO: grava essa leitura no banco local ===
                    try
                    {
                        var sample = new RealtimeSample
                        {
                            TalhaoId = _talhao.Id,
                            Timestamp = now,

                            Nitrogenio = readings.N,
                            Fosforo = readings.P,
                            Potassio = readings.K,
                            PH = readings.PH,
                            CondutividadeEletrica = readings.CE,
                            TemperaturaC = readings.Temp,
                            Umidade = readings.Umid
                        };

                        await _realtimeRepo.AddAsync(sample);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao salvar RealtimeSample: {ex.Message}");
                    }
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

    private static string FormatValue(double? value, string format)
    {
        if (!value.HasValue)
            return "--";

        return value.Value.ToString(format, CultureInfo.InvariantCulture);
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        _cts?.Cancel();
        await Shell.Current.GoToAsync("..");
    }
}
