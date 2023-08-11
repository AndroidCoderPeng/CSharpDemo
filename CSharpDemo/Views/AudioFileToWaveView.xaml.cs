using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using CSharpDemo.Events;
using Prism.Events;
using Point = System.Windows.Point;

namespace CSharpDemo.Views
{
    public partial class AudioFileToWaveView : UserControl
    {
        public AudioFileToWaveView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            //去掉四周坐标轴
            ScottPlotChart.Plot.XAxis.IsVisible = false;
            ScottPlotChart.Plot.XAxis2.IsVisible = false;
            ScottPlotChart.Plot.YAxis.IsVisible = false;
            ScottPlotChart.Plot.YAxis2.IsVisible = false;

            eventAggregator.GetEvent<WavePointEvent>().Subscribe(delegate(List<Point> list)
            {
                var xDoubles = new List<double>();
                var yDoubles = new List<double>();

                foreach (var wave in list)
                {
                    xDoubles.Add(wave.X);
                    yDoubles.Add(wave.Y);
                }

                ScottPlotChart.Plot.AddSignalXY(xDoubles.ToArray(), yDoubles.ToArray(), Color.LimeGreen);
                ScottPlotChart.Refresh();
            });
        }
    }
}