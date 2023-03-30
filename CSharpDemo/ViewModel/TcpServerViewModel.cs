using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using GalaSoft.MvvmLight;
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

        public TcpServerViewModel()
        {
            //启动TCP Server
            var service = new TcpService
            {
                Connecting = (client, e) =>
                {
                    Debug.WriteLine($"客户端{client.ID}正在连接");
                    Application.Current.Dispatcher.Invoke(() => { Messages.Add($"客户端{client.ID}正在连接"); });
                },
                Connected = (client, e) =>
                {
                    Debug.WriteLine($"客户端{client.ID}已连接");
                    Application.Current.Dispatcher.Invoke(() => { Messages.Add($"客户端{client.ID}已连接"); });
                },
                Disconnected = (client, e) =>
                {
                    Debug.WriteLine($"客户端{client.ID}已断开连接");
                    Application.Current.Dispatcher.Invoke(() => { Messages.Add($"客户端{client.ID}已断开连接"); });
                },
                Received = (client, byteBlock, requestInfo) =>
                {
                    //从客户端收到信息
                    var mes = byteBlock.ToString();
                    Application.Current.Dispatcher.Invoke(() => { Messages.Add($"已从{client.ID}接收到信息：{mes}"); });
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

            //获取本地IP
            var hostName = Dns.GetHostName();
            var addresses = Dns.GetHostAddresses(hostName);
            foreach (var ip in addresses)
            {
                if (ip.AddressFamily.ToString() != "InterNetwork") continue;
                Messages.Add($"服务端{ip}:7777已启动");
                //只找一个IPV4地址
                return;
            }
        }
    }
}