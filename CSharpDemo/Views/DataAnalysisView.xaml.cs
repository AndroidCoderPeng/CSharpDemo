using System.Windows.Controls;

namespace CSharpDemo.Views
{
    public partial class DataAnalysisView : UserControl
    {
        public DataAnalysisView()
        {
            InitializeComponent();

            var configuration = ScottplotView.Configuration;
            //禁用XY缩放
            configuration.LockHorizontalAxis = true;
            configuration.LockVerticalAxis = true;
            configuration.ScrollWheelZoom = false;
            configuration.RightClickDragZoom = false;
            var scottPlot = ScottplotView.Plot;
            //去掉网格线
            scottPlot.Grid(false);
            //十字准线
            var crosshair = scottPlot.AddCrosshair(0, 0);

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
            ScottplotView.MouseMove += delegate
            {
                if (ShowCrossLineCheckBox.IsChecked == true)
                {
                    var (coordinateX, coordinateY) = ScottplotView.GetMouseCoordinates();
                    crosshair.X = coordinateX;
                    crosshair.Y = coordinateY;
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