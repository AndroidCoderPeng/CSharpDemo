using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;
using CSharpDemo.Utils;
using Prism.Commands;
using Prism.Mvvm;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace CSharpDemo.ViewModels
{
    public class UdpServerViewModel : BindableBase
    {
        private ObservableCollection<string> _messages = new ObservableCollection<string>();

        public ObservableCollection<string> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand<ListBox> ItemSelectedCommand { set; get; }

        public UdpServerViewModel()
        {
            var server = new AppServer();
            if (!server.Setup(new ServerConfig
                {
                    Ip = "Any",
                    Port = 7777,
                    Mode = SocketMode.Udp
                })) //配置服务器
            {
                Debug.WriteLine("UdpServerViewModel => 配置服务器失败！");
                return;
            }

            //获取本地IP
            var hostAddress = SystemHelper.GetHostAddress();
            Debug.WriteLine($"UdpServerViewModel => {hostAddress}");

            if (!server.Start())
            {
                Debug.WriteLine("UdpServerViewModel => 启动失败！");
                return;
            }

            Messages.Add($"服务端{hostAddress}:7777已启动");

            server.NewSessionConnected += NewSessionConnected;
            server.NewRequestReceived += NewRequestReceived;

            ItemSelectedCommand = new DelegateCommand<ListBox>(delegate(ListBox box)
            {
                Debug.WriteLine($"UdpServerViewModel => {box.SelectedItem}");
            });
        }

        private static void NewSessionConnected(AppSession session)
        {
            Debug.WriteLine($"UdpServerViewModel => {session.SessionID} Connected");
        }

        private static void NewRequestReceived(AppSession session, StringRequestInfo requestInfo)
        {
            var body = requestInfo.Body;
            Debug.WriteLine($"UdpServerViewModel => {body}");
        }
    }
}