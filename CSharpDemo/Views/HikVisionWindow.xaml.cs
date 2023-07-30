using System;
using System.ComponentModel;
using HandyControl.Controls;
using HikVisionPreview;
using Window = System.Windows.Window;

namespace CSharpDemo.Views
{
    public partial class HikVisionWindow : Window
    {
        private readonly int _userId = -1;
        private readonly int _realHandle = -1;

        public HikVisionWindow(int userId)
        {
            InitializeComponent();

            if (_realHandle < 0)
            {
                _userId = userId;
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
                _realHandle = CHCNetSDK.NET_DVR_RealPlay_V40(userId, ref lpPreviewInfo, null, new IntPtr());

                if (_realHandle < 0)
                {
                    Growl.ErrorGlobal($"NET_DVR_RealPlay_V40 failed, error code= {CHCNetSDK.NET_DVR_GetLastError()}");
                    return;
                }

                //预览成功
                Growl.SuccessGlobal("Preview Success!");
            }
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