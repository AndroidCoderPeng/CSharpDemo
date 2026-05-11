using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using CSharpDemo.Events;
using CSharpDemo.Utils;
using MathWorks.MATLAB.NET.Arrays;
using Prism.Events;
using ScottPlot;
using Color = ScottPlot.Color;
using Colors = ScottPlot.Colors;

namespace CSharpDemo.Views
{
    public partial class AlgorithmTestView : UserControl
    {
        public AlgorithmTestView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            // 禁用缩放
            ScottPlotView.UserInputProcessor.Disable();

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

            //XY轴坐标
            ScottPlotView.Plot.XLabel("Pipe Length(m)");
            ScottPlotView.Plot.YLabel("Correlation Coefficient");

            BindCrosshair();

            eventAggregator.GetEvent<CorrelatorResultEvent>().Subscribe(delegate(MWArray[] array)
            {
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
            }, ThreadOption.UIThread);
        }

        private void BindCrosshair()
        {
            var crosshair = ScottPlotView.Plot.Add.Crosshair(0, 0);
            crosshair.LineColor = Colors.Red;
            crosshair.HorizontalLine.LinePattern = LinePattern.Dotted;
            crosshair.VerticalLine.LinePattern = LinePattern.Dotted;
            crosshair.IsVisible = false;
            ScottPlotView.Refresh();

            ShowCrossLineCheckBox.Checked += delegate
            {
                crosshair.IsVisible = true;
                ScottPlotView.Refresh();
            };

            ShowCrossLineCheckBox.Unchecked += delegate
            {
                crosshair.IsVisible = false;
                ScottPlotView.Refresh();
            };

            //鼠标进入
            ScottPlotView.MouseEnter += delegate
            {
                crosshair.IsVisible = ShowCrossLineCheckBox.IsChecked == true;
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

                    crosshair.X = coordinates.X;
                    crosshair.Y = coordinates.Y;
                    ScottPlotView.Refresh();
                }

                ScottPlotView.Refresh();
            };

            //鼠标离开
            ScottPlotView.MouseLeave += delegate
            {
                crosshair.IsVisible = false;
                ScottPlotView.Refresh();
            };
        }
    }
}