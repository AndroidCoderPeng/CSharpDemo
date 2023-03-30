using System.Collections.ObjectModel;

namespace CSharpDemo.Service
{
    public interface IMainDataService
    {
        ObservableCollection<string> GetFunctionModels();
    }
}