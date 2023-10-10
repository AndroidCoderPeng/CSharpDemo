using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using CSharpDemo.Events;
using Prism.Events;

namespace CSharpDemo.Views
{
    public partial class AudioFileToWaveView : UserControl
    {
        public AudioFileToWaveView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            //去掉四周坐标轴
            var scottPlot = ScottplotView.Plot;
            //去掉网格线
            scottPlot.Grid(false);
            //去掉四周坐标轴
            scottPlot.Frameless();

            eventAggregator.GetEvent<WavePointEvent>().Subscribe(delegate(List<double> doubles)
            {
                ScottplotView.Plot.AddSignal(
                    doubles.ToArray(), color: Color.FromArgb(255, 49, 151, 36)
                );
                ScottplotView.Refresh();
            });
        }
    }
}