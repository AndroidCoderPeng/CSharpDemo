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
        public delegate void TransferValueEventHandler(string value);

        public DelegateValueDialog(TransferValueEventHandler eventHandler)
        {
            InitializeComponent();

            DelegateButton.Click += delegate
            {
                eventHandler(DelegateTextBox.Text);
                Close();
            };
        }
    }
}