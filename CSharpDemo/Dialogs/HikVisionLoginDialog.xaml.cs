using System.Windows;
using TouchSocket.Core;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CSharpDemo.Dialogs
{
    /// <summary>
    /// HikVisionLoginDialog.xaml 的交互逻辑
    /// </summary>
    public partial class HikVisionLoginDialog : Window
    {
        /// <summary>
        /// 窗口传值委托
        /// </summary>
        public delegate void TransferValueDelegateHandler(string host, string name, string port, string password);

        public HikVisionLoginDialog(TransferValueDelegateHandler delegateHandler)
        {
            InitializeComponent();
            LoginButton.Click += delegate
            {
                if (HostAddressTextBox.Text.IsNullOrEmpty() ||
                    UserNameTextBox.Text.IsNullOrEmpty() ||
                    HostPortTextBox.Text.IsNullOrEmpty() ||
                    DevicePasswordTextBox.Password.IsNullOrEmpty())
                {
                    MessageBox.Show("参数输入错误，请检查", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                delegateHandler(
                    HostAddressTextBox.Text, UserNameTextBox.Text,
                    HostPortTextBox.Text, DevicePasswordTextBox.Password
                );
                Close();
            };
        }

        private void HikVisionLoginDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            MouseDown += delegate { DragMove(); };
        }
    }
}