using System.Windows;

namespace CSharpDemo.Views
{
    /// <summary>
    /// HikVisionLoginDialog.xaml 的交互逻辑
    /// </summary>
    public partial class HikVisionLoginWindow : Window
    {
        /// <summary>
        /// 窗口传值委托
        /// </summary>
        public delegate void TransferValueDelegateHandler(string host, string name, string port, string password);

        public HikVisionLoginWindow(TransferValueDelegateHandler delegateHandler)
        {
            InitializeComponent();

            CancelButton.Click += delegate { Close(); };

            LoginButton.Click += delegate
            {
                if (string.IsNullOrEmpty(HostAddressTextBox.Text) || string.IsNullOrEmpty(UserNameTextBox.Text) ||
                    string.IsNullOrEmpty(HostPortTextBox.Text) || string.IsNullOrEmpty(DevicePasswordTextBox.Password))
                {
                    MessageBox.Show("参数输入错误，请检查", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                delegateHandler(
                    HostAddressTextBox.Text, UserNameTextBox.Text, HostPortTextBox.Text, DevicePasswordTextBox.Password
                );
                Close();
            };
        }
    }
}