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