using System.Windows;
using System.Windows.Controls;
using CSharpDemo.Dialogs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace CSharpDemo.ViewModel
{
    public class DelegateValueViewModel : ViewModelBase
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

        #endregion

        #region RelayCommand

        public RelayCommand<Page> DelegateCommand { get; }

        #endregion

        /// <summary>
        /// 委托：
        /// 1、什么页面需要值，就在此页面定义传值函数
        /// 2、在另一个页面定义委托，把值给委托
        /// </summary>
        public DelegateValueViewModel()
        {
            DelegateCommand = new RelayCommand<Page>(it =>
            {
                var dialog = new DelegateDialog(ShowDelegateValue)
                {
                    Owner = Window.GetWindow(it)
                };
                dialog.ShowDialog();
            });
        }

        private void ShowDelegateValue(string value)
        {
            DelegateValue = value;
        }
    }
}