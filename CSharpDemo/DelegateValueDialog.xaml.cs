using System.Windows;

namespace CSharpDemo.Dialogs
{
    /// <summary>
    /// DelegateDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DelegateValueDialog : Window
    {
        /// <summary>
        /// 窗口传值委托
        /// </summary>
        public delegate void TransferValueDelegateHandler(string value);

        public DelegateValueDialog(TransferValueDelegateHandler delegateHandler)
        {
            InitializeComponent();

            DelegateButton.Click += delegate
            {
                delegateHandler(DelegateTextBox.Text);
                Close();
            };
        }
    }
}