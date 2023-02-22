using System;
using System.Windows.Threading;
using CSharpDemo.Pages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HandyControl.Tools.Extension;

namespace CSharpDemo.ViewModel
{
    public class CircleLoadingViewModel : ViewModelBase
    {
        #region VM

        private double _processValue;

        public double ProcessValue
        {
            get => _processValue;
            private set
            {
                _processValue = value;
                RaisePropertyChanged(() => ProcessValue);
            }
        }

        #endregion

        #region RelayCommand

        public RelayCommand<CircleLoadingPage> InventoryCommand { get; }

        #endregion

        private readonly DispatcherTimer _inventoryTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1)
        };

        private int _countDownTime = 1500;
        private CircleLoadingPage _page;
        
        public CircleLoadingViewModel()
        {
            InventoryCommand = new RelayCommand<CircleLoadingPage>(it =>
            {
                _page = it;

                _inventoryTimer.Start();
                _page.ProgressBarPanel.Show();
                _page.GridView.Hide();
            });

            _inventoryTimer.Tick += delegate
            {
                if (_countDownTime > 0)
                {
                    _countDownTime--;

                    ProcessValue += 0.01;
                }
                else
                {
                    _inventoryTimer.Stop();
                    _page.ProgressBarPanel.Hide();
                    _page.GridView.Show();
                    _countDownTime = 1500;
                    ProcessValue = 0;
                }
            };
        }
    }
}