using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CSharpDemo.Utils;
using GalaSoft.MvvmLight.Messaging;

namespace CSharpDemo.Pages
{
    public partial class CircleLoadingPage : Page
    {
        private readonly DispatcherTimer _inventoryTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1)
        };

        private double _processValue;
        private int _countDownTime = 1500;

        public CircleLoadingPage()
        {
            InitializeComponent();

            ProgressBar.Visibility = Visibility.Visible;
            GoodsGrid.Visibility = Visibility.Collapsed;

            _inventoryTimer.Start();
            _inventoryTimer.Tick += delegate
            {
                if (_countDownTime > 0)
                {
                    _countDownTime--;

                    _processValue += 0.01;
                    Messenger.Default.Send(_processValue, MessengerToken.StartLoading);
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