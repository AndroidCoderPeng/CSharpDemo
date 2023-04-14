using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CSharpDemo.Utils;
using CSharpDemo.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace CSharpDemo.ViewModel
{
    public class TcpServerViewModel : ViewModelBase
    {
        private ObservableCollection<string> _messages = new ObservableCollection<string>();

        public ObservableCollection<string> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                RaisePropertyChanged(() => Messages);
            }
        }

        public RelayCommand<ListBox> ItemSelectedCommand { get; }

        private readonly TcpService _tcpService = new TcpService();

        public TcpServerViewModel()
        {
            //启动TCP Server
            _tcpService.Connected = (client, e) =>
            {
                Application.Current.Dispatcher.Invoke(() => { Messages.Add($"客户端{client.ID}已连接"); });
            };
            _tcpService.Disconnected = (client, e) =>
            {
                Application.Current.Dispatcher.Invoke(() => { Messages.Add($"客户端{client.ID}已断开连接"); });
            };
            _tcpService.Received = (client, byteBlock, requestInfo) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var message = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);

                    Application.Current.Dispatcher.Invoke(() => { Messages.Add($"已从{client.ID}接收到信息：{message}"); });
                });
            };

            //获取本地IP
            var hostAddress = SystemHelper.GetHostAddress();
            Messages.Add($"服务端{hostAddress}:7777已启动");

            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new[] { new IPHost(hostAddress + ":" + 7777) });
            _tcpService.Setup(config).Start(); //启动

            ItemSelectedCommand = new RelayCommand<ListBox>(it =>
            {
                var item = it.SelectedItem.ToString();
                //已从1接收到信息：{"position":"[0.20137861,0.12853289],[0.8101852,0.12853289],[0.20137861,0.44571573],[0.8101852,0.44571573]","color":"#FF0000","code":"11,12"}

                if (item.Contains("position"))
                {
                    var first = item.SplitFirst('：')[1];
                    new VideoReginWindow(first).ShowDialog();
                }
            });
        }
    }
}