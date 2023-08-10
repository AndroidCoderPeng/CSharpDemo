using System.Windows.Controls;

namespace CSharpDemo.Views
{
    public partial class DataAnalysisView : UserControl
    {
        public DataAnalysisView()
        {
            InitializeComponent();

            var scottPlot = ScottPlotChart.Plot;
            scottPlot.XLabel("频率(Hz)");
            scottPlot.YLabel("相关系数");
            ScottPlotChart.Refresh();
        }
    }
}