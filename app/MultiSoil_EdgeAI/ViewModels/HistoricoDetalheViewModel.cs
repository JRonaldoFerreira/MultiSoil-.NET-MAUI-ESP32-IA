// ViewModels/HistoricoDetalheViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.ViewModels;

public partial class HistoricoDetalheViewModel : ObservableObject, IQueryAttributableVm
{
    private readonly IHistoricoRepository _historicoRepo;
    private readonly IRealtimeSampleRepository _samplesRepo;
    private readonly ITalhaoRepository _talhaoRepo;

    private int _id;

    [ObservableProperty] private string _talhaoDescricao = "-";
    [ObservableProperty] private string _intervaloText = string.Empty;
    [ObservableProperty] private DateTime _dataColeta;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<RealtimeSample> Capturas { get; } = new();

    public HistoricoDetalheViewModel(
        IHistoricoRepository historicoRepo,
        IRealtimeSampleRepository samplesRepo,
        ITalhaoRepository talhaoRepo)
    {
        _historicoRepo = historicoRepo;
        _samplesRepo = samplesRepo;
        _talhaoRepo = talhaoRepo;
    }

    // Recebe o id pela rota: ...?id=123
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) &&
            idObj is string idStr &&
            int.TryParse(idStr, out var id))
        {
            _id = id;
        }
    }

    public async Task OnAppearing()
    {
        if (_id <= 0) return;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            Capturas.Clear();

            var hist = await _historicoRepo.GetByIdAsync(_id);
            if (hist is null)
            {
                ErrorMessage = "Registro não encontrado.";
                return;
            }

            DataColeta = hist.DataColeta.Date;
            IntervaloText =
                $"{TimeSpan.FromMinutes(hist.HoraInicioMin):hh\\:mm} – {TimeSpan.FromMinutes(hist.HoraFimMin):hh\\:mm}";

            var talhao = await _talhaoRepo.GetByIdAsync(hist.TalhaoId);
            TalhaoDescricao = talhao is null
                ? "-"
                : $"{talhao.Nome} ({talhao.Cultura})";

            var dia = hist.DataColeta.Date;
            var start = dia.AddMinutes(hist.HoraInicioMin);
            var endInclusive = dia.AddMinutes(hist.HoraFimMin)
                                  .AddMinutes(1)
                                  .AddTicks(-1);

            var samples = await _samplesRepo.GetSamplesAsync(
                hist.TalhaoId,
                start,
                endInclusive);

            if (samples.Count == 0)
            {
                // Fallback: tenta o dia inteiro
                var fullDayStart = dia;
                var fullDayEnd = dia.AddDays(1).AddTicks(-1);

                samples = await _samplesRepo.GetSamplesAsync(
                    hist.TalhaoId,
                    fullDayStart,
                    fullDayEnd);
            }

            foreach (var s in samples)
                Capturas.Add(s);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar capturas: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // === COMANDOS ===

    [RelayCommand]
    private async Task Voltar()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (_id <= 0)
            return;

        var ok = await Shell.Current.DisplayAlert(
            "Excluir",
            "Deseja excluir este registro de histórico?",
            "Sim", "Não");

        if (!ok)
            return;

        await _historicoRepo.DeleteAsync(_id);
        await Shell.Current.GoToAsync("..");
    }

    // NOVO: exportar CSV com todas as capturas associadas
    [RelayCommand]
    private async Task ExportCsv()
    {
        if (_id <= 0)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            // Garante que temos capturas carregadas
            if (Capturas.Count == 0)
            {
                await LoadAsync();
            }

            if (Capturas.Count == 0)
            {
                await Shell.Current.DisplayAlert(
                    "Exportar CSV",
                    "Não há capturas para exportar.",
                    "OK");
                return;
            }

            var culture = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();

            // Cabeçalho
            sb.AppendLine("Timestamp;Nitrogenio;Fosforo;Potassio;PH;CondutividadeEletrica;TemperaturaC;Umidade");

            string F(double? v) => v.HasValue
                ? v.Value.ToString("0.######", culture)
                : string.Empty;

            foreach (var s in Capturas)
            {
                var line = string.Join(";", new[]
                {
                    s.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", culture),
                    F(s.Nitrogenio),
                    F(s.Fosforo),
                    F(s.Potassio),
                    F(s.PH),
                    F(s.CondutividadeEletrica),
                    F(s.TemperaturaC),
                    F(s.Umidade)
                });

                sb.AppendLine(line);
            }

            var fileName = $"historico_{_id}_{DataColeta:yyyyMMdd}.csv";
            var dir = FileSystem.CacheDirectory;
            var path = Path.Combine(dir, fileName);

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = $"Exportar histórico #{_id}",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao exportar CSV: {ex.Message}";
            await Shell.Current.DisplayAlert("Erro", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
