using System;
using GalaSoft.MvvmLight;
using LiveCharts;
using LiveCharts.Defaults;

namespace CSharpDemo.ViewModel
{
    public class LiveChartsViewModel : ViewModelBase
    {
        public IChartValues ColumnValuesB { get; set; } = new ChartValues<ObservablePoint>();

        public LiveChartsViewModel()
        {
            var random = new Random();
            for (var i = 0; i < 300; i++)
            {
                var p = new ObservablePoint
                {
                    X = i * 10,
                    Y = random.Next(0, 100)
                };

                ColumnValuesB.Add(p);
            }
        }
    }
}