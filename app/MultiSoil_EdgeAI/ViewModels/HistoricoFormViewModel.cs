// ViewModels/HistoricoFormViewModel.cs
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiSoil_EdgeAI.Interfaces;
using MultiSoil_EdgeAI.Models;

namespace MultiSoil_EdgeAI.ViewModels;

public partial class HistoricoFormViewModel : ObservableObject, IQueryAttributableVm
{
    private readonly IHistoricoRepository _repo;
    private readonly ITalhaoRepository _talhoesRepo;
    private readonly ISensorReadingService _sensorService;

    private int _id;           // se >0 edita (carrega valores salvos)
    private int _prefTalhaoId; // talhão preferido (quando vindo da lista)

    public ObservableCollection<Talhao> Talhoes { get; } = new();

    [ObservableProperty] private Talhao? selectedTalhao;
    [ObservableProperty] private DateTime dataColeta = DateTime.Today;
    [ObservableProperty] private TimeSpan inicioTime = TimeSpan.FromHours(8);
    [ObservableProperty] private TimeSpan fimTime = TimeSpan.FromHours(9);

    // Seleção de métricas
    [ObservableProperty] private bool selN = true;
    [ObservableProperty] private bool selP = true;
    [ObservableProperty] private bool selK = true;
    [ObservableProperty] private bool selPH = true;
    [ObservableProperty] private bool selCE = true;
    [ObservableProperty] private bool selT = true;
    [ObservableProperty] private bool selU = true;

    // Prévia
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
        ISensorReadingService sensorService)
    {
        _repo = repo;
        _talhoesRepo = talhoesRepo;
        _sensorService = sensorService;
    }

    public async Task OnAppearing()
    {
        await LoadTalhoesAsync();

        if (_id > 0)
        {
            // Edição: carrega o registro
            var m = await _repo.GetByIdAsync(_id);
            if (m is not null)
            {
                SelectedTalhao = Talhoes.FirstOrDefault(t => t.Id == m.TalhaoId);
                DataColeta = m.DataColeta.Date;
                InicioTime = TimeSpan.FromMinutes(m.HoraInicioMin);
                FimTime = TimeSpan.FromMinutes(m.HoraFimMin);

                PrevN = m.Nitrogenio; PrevP = m.Fosforo; PrevK = m.Potassio;
                PrevPH = m.PH; PrevCE = m.CondutividadeEletrica;
                PrevT = m.TemperaturaC; PrevU = m.Umidade;

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
        foreach (var t in list) Talhoes.Add(t);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && idObj is string idStr && int.TryParse(idStr, out var id))
            _id = id;

        if (query.TryGetValue("talhaoId", out var tObj) && tObj is string tStr && int.TryParse(tStr, out var tid))
            _prefTalhaoId = tid;
    }

    private SensorMetric BuildSelection()
    {
        SensorMetric m = SensorMetric.None;
        if (SelN) m |= SensorMetric.N;
        if (SelP) m |= SensorMetric.P;
        if (SelK) m |= SensorMetric.K;
        if (SelPH) m |= SensorMetric.PH;
        if (SelCE) m |= SensorMetric.CE;
        if (SelT) m |= SensorMetric.Temp;
        if (SelU) m |= SensorMetric.Umid;
        return m;
    }

    [RelayCommand]
    private async Task Buscar()
    {
        ErrorMessage = null;
        HasPreview = false;

        if (SelectedTalhao is null)
        {
            ErrorMessage = "Selecione um talhão.";
            return;
        }
        var sel = BuildSelection();
        if (sel == SensorMetric.None)
        {
            ErrorMessage = "Selecione ao menos uma métrica.";
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _sensorService.GetReadingsAsync(
                SelectedTalhao.Id, DataColeta, InicioTime, FimTime, sel);

            if (result is null)
            {
                ErrorMessage = "Não há dados no servidor para o período selecionado.";
                return;
            }

            // Preenche somente o que foi solicitado (o service já retorna null para o não solicitado)
            PrevN = result.N;
            PrevP = result.P;
            PrevK = result.K;
            PrevPH = result.PH;
            PrevCE = result.CE;
            PrevT = result.Temp;
            PrevU = result.Umid;

            HasPreview = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Falha ao buscar dados: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

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

            // >>> PREVIEW É double?; MODEL TAMBÉM É double?
            Nitrogenio = PrevN,
            Fosforo = PrevP,
            Potassio = PrevK,
            PH = PrevPH,
            CondutividadeEletrica = PrevCE,
            TemperaturaC = PrevT,
            Umidade = PrevU
        };
        if (_id == 0) await _repo.CreateAsync(model);
        else await _repo.UpdateAsync(model);

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Cancel() => await Shell.Current.GoToAsync("..");
}
