using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CSharpDemo.Pages
{
    public partial class CircleLoadingPage : Page
    {
        private readonly DispatcherTimer _inventoryTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        private double _processValue;

        private int _countDownTime = 15;

        public CircleLoadingPage()
        {
            InitializeComponent();

            _inventoryTimer.Start();
            _inventoryTimer.Tick += delegate
            {
                if (_countDownTime > 0)
                {
                    _countDownTime--;
                    ProgressBar.Value = ++_processValue;
                }
                else
                {
                    _inventoryTimer.Stop();
                    ProgressBar.Visibility = Visibility.Collapsed;
                    GoodsGrid.Visibility = Visibility.Visible;
                }
            };
        }
    }
}