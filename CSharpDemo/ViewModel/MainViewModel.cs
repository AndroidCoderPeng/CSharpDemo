using System.Collections.ObjectModel;
using System.Windows.Controls;
using CSharpDemo.Utils;
using CSharpDemo.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace CSharpDemo.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly string[] _functions = { "摄像头", "折线图", "波形图", "TCP服务端", "频率筛选", "圆形进度条" };

        #region VM

        public ObservableCollection<string> FunctionModels { get; } = new ObservableCollection<string>();

        #endregion

        #region RelayCommand

        public RelayCommand<ListBox> FunctionSelectedCommand { get; }

        #endregion

        public MainViewModel()
        {
            foreach (var function in _functions)
            {
                FunctionModels.Add(function);
            }

            FunctionSelectedCommand = new RelayCommand<ListBox>(it =>
            {
                if (it.SelectedIndex == -1) return;

                switch (it.SelectedIndex)
                {
                    case 0:
                        MainWindow.Service.Navigate("CameraPage".CreateUri());
                        break;
                    case 1:
                        MainWindow.Service.Navigate("LiveChartsPage".CreateUri());
                        break;
                    case 2:
                        MainWindow.Service.Navigate("ScottPlotPage".CreateUri());
                        break;
                    case 3:
                        MainWindow.Service.Navigate("TcpServerPage".CreateUri());
                        break;
                    case 4:
                        MainWindow.Service.Navigate("FrequencyPage".CreateUri());
                        break;
                    case 5:
                        MainWindow.Service.Navigate("CircleLoadingPage".CreateUri());
                        break;
                }
            });
        }
    }
}