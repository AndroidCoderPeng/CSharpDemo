using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using AForge.Video.DirectShow;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace CSharpDemo.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private FilterInfo _currentDevice;

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var videoDevices = new ObservableCollection<FilterInfo>();
            foreach (FilterInfo filter in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                videoDevices.Add(filter);
            }

            if (videoDevices.Any())
            {
                _currentDevice = videoDevices[0];
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCamera(object sender, RoutedEventArgs e)
        {
            if (_currentDevice != null)
            {
                if (CameraPreviewPlayer.IsRunning)
                {
                    CameraPreviewPlayer.SignalToStop();
                    OperateCameraButton.Content = "打开相机";
                }
                else
                {
                    var videoSource = new VideoCaptureDevice(_currentDevice.MonikerString);
                    CameraPreviewPlayer.VideoSource = videoSource;
                    CameraPreviewPlayer.Start();
                    OperateCameraButton.Content = "关闭相机";
                }
            }
        }

        private void OpenLiveCharts(object sender, RoutedEventArgs e)
        {
            new LiveChartsWindow { Owner = this }.ShowDialog();
        }
        
        private void OpenTcpServer(object sender, RoutedEventArgs re)
        {
            //启动TCP Server
            var service = new TcpService
            {
                Connecting = (client, e) => { Debug.WriteLine($"客户端{client.ID}正在连接"); },
                Connected = (client, e) => { Debug.WriteLine($"客户端{client.ID}已连接"); },
                Disconnected = (client, e) => { Debug.WriteLine($"客户端{client.ID}已断开连接"); },
                Received = (client, byteBlock, requestInfo) =>
                {
                    //从客户端收到信息
                    var mes = byteBlock.ToString();
                    Debug.WriteLine($"已从{client.ID}接收到信息：{mes}");

                    //将收到的信息直接返回给发送方
                    client.Send(mes);

                    //将收到的信息返回给特定ID的客户端
                    //client.Send("id",mes);

                    //将收到的信息返回给在线的所有客户端。
                    // var clients = service.GetClients();
                    // foreach (var targetClient in clients)
                    // {
                    //     if (targetClient.ID != client.ID)
                    //     {
                    //         targetClient.Send(mes);
                    //     }
                    // }
                }
            };
            //同时监听两个地址
            var socketConfig = new TouchSocketConfig().SetListenIPHosts(new[] { new IPHost(7777) });
            service.Setup(socketConfig).Start(); //启动
        }
        
        private void OpenScottplot(object sender, RoutedEventArgs e)
        {
            new ScottplotWindow { Owner = this }.ShowDialog();
        }
    }
}