using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ScottPlot;

namespace CSharpDemo.Views
{
    public partial class AlgorithmTestView : UserControl
    {
        private string _redDataPath;
        private string _blueDataPath;
        private Window _loadingWindow;

        public AlgorithmTestView()
        {
            InitializeComponent();

            ImportRedDataButton.Click += (sender, args) =>
            {
                var fileDialog = new OpenFileDialog
                {
                    // 设置默认格式
                    DefaultExt = ".txt",
                    Filter = "水听器数据文件(*.txt)|*.txt"
                };
                if (fileDialog.ShowDialog() == true)
                {
                    _redDataPath = fileDialog.FileName;
                }
            };

            ImportBlueDataButton.Click += (sender, args) =>
            {
                var fileDialog = new OpenFileDialog
                {
                    // 设置默认格式
                    DefaultExt = ".txt",
                    Filter = "水听器数据文件(*.txt)|*.txt"
                };
                if (fileDialog.ShowDialog() == true)
                {
                    _blueDataPath = fileDialog.FileName;
                }
            };

            // 设置坐标轴
            ScottPlotView.Plot.XLabel("Pipe Length(m)");
            ScottPlotView.Plot.YLabel("Coefficient");
            
            // 禁用缩放
            ScottPlotView.UserInputProcessor.Disable();
            
            // 隐藏网格
            ScottPlotView.Plot.HideGrid();
            
            // 默认显示十字光标
            var crosshair = ScottPlotView.Plot.Add.Crosshair(0, 0);
            crosshair.IsVisible = true;
            crosshair.LineWidth = 1;
            crosshair.LineColor = Colors.Red;

            if (!crosshair.IsVisible)
            {
                crosshair.IsVisible = false;
            }

            CrossLineCheckBox.Checked += (sender, args) =>
            {
                crosshair.IsVisible = true;
                ScottPlotView.Refresh();
            };

            CrossLineCheckBox.Unchecked += (sender, args) =>
            {
                crosshair.IsVisible = false;
                ScottPlotView.Refresh();
            };

            ScottPlotView.MouseMove += (sender, args) =>
            {
                if (!crosshair.IsVisible) return;

                var mousePosition = args.GetPosition(ScottPlotView);
                var coordinates = ScottPlotView.Plot.GetCoordinates((float)mousePosition.X, (float)mousePosition.Y);
                crosshair.Position = coordinates;
                ScottPlotView.Refresh();
            };

            StartCalculateButton.Click += async (sender, args) =>
            {
                ShowLoadingDialog("数据计算中，请稍后...");

                // var result = await new TaskFactory<MWArray>().StartNew(() =>
                //     LazyLeak.Value.mainFunction(_redDataPath, _blueDataPath, 7500)
                // );
                //
                // Console.WriteLine(JsonConvert.SerializeObject(result));
                //
                // //最大相关系数  
                // var maxCorrelationCoefficient = Convert.ToDouble(result[3].ToString());
                //
                // var xDoubles = ((MWNumericArray)result[5]).GetArray();
                // var yDoubles = ((MWNumericArray)result[4]).GetArray();
                //
                // var timeDiff = Convert.ToDouble(result[6].ToString());
                // Console.WriteLine($@"时间差 => {timeDiff}");
                //
                // var chart = _view.ScottplotView;
                // chart.Plot.SetAxisLimits(0, xDoubles.Last(), 0, maxCorrelationCoefficient);
                // chart.Plot.AddFill(
                //     xDoubles, yDoubles,
                //     color: Color.FromArgb(255, 49, 151, 36),
                //     lineWidth: 0.1f,
                //     lineColor: Color.FromArgb(255, 49, 151, 36)
                // );
                //
                // chart.Plot.AxisAuto();
                // chart.Refresh();

                DismissLoadingDialog();
            };
        }

        private void ShowLoadingDialog(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var progressRing = new ProgressBar
                {
                    Height = 20,
                    Style = null,
                    IsIndeterminate = true,
                    Margin = new Thickness(0, 0, 0, 15)
                };

                var textBlock = new TextBlock
                {
                    Text = message,
                    FontSize = 14
                };

                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(30, 20, 30, 20),
                    Children = { progressRing, textBlock }
                };

                _loadingWindow = new Window
                {
                    Title = "加载中",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Window.GetWindow(this),
                    ResizeMode = ResizeMode.NoResize,
                    Content = stackPanel,
                    Style = null,
                    ShowInTaskbar = false
                };

                _loadingWindow.ShowDialog();
            });
        }

        private void DismissLoadingDialog()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_loadingWindow != null)
                {
                    _loadingWindow.Close();
                    _loadingWindow = null;
                }
            });
        }
    }
}