using System.Linq;
using System.Windows.Controls;
using CSharpDemo.Events;
using CSharpDemo.Utils;
using MathWorks.MATLAB.NET.Arrays;
using Prism.Events;
using ScottPlot;

namespace CSharpDemo.Views
{
    public partial class AlgorithmTestView : UserControl
    {
        public AlgorithmTestView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            // 禁用缩放
            ScottplotView.UserInputProcessor.Disable();

            // 禁用网格线
            var scottPlot = ScottplotView.Plot;
            scottPlot.HideGrid();

            //XY轴坐标
            ScottplotView.Plot.XLabel("Pipe Length(m)");
            ScottplotView.Plot.YLabel("Correlation Coefficient");

            BindCrosshair();

            eventAggregator.GetEvent<CorrelatorResultEvent>().Subscribe(delegate(MWArray[] array)
            {
                //柱状图横坐标集合
                var xDoubles = ((MWNumericArray)array[5]).GetArray();

                //柱状图纵坐标集合
                var yDoubles = ((MWNumericArray)array[4]).GetArray();

                var scatter = ScottplotView.Plot.Add.Scatter(xDoubles, yDoubles);
                scatter.Color = new Color(49, 151, 36);
                scatter.LineWidth = 1;
                scatter.MarkerStyle.IsVisible = false;

                var baseline = ScottplotView.Plot.Add.Scatter(
                    xDoubles,
                    Enumerable.Repeat(0.0, xDoubles.Length).ToArray());
                baseline.Color = Colors.Transparent;
                baseline.MarkerStyle.IsVisible = false;
                baseline.LineStyle.IsVisible = false;

                var fillY = ScottplotView.Plot.Add.FillY(scatter, baseline);
                fillY.FillColor = new Color(49, 151, 36);
                fillY.LineStyle.IsVisible = false;

                // 数据会自动自动缩放至最合适的视角
                ScottplotView.Plot.Axes.Margins(0.05f, 0.05f);
                ScottplotView.Refresh();
            }, ThreadOption.UIThread);
        }

        private void BindCrosshair()
        {
            // 禁用十字准线
            var crosshair = ScottplotView.Plot.Add.Crosshair(0, 0);
            crosshair.LineColor = Colors.Red;
            crosshair.IsVisible = false;
            ScottplotView.Refresh();

            ShowCrossLineCheckBox.Checked += delegate
            {
                crosshair.IsVisible = true;
                ScottplotView.Refresh();
            };

            ShowCrossLineCheckBox.Unchecked += delegate
            {
                crosshair.IsVisible = false;
                ScottplotView.Refresh();
            };

            //鼠标进入
            ScottplotView.MouseEnter += delegate
            {
                crosshair.IsVisible = ShowCrossLineCheckBox.IsChecked == true;
                ScottplotView.Refresh();
            };

            //鼠标移动
            ScottplotView.MouseMove += (sender, e) =>
            {
                if (ShowCrossLineCheckBox.IsChecked == true)
                {
                    var p = e.GetPosition(ScottplotView);
                    var mousePixel = new Pixel(p.X, p.Y);
                    var coordinates = ScottplotView.Plot.GetCoordinates(mousePixel);

                    crosshair.X = coordinates.X;
                    crosshair.Y = coordinates.Y;
                    ScottplotView.Refresh();
                }

                ScottplotView.Refresh();
            };

            //鼠标离开
            ScottplotView.MouseLeave += delegate
            {
                crosshair.IsVisible = false;
                ScottplotView.Refresh();
            };
        }
    }
}