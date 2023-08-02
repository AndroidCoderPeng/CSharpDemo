using System.Windows;
using System.Windows.Controls;
using CSharpDemo.Dialogs;
using Prism.Commands;
using Prism.Mvvm;

namespace CSharpDemo.ViewModels
{
    public class TransmitValueViewModel : BindableBase
    {
        #region VM

        private string _delegateValue;

        public string DelegateValue
        {
            get => _delegateValue;
            set
            {
                _delegateValue = value;
                RaisePropertyChanged();
            }
        }

        private string _eventValue;

        public string EventValue
        {
            get => _eventValue;
            set
            {
                _eventValue = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region DelegateCommand

        public DelegateCommand<UserControl> DelegateCommand { get; }
        public DelegateCommand<UserControl> EventCommand { get; }

        #endregion

        /// <summary>
        /// 委托：
        /// 1、什么页面需要值，就在此页面定义传值函数
        /// 2、在另一个页面定义委托，把值给委托
        /// </summary>
        public TransmitValueViewModel()
        {
            DelegateCommand = new DelegateCommand<UserControl>(delegate(UserControl control)
            {
                var dialog = new DelegateValueDialog(ShowDelegateValue)
                {
                    Owner = Window.GetWindow(control)
                };
                dialog.ShowDialog();
            });

            EventCommand = new DelegateCommand<UserControl>(delegate(UserControl control)
            {
                var dialog = new EventValueDialog
                {
                    Owner = Window.GetWindow(control)
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