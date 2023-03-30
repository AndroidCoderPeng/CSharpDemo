using System.Windows;

namespace CSharpDemo.Dialogs
{
    /// <summary>
    /// DelegateDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DelegateDialog : Window
    {
        /// <summary>
        /// 窗口传值委托
        /// </summary>
        public delegate void TransferValueEventHandler(string value);

        public DelegateDialog(TransferValueEventHandler eventHandler)
        {
            InitializeComponent();

            DelegateButton.Click += delegate { eventHandler(DelegateTextBox.Text); };
        }
    }
}