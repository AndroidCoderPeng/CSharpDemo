using System.Collections.ObjectModel;
using CSharpDemo.Service;
using GalaSoft.MvvmLight;

namespace CSharpDemo.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region VM

        public ObservableCollection<string> FunctionModels { get; }

        #endregion

        public MainViewModel(IMainDataService dataService)
        {
            FunctionModels = dataService.GetFunctionModels();
        }
    }
}