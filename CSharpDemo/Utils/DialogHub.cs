using System;
using System.Windows;
using CSharpDemo.Dialogs;

namespace CSharpDemo.Utils
{
    public class DialogHub
    {
        #region 懒汉单例模式

        private static readonly Lazy<DialogHub> Lazy = new Lazy<DialogHub>(() => new DialogHub());

        public static DialogHub Get => Lazy.Value;


        private DialogHub()
        {
        }

        #endregion

        private LoadingDialog _loadingDialog;

        public void ShowLoadingDialog(Window owner, string message)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                _loadingDialog = new LoadingDialog(message)
                {
                    Owner = owner
                };
                _loadingDialog.Show();
            });
        }

        public void DismissLoadingDialog()
        {
            Application.Current.Dispatcher.Invoke(delegate { _loadingDialog?.Close(); });
        }
    }
}