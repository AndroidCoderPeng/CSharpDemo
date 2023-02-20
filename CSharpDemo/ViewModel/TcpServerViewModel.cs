using System.Diagnostics;
using GalaSoft.MvvmLight;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace CSharpDemo.ViewModel
{
    public class TcpServerViewModel : ViewModelBase
    {
        public TcpServerViewModel()
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
    }
}