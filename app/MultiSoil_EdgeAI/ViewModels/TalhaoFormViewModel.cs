using System.Windows.Input;
using MultiSoil_EdgeAI.Models;
using MultiSoil_EdgeAI.Services;


namespace MultiSoil_EdgeAI.ViewModels;


[QueryProperty(nameof(Id), "id")]
public class TalhaoFormViewModel : BaseViewModel
{
    private readonly ITalhaoService _service;

    public List<string> StatusOptions { get; } = new() { "Ativo", "Inativo" };

    public Talhao Item { get => _item; set => SetProperty(ref _item, value); }
    private Talhao _item = new();


    public int Id { get => _id; set { _id = value; _ = CarregarAsync(); } }
    private int _id;


    public ICommand SalvarCmd { get; }


    public TalhaoFormViewModel(ITalhaoService service)
    {
        _service = service;
        SalvarCmd = new Command(async () => await SalvarAsync());
    }


    private async Task CarregarAsync()
    {
        if (_id <= 0) { Item = new Talhao(); return; }
        var found = await _service.GetAsync(_id);
        Item = found ?? new Talhao();
    }


    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Item.Nome))
        {
            await App.Current.MainPage.DisplayAlert("Validação", "Informe o nome do talhão.", "OK");
            return;
        }


        if (Item.Id == 0) await _service.CreateAsync(Item);
        else await _service.UpdateAsync(Item);


        await Shell.Current.GoToAsync("..", true);
    }
}