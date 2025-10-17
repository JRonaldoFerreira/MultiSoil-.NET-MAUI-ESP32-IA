// ViewModels/IQueryAttributableVm.cs
namespace MultiSoil_EdgeAI.ViewModels;

public interface IQueryAttributableVm
{
    void ApplyQueryAttributes(IDictionary<string, object> query);
}
