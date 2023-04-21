using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using CSharpDemo.Dialogs;
using HandyControl.Controls;
using HikVisionPreview;
using Window = System.Windows.Window;

namespace CSharpDemo.Views
{
    public partial class HikVisionWindow : Window
    {
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new HikVisionLoginDialog(GetLoginParam) { Owner = this }.ShowDialog();
        }

        private int _userId = -1;
        private int _realHandle = -1;
        private CHCNetSDK.NET_DVR_USER_LOGIN_INFO _structLogInfo;
        private CHCNetSDK.NET_DVR_DEVICEINFO_V40 _deviceInfo;

        private void GetLoginParam(string host, string name, string port, string password)
        {
            _structLogInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO();

            //设备IP地址或者域名
            var byIp = Encoding.Default.GetBytes(host);
            _structLogInfo.sDeviceAddress = new byte[129];
            byIp.CopyTo(_structLogInfo.sDeviceAddress, 0);

            //设备用户名
            var byUserName = Encoding.Default.GetBytes(name);
            _structLogInfo.sUserName = new byte[64];
            byUserName.CopyTo(_structLogInfo.sUserName, 0);

            //设备密码
            var byPassword = Encoding.Default.GetBytes(password);
            _structLogInfo.sPassword = new byte[64];
            byPassword.CopyTo(_structLogInfo.sPassword, 0);

            _structLogInfo.wPort = ushort.Parse(port); //设备服务端口号
            _structLogInfo.bUseAsynLogin = false;

            _deviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();

            //登录设备 Login the device
            _userId = CHCNetSDK.NET_DVR_Login_V40(ref _structLogInfo, ref _deviceInfo);
            if (_userId < 0)
            {
                Growl.ErrorGlobal($"NET_DVR_Login_V40 failed, error code= {CHCNetSDK.NET_DVR_GetLastError()}");
                return;
            }

            //登录成功
            Growl.SuccessGlobal("Login Success!");

            if (_realHandle < 0)
            {
                var lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO
                {
                    //TODO RealPlayPictureBoxHost.Child才是PictureBox，巨坑！！！
                    hPlayWnd = RealPlayPictureBoxHost.Child.Handle, //预览窗口
                    lChannel = 1, //预览的设备通道
                    dwStreamType = 0, //码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                    dwLinkMode = 0, //连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                    bBlocked = false, //0- 非阻塞取流，1- 阻塞取流
                    dwDisplayBufNum = 6, //播放库播放缓冲区最大缓冲帧数
                    byProtoType = 0,
                    byPreviewMode = 0
                };

                //打开预览 Start live view 
                _realHandle = CHCNetSDK.NET_DVR_RealPlay_V40(_userId, ref lpPreviewInfo, null, new IntPtr());

                if (_realHandle < 0)
                {
                    Growl.ErrorGlobal($"NET_DVR_RealPlay_V40 failed, error code= {CHCNetSDK.NET_DVR_GetLastError()}");
                    return;
                }

                //预览成功
                Growl.SuccessGlobal("Preview Success!");
            }
        }

        public HikVisionWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_realHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(_realHandle);
            }

            if (_userId >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(_userId);
            }

            base.OnClosing(e);
        }
    }
}