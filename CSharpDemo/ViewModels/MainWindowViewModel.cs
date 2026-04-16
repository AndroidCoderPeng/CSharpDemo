using System.Collections.Generic;
using System.Windows.Controls;
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

        public DelegateCommand<ListBox> ItemSelectedCommand { set; get; }

        #endregion

        private readonly IRegionManager _regionManager;

        public MainWindowViewModel(IRegionManager regionManager, IAppDataService dataService)
        {
            _regionManager = regionManager;

            ItemModels = dataService.GetItemModels();

            ItemSelectedCommand = new DelegateCommand<ListBox>(OnListItemSelected);
        }

        private void OnListItemSelected(ListBox box)
        {
            var region = _regionManager.Regions["ContentRegion"];
            switch (box.SelectedIndex)
            {
                case 0:
                    region.RequestNavigate("SerialPortView");
                    break;
                case 1:
                    region.RequestNavigate("AudioWaveView");
                    break;
                case 2:
                    region.RequestNavigate("AlgorithmTestView");
                    break;
            }
        }
    }
}