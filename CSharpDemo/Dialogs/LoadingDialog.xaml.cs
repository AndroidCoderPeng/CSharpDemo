using System;
using System.Windows;
using System.Windows.Threading;

namespace CSharpDemo.Dialogs
{
    public partial class LoadingDialog : Window
    {
        //运行时间Timer
        private readonly DispatcherTimer _runningTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        //计算时间
        private int _runningSeconds;

        public LoadingDialog(string message)
        {
            InitializeComponent();
            _runningTimer.Tick += delegate
            {
                _runningSeconds++;
                Application.Current.Dispatcher.Invoke(delegate
                {
                    MessageTextBlock.Text = message + $"{_runningSeconds}s";
                });
            };
            _runningTimer.Start();
        }

        private void LoadingDialog_OnClosed(object sender, EventArgs e)
        {
            _runningTimer.Stop();
        }
    }
}