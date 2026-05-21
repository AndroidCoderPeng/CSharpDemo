using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CSharpDemo.Events;
using CSharpDemo.Utils;
using MathWorks.MATLAB.NET.Arrays;
using Prism.Events;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.Plottables;
using Color = ScottPlot.Color;
using Colors = ScottPlot.Colors;

namespace CSharpDemo.Views
{
    public partial class AlgorithmTestView : UserControl
    {
        private Crosshair _crosshair;

        public AlgorithmTestView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            // 禁用缩放
            ScottPlotView.UserInputProcessor.Disable();
            FirstTdView.UserInputProcessor.Disable();
            SecondTdView.UserInputProcessor.Disable();
            FirstFdView.UserInputProcessor.Disable();
            SecondFdView.UserInputProcessor.Disable();
            FirstMelView.UserInputProcessor.Disable();
            SecondMelView.UserInputProcessor.Disable();

            // 网格线
            var scottPlot = ScottPlotView.Plot;
            ShowGridLineCheckBox.Checked += delegate
            {
                scottPlot.ShowGrid();
                ScottPlotView.Refresh();
            };

            ShowGridLineCheckBox.Unchecked += delegate
            {
                scottPlot.HideGrid();
                ScottPlotView.Refresh();
            };

            BindCrosshair();

            eventAggregator.GetEvent<CorrelatorResultEvent<MWArray[]>>()
                .Subscribe(ShowMatlabCalculateResult, ThreadOption.UIThread);

            eventAggregator.GetEvent<CorrelatorResultEvent<SensorDataWrapper>>().Subscribe(
                delegate(SensorDataWrapper wrapper)
                {
                    switch (wrapper.DataType)
                    {
                        case "TimeDomain":
                            ShowTimeDomain(wrapper.FirstSensor, wrapper.SecondSensor);
                            break;

                        case "FrequencyDomain":
                            ShowFrequencyDomain(wrapper.FirstSensor, wrapper.SecondSensor);
                            break;
                    }
                }, ThreadOption.UIThread);

            eventAggregator.GetEvent<CorrelatorResultEvent<(double[], double[,], double[,])>>()
                .Subscribe(ShowMelSpectrum, ThreadOption.UIThread);
        }

        private void BindCrosshair()
        {
            _crosshair = ScottPlotView.Plot.Add.Crosshair(0, 0);
            _crosshair.LineColor = Colors.Red;
            _crosshair.HorizontalLine.LinePattern = LinePattern.Dotted;
            _crosshair.VerticalLine.LinePattern = LinePattern.Dotted;
            _crosshair.IsVisible = false;
            ScottPlotView.Refresh();

            ShowCrossLineCheckBox.Checked += delegate
            {
                _crosshair.IsVisible = true;
                ScottPlotView.Refresh();
            };

            ShowCrossLineCheckBox.Unchecked += delegate
            {
                _crosshair.IsVisible = false;
                ScottPlotView.Refresh();
            };

            //鼠标进入
            ScottPlotView.MouseEnter += delegate
            {
                _crosshair.IsVisible = ShowCrossLineCheckBox.IsChecked == true;
                ScottPlotView.Refresh();
            };

            //鼠标移动
            ScottPlotView.MouseMove += (sender, e) =>
            {
                if (ShowCrossLineCheckBox.IsChecked == true)
                {
                    var p = e.GetPosition(ScottPlotView);

                    // 获取DPI缩放因子
                    var dpiScale = VisualTreeHelper.GetDpi(ScottPlotView);
                    var dpiX = dpiScale.PixelsPerInchX / 96.0;
                    var dpiY = dpiScale.PixelsPerInchY / 96.0;

                    // 转换为物理像素坐标
                    var pixelX = p.X * dpiX;
                    var pixelY = p.Y * dpiY;

                    // 使用Plot的坐标转换
                    var mousePixel = new Pixel(pixelX, pixelY);
                    var coordinates = ScottPlotView.Plot.GetCoordinates(mousePixel);

                    _crosshair.X = coordinates.X;
                    _crosshair.Y = coordinates.Y;
                    ScottPlotView.Refresh();
                }

                ScottPlotView.Refresh();
            };

            //鼠标离开
            ScottPlotView.MouseLeave += delegate
            {
                _crosshair.IsVisible = false;
                ScottPlotView.Refresh();
            };
        }

        private void ShowMatlabCalculateResult(MWArray[] array)
        {
            ScottPlotView.Visibility = Visibility.Visible;
            TdGrid.Visibility = Visibility.Collapsed;
            FdGrid.Visibility = Visibility.Collapsed;
            MelGrid.Visibility = Visibility.Collapsed;

            ScottPlotView.Plot.Clear();

            BindCrosshair();

            //XY轴坐标
            ScottPlotView.Plot.XLabel("Pipe Length(m)");
            ScottPlotView.Plot.YLabel("Correlation Coefficient");

            //柱状图横坐标集合
            var xDoubles = ((MWNumericArray)array[5]).GetArray();

            //柱状图纵坐标集合
            var yDoubles = ((MWNumericArray)array[4]).GetArray();

            var scatter = ScottPlotView.Plot.Add.Scatter(xDoubles, yDoubles);
            scatter.Color = new Color(49, 151, 36);
            scatter.LineWidth = 1;
            scatter.MarkerStyle.IsVisible = false;

            var baseline = ScottPlotView.Plot.Add.Scatter(
                xDoubles,
                Enumerable.Repeat(0.0, xDoubles.Length).ToArray());
            baseline.Color = Colors.Transparent;
            baseline.MarkerStyle.IsVisible = false;
            baseline.LineStyle.IsVisible = false;

            var fillY = ScottPlotView.Plot.Add.FillY(scatter, baseline);
            fillY.FillColor = new Color(49, 151, 36);
            fillY.LineStyle.IsVisible = false;

            // 数据会自动自动缩放至最合适的视角
            ScottPlotView.Plot.Axes.Margins(0.05f, 0.05f);
            ScottPlotView.Refresh();
        }

        private void ShowTimeDomain((double[], double[]) firstSensor, (double[], double[]) secondSensor)
        {
            ScottPlotView.Visibility = Visibility.Collapsed;
            TdGrid.Visibility = Visibility.Visible;
            FdGrid.Visibility = Visibility.Collapsed;
            MelGrid.Visibility = Visibility.Collapsed;

            Console.WriteLine(@"渲染时域图");

            // 传感器1
            FirstTdView.Plot.Clear();
            var firstPlot = FirstTdView.Plot.Add.SignalXY(firstSensor.Item1, firstSensor.Item2);
            firstPlot.LineColor = new Color(49, 151, 36);
            FirstTdView.Plot.XLabel("Time (s)");
            FirstTdView.Plot.YLabel("Amplitude");
            FirstTdView.Plot.Axes.Margins(0.05f, 0.05f);
            FirstTdView.Refresh();

            // 传感器2
            SecondTdView.Plot.Clear();
            var secondPlot = SecondTdView.Plot.Add.SignalXY(secondSensor.Item1, secondSensor.Item2);
            secondPlot.LineColor = new Color(49, 151, 36);
            SecondTdView.Plot.XLabel("Time (s)");
            SecondTdView.Plot.YLabel("Amplitude");
            SecondTdView.Plot.Axes.Margins(0.05f, 0.05f);
            SecondTdView.Refresh();
        }

        private void ShowFrequencyDomain((double[], double[]) firstSensor, (double[], double[]) secondSensor)
        {
            ScottPlotView.Visibility = Visibility.Collapsed;
            TdGrid.Visibility = Visibility.Collapsed;
            FdGrid.Visibility = Visibility.Visible;
            MelGrid.Visibility = Visibility.Collapsed;

            Console.WriteLine(@"渲染频域图");

            // 传感器1
            FirstFdView.Plot.Clear();
            var firstBars = new List<Bar>();
            for (var i = 0; i < firstSensor.Item1.Length; i++)
            {
                firstBars.Add(new Bar
                {
                    Position = firstSensor.Item1[i],
                    Value = firstSensor.Item2[i],
                    FillColor = new Color(49, 151, 36, 180),
                    LineColor = new Color(49, 151, 36),
                    LineWidth = 1
                });
            }

            FirstFdView.Plot.Add.Bars(firstBars);
            FirstFdView.Plot.XLabel("Frequency (Hz)");
            FirstFdView.Plot.YLabel("Magnitude");
            FirstFdView.Plot.Axes.Margins(0.05f, 0.05f);
            FirstFdView.Refresh();

            // 传感器2
            SecondFdView.Plot.Clear();
            var secondBars = new List<Bar>();
            for (var i = 0; i < secondSensor.Item1.Length; i++)
            {
                secondBars.Add(new Bar
                {
                    Position = secondSensor.Item1[i],
                    Value = secondSensor.Item2[i],
                    FillColor = new Color(49, 151, 36, 180),
                    LineColor = new Color(49, 151, 36),
                    LineWidth = 1
                });
            }

            SecondFdView.Plot.Add.Bars(secondBars);
            SecondFdView.Plot.XLabel("Frequency (Hz)");
            SecondFdView.Plot.YLabel("Magnitude");
            SecondFdView.Plot.Axes.Margins(0.05f, 0.05f);
            SecondFdView.Refresh();
        }

        private void ShowMelSpectrum((double[], double[,], double[,] ) melSpectrum)
        {
            ScottPlotView.Visibility = Visibility.Collapsed;
            TdGrid.Visibility = Visibility.Collapsed;
            FdGrid.Visibility = Visibility.Collapsed;
            MelGrid.Visibility = Visibility.Visible;

            Console.WriteLine(@"渲染梅尔频谱图");

            var timeAxis = melSpectrum.Item1;
            var firstSensor = melSpectrum.Item2;
            var secondSensor = melSpectrum.Item3;

            FirstMelView.Plot.Clear();
            var firstNumFrames = firstSensor.GetLength(0);
            var firstNumMelFilters = firstSensor.GetLength(1);
            var firstMelTransposed = new double[firstNumMelFilters, firstNumFrames];

            for (var i = 0; i < firstNumFrames; i++)
            {
                for (var j = 0; j < firstNumMelFilters; j++)
                {
                    firstMelTransposed[j, i] = firstSensor[i, j];
                }
            }

            var firstHeatmap = FirstMelView.Plot.Add.Heatmap(firstMelTransposed);
            firstHeatmap.Colormap = new Viridis();
            firstHeatmap.Extent = new CoordinateRect(
                left: timeAxis[0],
                right: timeAxis[firstNumFrames - 1],
                bottom: 0,
                top: firstNumMelFilters
            );
            FirstMelView.Plot.XLabel("Time (s)");
            FirstMelView.Plot.YLabel("Mel Frequency (Hz)");
            FirstMelView.Plot.Axes.Margins(0.05f, 0.05f);
            FirstMelView.Refresh();

            SecondMelView.Plot.Clear();
            var secondNumFrames = secondSensor.GetLength(0);
            var secondNumMelFilters = secondSensor.GetLength(1);
            var secondMelTransposed = new double[secondNumMelFilters, secondNumFrames];

            for (var i = 0; i < secondNumFrames; i++)
            {
                for (var j = 0; j < secondNumMelFilters; j++)
                {
                    secondMelTransposed[j, i] = secondSensor[i, j];
                }
            }

            var secondHeatmap = SecondMelView.Plot.Add.Heatmap(secondMelTransposed);
            secondHeatmap.Colormap = new Viridis();
            secondHeatmap.Extent = new CoordinateRect(
                left: timeAxis[0],
                right: timeAxis[secondNumFrames - 1],
                bottom: 0,
                top: secondNumMelFilters
            );
            SecondMelView.Plot.XLabel("Time (s)");
            SecondMelView.Plot.YLabel("Mel Frequency (Hz)");
            SecondMelView.Plot.Axes.Margins(0.05f, 0.05f);
            SecondMelView.Refresh();
        }
    }
}