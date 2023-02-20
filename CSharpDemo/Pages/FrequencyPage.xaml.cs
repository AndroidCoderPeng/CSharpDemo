using System;
using System.Drawing;
using System.Windows.Controls;

namespace CSharpDemo.Pages
{
    public partial class FrequencyPage : Page
    {
        public FrequencyPage()
        {
            InitializeComponent();

            var random = new Random();
            var redDataY = new double[500];
            var blueDataY = new double[500];

            for (var i = 0; i < 500; i++)
            {
                redDataY[i] = random.NextDouble();

                blueDataY[i] = random.NextDouble();
            }

            RedScottplotView.Plot.AddSignal(redDataY, color: Color.FromArgb(255, 49, 151, 36));
            RedScottplotView.Refresh();

            BlueScottplotView.Plot.AddSignal(blueDataY, color: Color.FromArgb(255, 49, 151, 36));
            BlueScottplotView.Refresh();
        }
    }
}