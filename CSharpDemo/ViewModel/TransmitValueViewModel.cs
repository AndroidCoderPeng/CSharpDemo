using System.Windows;
using System.Windows.Controls;
using CSharpDemo.Dialogs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace CSharpDemo.ViewModel
{
    public class TransmitValueViewModel : ViewModelBase
    {
        #region VM

        private string _delegateValue;

        public string DelegateValue
        {
            get => _delegateValue;
            set
            {
                _delegateValue = value;
                RaisePropertyChanged(() => DelegateValue);
            }
        }

        private string _eventValue;

        public string EventValue
        {
            get => _eventValue;
            set
            {
                _eventValue = value;
                RaisePropertyChanged(() => EventValue);
            }
        }

        #endregion

        #region RelayCommand

        public RelayCommand<Page> DelegateCommand { get; }
        public RelayCommand<Page> EventCommand { get; }

        #endregion

        /// <summary>
        /// 委托：
        /// 1、什么页面需要值，就在此页面定义传值函数
        /// 2、在另一个页面定义委托，把值给委托
        /// </summary>
        public TransmitValueViewModel()
        {
            DelegateCommand = new RelayCommand<Page>(it =>
            {
                var dialog = new DelegateValueDialog(ShowDelegateValue)
                {
                    Owner = Window.GetWindow(it)
                };
                dialog.ShowDialog();
            });

            EventCommand = new RelayCommand<Page>(it =>
            {
                var dialog = new EventValueDialog
                {
                    Owner = Window.GetWindow(it)
                };
                dialog.ValueChangedEventHandler += Dialog_TextChangedEventHandler;
                dialog.ShowDialog();
            });
        }

        private void ShowDelegateValue(string value)
        {
            DelegateValue = value;
        }

        private void Dialog_TextChangedEventHandler(object sender, string value)
        {
            EventValue = value;
        }
    }
}