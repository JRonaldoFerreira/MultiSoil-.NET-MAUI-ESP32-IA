// ViewModels/HistoricoFormViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.ViewModels;

public partial class HistoricoFormViewModel : ObservableObject, IQueryAttributableVm
{
    private readonly IHistoricoRepository _repo;
    private readonly IRealtimeSampleRepository _realtimeRepo;
    private readonly ITalhaoRepository _talhoesRepo;

    private int _id;           // se >0 edita (carrega valores salvos)
    private int _prefTalhaoId; // talhão preferido (quando vindo da lista)

    public ObservableCollection<Talhao> Talhoes { get; } = new();

    [ObservableProperty] private Talhao? selectedTalhao;
    [ObservableProperty] private DateTime dataColeta = DateTime.Today;

    // se quiser começar o formulário com 00:00–23:59:
    [ObservableProperty] private TimeSpan inicioTime = TimeSpan.Zero;              // 00:00
    [ObservableProperty] private TimeSpan fimTime = new TimeSpan(23, 59, 0);
    // Seleção de métricas (quais serão consideradas ao montar o histórico)
    [ObservableProperty] private bool selN = true;
    [ObservableProperty] private bool selP = true;
    [ObservableProperty] private bool selK = true;
    [ObservableProperty] private bool selPH = true;
    [ObservableProperty] private bool selCE = true;
    [ObservableProperty] private bool selT = true;
    [ObservableProperty] private bool selU = true;

    // Prévia (valores agregados a partir das leituras em tempo real)
    [ObservableProperty] private double? prevN;
    [ObservableProperty] private double? prevP;
    [ObservableProperty] private double? prevK;
    [ObservableProperty] private double? prevPH;
    [ObservableProperty] private double? prevCE;
    [ObservableProperty] private double? prevT;
    [ObservableProperty] private double? prevU;

    [ObservableProperty] private bool hasPreview;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? errorMessage;

    public HistoricoFormViewModel(
        IHistoricoRepository repo,
        ITalhaoRepository talhoesRepo,
        IRealtimeSampleRepository realtimeRepo)
    {
        _repo = repo;
        _talhoesRepo = talhoesRepo;
        _realtimeRepo = realtimeRepo;
    }

    // Chamado no OnAppearing da View
    public async Task OnAppearing()
    {
        await LoadTalhoesAsync();

        if (_id > 0)
        {
            // Edição: carrega o registro já salvo
            var m = await _repo.GetByIdAsync(_id);
            if (m is not null)
            {
                SelectedTalhao = Talhoes.FirstOrDefault(t => t.Id == m.TalhaoId);
                DataColeta = m.DataColeta.Date;
                InicioTime = TimeSpan.FromMinutes(m.HoraInicioMin);
                FimTime = TimeSpan.FromMinutes(m.HoraFimMin);

                PrevN = m.Nitrogenio;
                PrevP = m.Fosforo;
                PrevK = m.Potassio;
                PrevPH = m.PH;
                PrevCE = m.CondutividadeEletrica;
                PrevT = m.TemperaturaC;
                PrevU = m.Umidade;

                HasPreview = true;
                return;
            }
        }

        if (_prefTalhaoId > 0)
            SelectedTalhao = Talhoes.FirstOrDefault(t => t.Id == _prefTalhaoId);
    }

    private async Task LoadTalhoesAsync()
    {
        Talhoes.Clear();
        var list = await _talhoesRepo.GetAllAsync();
        foreach (var t in list)
            Talhoes.Add(t);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) &&
            idObj is string idStr &&
            int.TryParse(idStr, out var id))
        {
            _id = id;
        }

        if (query.TryGetValue("talhaoId", out var tObj) &&
            tObj is string tStr &&
            int.TryParse(tStr, out var tid))
        {
            _prefTalhaoId = tid;
        }
    }

    // === BOTÃO "BUSCAR NO SERVIDOR" (AGORA: BUSCAR NO BANCO LOCAL) ===
    [RelayCommand]
    
    private async Task Buscar()
    {
        ErrorMessage = null;
        HasPreview = false;

        if (SelectedTalhao is null)
        {
            ErrorMessage = "Selecione um talhão.";
            await Shell.Current.DisplayAlert("Histórico", ErrorMessage, "OK");
            return;
        }

        if (FimTime <= InicioTime)
        {
            ErrorMessage = "Hora final deve ser maior que a inicial.";
            await Shell.Current.DisplayAlert("Histórico", ErrorMessage, "OK");
            return;
        }

        var dia = DataColeta.Date;

        // 1) Intervalo escolhido pelo usuário
        var start = dia + InicioTime;
        var end = dia + FimTime;

        try
        {
            IsBusy = true;

            var samples = await _realtimeRepo.GetSamplesAsync(SelectedTalhao.Id, start, end);

            // 2) Se não achou nada, tenta o dia inteiro como fallback
            if (samples.Count == 0)
            {
                var fullDayStart = dia;
                var fullDayEnd = dia.AddDays(1).AddTicks(-1); // 23:59:59.999...

                samples = await _realtimeRepo.GetSamplesAsync(SelectedTalhao.Id, fullDayStart, fullDayEnd);

                if (samples.Count == 0)
                {
                    ErrorMessage = "Nenhuma leitura em tempo real encontrada para essa data no banco local.";
                    await Shell.Current.DisplayAlert("Histórico", ErrorMessage, "OK");
                    return;
                }
                else
                {
                    // Opcional: avisa que usou o dia inteiro
                    await Shell.Current.DisplayAlert(
                        "Histórico",
                        $"Não foram encontradas leituras no intervalo {InicioTime:hh\\:mm}–{FimTime:hh\\:mm}, " +
                        $"mas foram encontradas {samples.Count} leituras ao longo do dia todo.",
                        "OK");
                }
            }

            // Helper: média ignorando null
            static double? Avg(Func<RealtimeSample, double?> selector, IEnumerable<RealtimeSample> list)
            {
                var vals = list.Select(selector)
                               .Where(v => v.HasValue)
                               .Select(v => v!.Value)
                               .ToList();

                if (vals.Count == 0) return null;
                return vals.Average();
            }

            // Aplica seleção de métricas: só preenche o que estiver marcado
            PrevN = SelN ? Avg(s => s.Nitrogenio, samples) : null;
            PrevP = SelP ? Avg(s => s.Fosforo, samples) : null;
            PrevK = SelK ? Avg(s => s.Potassio, samples) : null;
            PrevPH = SelPH ? Avg(s => s.PH, samples) : null;
            PrevCE = SelCE ? Avg(s => s.CondutividadeEletrica, samples) : null;
            PrevT = SelT ? Avg(s => s.TemperaturaC, samples) : null;
            PrevU = SelU ? Avg(s => s.Umidade, samples) : null;

            HasPreview = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Falha ao buscar dados locais: {ex.Message}";
            await Shell.Current.DisplayAlert("Erro", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
            SaveCommand.NotifyCanExecuteChanged();
        }
    }


    // ======== SALVAR ========

    public bool CanSave => HasPreview && SelectedTalhao is not null;

    partial void OnHasPreviewChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnSelectedTalhaoChanged(Talhao? value) => SaveCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        if (SelectedTalhao is null) return;

        var model = new Historico
        {
            Id = _id,
            TalhaoId = SelectedTalhao.Id,
            DataColeta = DataColeta,
            HoraInicioMin = (int)InicioTime.TotalMinutes,
            HoraFimMin = (int)FimTime.TotalMinutes,

            Nitrogenio = PrevN,
            Fosforo = PrevP,
            Potassio = PrevK,
            PH = PrevPH,
            CondutividadeEletrica = PrevCE,
            TemperaturaC = PrevT,
            Umidade = PrevU
        };

        if (_id == 0)
            await _repo.CreateAsync(model);
        else
            await _repo.UpdateAsync(model);

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Cancel() => await Shell.Current.GoToAsync("..");
}
