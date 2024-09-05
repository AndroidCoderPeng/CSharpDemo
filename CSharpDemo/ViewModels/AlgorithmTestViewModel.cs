using System;
using System.Threading.Tasks;
using System.Windows;
using CSharpDemo.Utils;
using CSharpDemo.Views;
using MathWorks.MATLAB.NET.Arrays;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

namespace CSharpDemo.ViewModels
{
    public class AlgorithmTestViewModel : BindableBase
    {
        #region VM

        private string _redDataPath;

        public string RedDataPath
        {
            get => _redDataPath;
            set
            {
                _redDataPath = value;
                RaisePropertyChanged();
            }
        }

        private string _blueDataPath;

        public string BlueDataPath
        {
            get => _blueDataPath;
            set
            {
                _blueDataPath = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region DelegateCommand

        public DelegateCommand<AlgorithmTestView> ViewLoadedCommand { set; get; }
        public DelegateCommand ImportRedDataCommand { set; get; }
        public DelegateCommand ImportBlueDataCommand { set; get; }
        public DelegateCommand StartCalculateCommand { set; get; }

        #endregion

        private static readonly Lazy<Leak_location.Leak_location> LazyLeak =
            new Lazy<Leak_location.Leak_location>(() => new Leak_location.Leak_location());

        private AlgorithmTestView _view;

        public AlgorithmTestViewModel()
        {
            ViewLoadedCommand = new DelegateCommand<AlgorithmTestView>(delegate(AlgorithmTestView view)
            {
                _view = view;
            });

            ImportRedDataCommand = new DelegateCommand(ImportRedData);
            ImportBlueDataCommand = new DelegateCommand(ImportBlueData);
            StartCalculateCommand = new DelegateCommand(CalculateData);
        }

        private void ImportRedData()
        {
            var fileDialog = new OpenFileDialog
            {
                // 设置默认格式
                DefaultExt = ".txt",
                Filter = "水听器数据文件(*.txt)|*.txt"
            };
            if (fileDialog.ShowDialog() == true)
            {
                RedDataPath = fileDialog.FileName;
            }
        }

        private void ImportBlueData()
        {
            var fileDialog = new OpenFileDialog
            {
                // 设置默认格式
                DefaultExt = ".txt",
                Filter = "水听器数据文件(*.txt)|*.txt"
            };
            if (fileDialog.ShowDialog() == true)
            {
                BlueDataPath = fileDialog.FileName;
            }
        }

        /// <summary>
        /// 异步计算获得结果
        /// </summary>
        private async void CalculateData()
        {
            DialogHub.Get.ShowLoadingDialog(Window.GetWindow(_view), "数据计算中，请稍后...");
            var result = await new TaskFactory<MWArray>().StartNew(() =>
                LazyLeak.Value.mainFunction(_redDataPath, _blueDataPath, 7500)
            );

            Console.WriteLine(result.ToString());

            // var maxCorrelationCoefficient = Convert.ToDouble(result[3].ToString());
            // var xDoubles = ((MWNumericArray)result[5]).GetArray();
            // var yDoubles = ((MWNumericArray)result[4]).GetArray();
            // var timeDiff = Convert.ToDouble(result[6].ToString());
            // Console.WriteLine($@"时间差 => {timeDiff}");
            // var chart = _view.ScottplotView;
            // chart.Plot.SetAxisLimits(0, xDoubles.Last(), 0, maxCorrelationCoefficient);
            // chart.Plot.AddFill(
            //     xDoubles, yDoubles,
            //     color: Color.FromArgb(255, 49, 151, 36),
            //     lineWidth: 0.1f,
            //     lineColor: Color.FromArgb(255, 49, 151, 36)
            // );
            // chart.Plot.AxisAuto();
            // chart.Refresh();
            // DialogHub.Get.DismissLoadingDialog();
        }
    }
}