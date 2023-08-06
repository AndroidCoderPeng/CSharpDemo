using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using CSharpDemo.Events;
using CSharpDemo.Utils;
using MathWorks.MATLAB.NET.Arrays;
using Prism.Events;
using ScottPlot;
using ScottPlot.Plottable;

namespace CSharpDemo.Views
{
    public partial class DataAnalysisView : UserControl
    {
        public DataAnalysisView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            var scottPlot = ScottPlotChart.Plot;
            scottPlot.XLabel("频率(Hz)");
            scottPlot.YLabel("相关系数");
            ScottPlotChart.Refresh();

            eventAggregator.GetEvent<CalculateResultEvent>().Subscribe(delegate(MWArray[] array)
            {
                Debug.WriteLine("DataAnalysisView.xaml => 开始渲染波形图");
                var xDoubles = ((MWNumericArray)array[5]).GetArray();
                var yDoubles = ((MWNumericArray)array[4]).GetArray();
                Application.Current.Dispatcher.Invoke(delegate
                {
                    //点如果较少，可以直接AddBar，但是超过一千个点，不能直接AddBar，否则Bar颜色会被边框覆盖从而呈现黑色
                    scottPlot.Add(new BarPlot(DataGen.Consecutive(xDoubles.Length), yDoubles, null, null)
                    {
                        FillColor = Color.FromArgb(255, 49, 151, 36),
                        BorderColor = Color.FromArgb(255, 49, 151, 36),
                        BorderLineWidth = 0.1f
                    });
                    ScottPlotChart.Refresh();
                });
            });
        }
    }
}