using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Accord.Math;
using CSharpDemo.Utils;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.Plottables;

namespace CSharpDemo.Views
{
    public partial class AlgorithmTestView : UserControl
    {
        private string _redDataPath;
        private string _blueDataPath;
        private Window _loadingWindow;

        // 管道参数
        private const double PipeLength = 300.0; // 管道长度(m)
        private const double WaveVelocity = 1480.0; // 波速(m/s)，水中声速
        private const double SamplingRate = 44100.0; // 采样率(Hz)

        // 绘图对象
        private Scatter _scatter;
        private Marker _peakMarker;
        private Text _peakLabel;

        // 频率表 (可以根据实际情况修改)
        private readonly double[] _frequencyTable =
        {
            50, 100, 150, 200, 250, 300, 350, 400, 450, 500,
            550, 600, 650, 700, 750, 800, 850, 900, 950, 1000,
            1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000,
            2200, 2400, 2600, 2800, 3000, 3200, 3400, 3600, 3800, 4000,
            4500, 5000, 5500, 6000, 6500, 7000, 7500, 8000
        };

        public AlgorithmTestView()
        {
            InitializeComponent();

            ImportRedDataButton.Click += (sender, args) =>
            {
                var fileDialog = new OpenFileDialog
                {
                    DefaultExt = ".txt",
                    Filter = "传感器数据文件(*.txt)|*.txt"
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
                    DefaultExt = ".txt",
                    Filter = "传感器数据文件(*.txt)|*.txt"
                };
                if (fileDialog.ShowDialog() == true)
                {
                    _blueDataPath = fileDialog.FileName;
                }
            };

            // 设置坐标轴
            ScottPlotView.Plot.XLabel("Frequency(Hz)");
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
                if (string.IsNullOrEmpty(_redDataPath) || string.IsNullOrEmpty(_blueDataPath))
                {
                    MessageBox.Show("请先导入两个传感器的数据文件!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ShowLoadingDialog("正在计算相关系数...");

                try
                {
                    await Task.Run(CalculateFrequencyCorrelation);
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"计算失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                finally
                {
                    DismissLoadingDialog();
                }
            };
        }

        private void CalculateFrequencyCorrelation()
        {
            // 1. 读取信号数据
            var redSignal = _redDataPath.ReadFromFile().Select(s => double.Parse(s.Trim())).ToArray();
            var blueSignal = _blueDataPath.ReadFromFile().Select(s => double.Parse(s.Trim())).ToArray();

            if (redSignal.Length != blueSignal.Length)
            {
                throw new InvalidOperationException("两个信号的长度不一致!");
            }

            Console.WriteLine($@"信号长度: {redSignal.Length} 采样点");
            Console.WriteLine($@"采样率: {SamplingRate} Hz");
            Console.WriteLine($@"频率表点数: {_frequencyTable.Length}");

            // 2. 使用Accord.Audio进行FFT变换
            var redSignalComplex = CreateComplexSignal(redSignal);
            var blueSignalComplex = CreateComplexSignal(blueSignal);

            // 执行FFT
            FourierTransform.FFT(redSignalComplex, FourierTransform.Direction.Forward);
            FourierTransform.FFT(blueSignalComplex, FourierTransform.Direction.Forward);

            Console.WriteLine(@"FFT变换完成");

            // 3. 对每个频率计算相干函数(Coherence)
            var frequencies = new double[_frequencyTable.Length];
            var correlations = new double[_frequencyTable.Length];

            int N = redSignal.Length;

            for (int i = 0; i < _frequencyTable.Length; i++)
            {
                double frequency = _frequencyTable[i];
                frequencies[i] = frequency;

                // 找到目标频率对应的bin索引
                int freqBin = (int)Math.Round(frequency * N / SamplingRate);

                if (freqBin >= N / 2) freqBin = N / 2 - 1;
                if (freqBin < 0) freqBin = 0;

                // 提取该频率的复数值
                Complex coeffA = redSignalComplex[freqBin];
                Complex coeffB = blueSignalComplex[freqBin];

                // 计算频域相关系数(归一化互功率谱/相干性)
                double magnitudeA = coeffA.Magnitude;
                double magnitudeB = coeffB.Magnitude;

                if (magnitudeA < 1e-10 || magnitudeB < 1e-10)
                {
                    correlations[i] = 0;
                }
                else
                {
                    // 归一化互相关: |X*Y| / (|X|*|Y|)
                    Complex crossSpectrum = coeffA * Complex.Conjugate(coeffB);
                    double coherence = crossSpectrum.Magnitude / (magnitudeA * magnitudeB);
                    correlations[i] = Math.Min(coherence, 1.0); // 限制在[0,1]范围
                }

                // 更新进度
                if (i % (_frequencyTable.Length / 10) == 0)
                {
                    Console.WriteLine($@"计算进度: {i * 100.0 / _frequencyTable.Length:F0}% (频率: {frequency}Hz)");
                }
            }

            // 4. 找到最大相关系数的频率
            int maxIndex = Array.IndexOf(correlations, correlations.Max());
            double peakFrequency = frequencies[maxIndex];
            double maxCorrelation = correlations[maxIndex];

            // 5. 根据最优频率计算泄漏位置
            double timeDelay = CalculateTimeDelayAtFrequency(redSignalComplex, blueSignalComplex, peakFrequency, N);
            double leakPosition = (PipeLength - WaveVelocity * timeDelay) / 2.0;

            // 确保位置在有效范围内
            leakPosition = Math.Max(0, Math.Min(PipeLength, leakPosition));

            Console.WriteLine(@"\n===== 频域分析结果 =====");
            Console.WriteLine($@"最优频率: {peakFrequency} Hz");
            Console.WriteLine($@"最大相关系数: {maxCorrelation:F6}");
            Console.WriteLine($@"时间差: {timeDelay:F6} s");
            Console.WriteLine($@"泄漏位置: 距离传感器A {leakPosition:F2}m");
            Console.WriteLine($@"置信度: {maxCorrelation:P2}");

            // 5. 在UI线程更新图表
            Application.Current.Dispatcher.Invoke(() =>
            {
                PlotFrequencyCorrelationCurve(frequencies, correlations, peakFrequency, maxCorrelation);
            });
        }

        /// <summary>
        /// 创建Accord.Audio的复数信号对象
        /// </summary>
        private Complex[] CreateComplexSignal(double[] signal)
        {
            var complexSignal = new Complex[signal.Length];

            for (int i = 0; i < signal.Length; i++)
            {
                complexSignal[i] = new Complex(signal[i], 0);
            }

            return complexSignal;
        }

        /// <summary>
        /// 计算指定频率下的时间延迟(通过相位差)
        /// </summary>
        private double CalculateTimeDelayAtFrequency(Complex[] fftA, Complex[] fftB, double frequency, int signalLength)
        {
            // 找到频率bin
            int freqBin = (int)Math.Round(frequency * signalLength / SamplingRate);
            if (freqBin >= signalLength / 2) freqBin = signalLength / 2 - 1;

            // 获取相位
            double phaseA = fftA[freqBin].Phase;
            double phaseB = fftB[freqBin].Phase;

            // 相位差
            double phaseDiff = phaseB - phaseA;

            // 解缠绕相位差到 [-π, π]
            while (phaseDiff > Math.PI) phaseDiff -= 2 * Math.PI;
            while (phaseDiff < -Math.PI) phaseDiff += 2 * Math.PI;

            // 转换为时间延迟: Δt = Δφ / (2πf)
            double timeDelay = phaseDiff / (2 * Math.PI * frequency);

            return timeDelay;
        }

        /// <summary>
        /// 绘制相关系数曲线
        /// </summary>
        private void PlotFrequencyCorrelationCurve(double[] frequencies, double[] correlations, 
            double peakFrequency, double maxCorrelation)
        {
            // 清除之前的绘图
            ScottPlotView.Plot.Clear();
            
            // 绘制相关系数曲线
            _scatter = ScottPlotView.Plot.Add.Scatter(frequencies, correlations);
            _scatter.LineWidth = 2;
            _scatter.Color = Colors.Blue;
            _scatter.LegendText = "频域相关系数";
            
            // 标记峰值点
            _peakMarker = ScottPlotView.Plot.Add.Marker(peakFrequency, maxCorrelation);
            _peakMarker.Size = 12;
            _peakMarker.Shape = MarkerShape.FilledCircle;
            _peakMarker.Color = Colors.Red;
            _peakMarker.LegendText = $"最优频率 ({peakFrequency}Hz)";
            
            // 添加峰值标注
            _peakLabel = ScottPlotView.Plot.Add.Text(
                $"频率: {peakFrequency}Hz\n相关系数: {maxCorrelation:F4}", 
                peakFrequency, maxCorrelation);
            _peakLabel.LabelFontSize = 12;
            _peakLabel.LabelFontColor = Colors.Red;
            _peakLabel.LabelAlignment = Alignment.UpperCenter;
            _peakLabel.OffsetY = -15;
            
            // 添加垂直参考线
            var vLine = ScottPlotView.Plot.Add.VerticalLine(peakFrequency);
            vLine.LineWidth = 1;
            vLine.Color = Colors.Red.WithAlpha(0.5);
            vLine.LinePattern = LinePattern.Dashed;
            
            // 重新添加十字光标
            var crosshair = ScottPlotView.Plot.Add.Crosshair(peakFrequency, maxCorrelation);
            crosshair.IsVisible = CrossLineCheckBox.IsChecked == true;
            crosshair.LineWidth = 1;
            crosshair.LineColor = Colors.Red;
            
            // 恢复鼠标移动事件
            ScottPlotView.MouseMove += (sender, args) =>
            {
                if (!crosshair.IsVisible) return;
                var mousePosition = args.GetPosition(ScottPlotView);
                var coordinates = ScottPlotView.Plot.GetCoordinates((float)mousePosition.X, (float)mousePosition.Y);
                crosshair.Position = coordinates;
                ScottPlotView.Refresh();
            };
            
            // 自动调整坐标轴
            ScottPlotView.Plot.Axes.Margins(0.05, 0.1);
            ScottPlotView.Refresh();
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