using System.Windows;
using HandyControl.Controls;
using HikVisionPreview;
using TouchSocket.Core;
using MessageBox = HandyControl.Controls.MessageBox;
using Window = System.Windows.Window;

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

            //初始化海康网络摄像头
            InitHikVisionSdk();

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

        private bool _initSdk;

        private void InitHikVisionSdk()
        {
            _initSdk = CHCNetSDK.NET_DVR_Init();
            if (!_initSdk)
            {
                MessageBox.Show("NET_DVR_Init error!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //保存SDK日志 To save the SDK log
            CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
            Growl.SuccessGlobal("HikVisionSdk Init Success");
        }

        private void HikVisionLoginDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            MouseDown += delegate { DragMove(); };
        }
    }
}