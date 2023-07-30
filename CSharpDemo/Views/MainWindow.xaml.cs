using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using CSharpDemo.Dialogs;
using CSharpDemo.Utils;
using HandyControl.Controls;
using HikVisionPreview;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CSharpDemo.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly NavigationService _service;

        public MainWindow()
        {
            InitializeComponent();

            _service = ContentFrame.NavigationService;
            _service.Navigate("CameraPage".CreateUri());
            FuncListBox.SelectedIndex = 0;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListBox listBox))
            {
                return;
            }

            switch (listBox.SelectedIndex)
            {
                case 0:
                    _service.Navigate("CameraPage".CreateUri());
                    break;
                case 1:
                    _service.Navigate("ScottPlotPage".CreateUri());
                    break;
                case 2:
                    _service.Navigate("UdpServerPage".CreateUri());
                    break;
                case 3:
                    _service.Navigate("TransmitValuePage".CreateUri());
                    break;
                case 4:
                    _service.Navigate("SerialPortPage".CreateUri());
                    break;
                case 5:
                    //初始化海康网络摄像头
                    if (InitHikVisionSdk())
                    {
                        new HikVisionLoginDialog(GetLoginParam) { Owner = this }.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("NET_DVR_Init error!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case 6:
                    _service.Navigate("DataAnalysisPage".CreateUri());
                    break;
            }
        }

        private bool InitHikVisionSdk()
        {
            try
            {
                if (!CHCNetSDK.NET_DVR_Init())
                {
                    return false;
                }

                //保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "E:\\SdkLog\\", true);
                Growl.SuccessGlobal("HikVisionSdk Init Success");
                return true;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
        }

        private void GetLoginParam(string host, string name, string port, string password)
        {
            var structLogInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO();

            //设备IP地址或者域名
            var byIp = Encoding.Default.GetBytes(host);
            structLogInfo.sDeviceAddress = new byte[129];
            byIp.CopyTo(structLogInfo.sDeviceAddress, 0);

            //设备用户名
            var byUserName = Encoding.Default.GetBytes(name);
            structLogInfo.sUserName = new byte[64];
            byUserName.CopyTo(structLogInfo.sUserName, 0);

            //设备密码
            var byPassword = Encoding.Default.GetBytes(password);
            structLogInfo.sPassword = new byte[64];
            byPassword.CopyTo(structLogInfo.sPassword, 0);

            structLogInfo.wPort = ushort.Parse(port); //设备服务端口号
            structLogInfo.bUseAsynLogin = false;

            var deviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();

            //登录设备 Login the device
            var userId = CHCNetSDK.NET_DVR_Login_V40(ref structLogInfo, ref deviceInfo);
            if (userId < 0)
            {
                Growl.ErrorGlobal($"NET_DVR_Login_V40 failed, error code= {CHCNetSDK.NET_DVR_GetLastError()}");
                return;
            }

            //登录成功
            Growl.SuccessGlobal("Login Success!");
            new HikVisionWindow(userId).Show();
        }
    }
}