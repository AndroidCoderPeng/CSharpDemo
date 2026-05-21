using System.Collections.Generic;
using CSharpDemo.Service;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace CSharpDemo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region VM

        public List<string> ItemModels { get; }

        #endregion

        #region DelegateCommand

        public DelegateCommand<object> ItemSelectedCommand { get; set; }

        #endregion

        private readonly IRegionManager _regionManager;

        public MainWindowViewModel(IRegionManager regionManager, IAppDataService dataService)
        {
            _regionManager = regionManager;

            ItemModels = dataService.GetItemModels();

            ItemSelectedCommand = new DelegateCommand<object>(OnListItemSelected);
        }

        private void OnListItemSelected(object index)
        {
            if (index == null)
            {
                return;
            }

            var region = _regionManager.Regions["ContentRegion"];
            switch (index)
            {
                case 0:
                    region.RequestNavigate("AlgorithmTestView");
                    break;
                case 1:
                    region.RequestNavigate("AudioCaptureView");
                    break;
                case 2:
                    region.RequestNavigate("AudioAnalyzerView");
                    break;
                case 3:
                    region.RequestNavigate("SerialPortView");
                    break;
            }
        }
    }
}