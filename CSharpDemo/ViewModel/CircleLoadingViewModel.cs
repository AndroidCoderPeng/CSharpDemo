using System;
using System.Windows;
using System.Windows.Threading;
using CSharpDemo.Pages;
using CSharpDemo.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace CSharpDemo.ViewModel
{
    public class CircleLoadingViewModel : ViewModelBase
    {
        private readonly DispatcherTimer _inventoryTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1)
        };

        private double _processValue;

        public double ProcessValue
        {
            get => _processValue;
            set
            {
                _processValue = value;
                RaisePropertyChanged(() => ProcessValue);
            }
        }

        private int _countDownTime = 1500;

        public CircleLoadingViewModel()
        {
            Messenger.Default.Register<CircleLoadingPage>(this, MessengerToken.StartLoading, it =>
            {
                _inventoryTimer.Start();
                _inventoryTimer.Tick += delegate
                {
                    if (_countDownTime > 0)
                    {
                        _countDownTime--;

                        _processValue += 0.01;
                        ProcessValue = _processValue;
                    }
                    else
                    {
                        _inventoryTimer.Stop();
                        it.ProgressBar.Visibility = Visibility.Collapsed;
                        it.GoodsGrid.Visibility = Visibility.Visible;
                    }
                };
            });
        }
    }
}