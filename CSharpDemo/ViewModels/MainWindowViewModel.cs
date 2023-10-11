using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CSharpDemo.Service;
using CSharpDemo.Views;
using HandyControl.Controls;
using HikVisionPreview;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using MessageBox = HandyControl.Controls.MessageBox;
using Window = System.Windows.Window;

namespace CSharpDemo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region VM

        public ObservableCollection<string> ItemModels { get; }

        #endregion

        #region DelegateCommand

        public DelegateCommand<Window> WindowLoadedCommand { set; get; }
        public DelegateCommand<ListBox> ItemSelectedCommand { set; get; }

        #endregion

        private Window _window;

        public MainWindowViewModel(IRegionManager regionManager, IContainerProvider container,
            IMainDataService dataService)
        {
            ItemModels = dataService.GetItemModels();

            WindowLoadedCommand = new DelegateCommand<Window>(delegate(Window window)
            {
                _window = window;
                //显示默认View
                regionManager.AddToRegion("ContentRegion", container.Resolve<CameraView>());
            });

            ItemSelectedCommand = new DelegateCommand<ListBox>(delegate(ListBox box)
            {
                var region = regionManager.Regions["ContentRegion"];
                switch (box.SelectedIndex)
                {
                    case 0:
                        region.RequestNavigate("CameraView");
                        break;
                    case 1:
                        region.RequestNavigate("TransmitValueView");
                        break;
                    case 2:
                        region.RequestNavigate("SerialPortView");
                        break;
                    case 3:
                        region.RequestNavigate("DataAnalysisView");
                        break;
                    case 4:
                        region.RequestNavigate("AudioWaveView");
                        break;
                    case 5:
                        //初始化海康网络摄像头
                        if (InitHikVisionSdk())
                        {
                            new HikVisionLoginWindow(GetLoginParam) { Owner = _window }.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("NET_DVR_Init error!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        break;
                }
            });
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
                Growl.Success("HikVisionSdk Init Success");
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
            var ip = Encoding.Default.GetBytes(host);
            structLogInfo.sDeviceAddress = new byte[129];
            ip.CopyTo(structLogInfo.sDeviceAddress, 0);

            //设备用户名
            var userName = Encoding.Default.GetBytes(name);
            structLogInfo.sUserName = new byte[64];
            userName.CopyTo(structLogInfo.sUserName, 0);

            //设备密码
            var pwd = Encoding.Default.GetBytes(password);
            structLogInfo.sPassword = new byte[64];
            pwd.CopyTo(structLogInfo.sPassword, 0);

            structLogInfo.wPort = ushort.Parse(port); //设备服务端口号
            structLogInfo.bUseAsynLogin = false;

            var deviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();

            //登录设备 Login the device
            var userId = CHCNetSDK.NET_DVR_Login_V40(ref structLogInfo, ref deviceInfo);
            if (userId < 0)
            {
                Growl.Error($"NET_DVR_Login_V40 failed, error code= {CHCNetSDK.NET_DVR_GetLastError()}");
                return;
            }

            //登录成功
            Growl.Success("Login Success!");
            new HikVisionWindow(userId).Show();
        }
    }
}