using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using CSharpDemo.Events;
using CSharpDemo.Model;
using Prism.Events;

namespace CSharpDemo.Views
{
    public partial class RealTimeAudioView : UserControl
    {
        public RealTimeAudioView(IEventAggregator aggregator)
        {
            InitializeComponent();

            var redSensorPlot = RedSensorScottPlotChart.Plot;
            //去掉网格线
            redSensorPlot.Grid(false);
            //去掉四周坐标轴
            redSensorPlot.Frameless();

            var blueSensorPlot = BlueSensorScottPlotChart.Plot;
            //去掉网格线
            blueSensorPlot.Grid(false);
            //去掉四周坐标轴
            blueSensorPlot.Frameless();

            aggregator.GetEvent<AudioWavePointEvent>().Subscribe(delegate(AudioWaveModel model)
            {
                if (model.IsRedSensor)
                {
                    RedSensorScottPlotChart.Plot.Clear();
                    RedSensorScottPlotChart.Refresh();

                    var xDoubles = new List<double>();
                    var yDoubles = new List<double>();

                    for (var i = 0; i < model.WavePoints.Length; i++)
                    {
                        xDoubles.Add(i);
                        yDoubles.Add(model.WavePoints[i]);
                    }

                    RedSensorScottPlotChart.Plot.AddSignalXY(xDoubles.ToArray(), yDoubles.ToArray(), Color.LimeGreen);
                    RedSensorScottPlotChart.Refresh();
                }
                else
                {
                    BlueSensorScottPlotChart.Plot.Clear();
                    BlueSensorScottPlotChart.Refresh();

                    var xDoubles = new List<double>();
                    var yDoubles = new List<double>();

                    for (var i = 0; i < model.WavePoints.Length; i++)
                    {
                        xDoubles.Add(i);
                        yDoubles.Add(model.WavePoints[i]);
                    }

                    BlueSensorScottPlotChart.Plot.AddSignalXY(xDoubles.ToArray(), yDoubles.ToArray(), Color.LimeGreen);
                    BlueSensorScottPlotChart.Refresh();
                }
            }, ThreadOption.UIThread);
        }
    }
}