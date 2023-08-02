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

        private ObservableCollection<string> _connectedLinks = new ObservableCollection<string>();

        public ObservableCollection<string> ConnectedLinks
        {
            get => _connectedLinks;
            set
            {
                _connectedLinks = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand<UserControl> ViewOnUnloadedCommand { set; get; }
        public DelegateCommand<ListBox> ItemSelectedCommand { set; get; }
        public DelegateCommand StartServerCommand { set; get; }

        public UdpServerViewModel()
        {
            ViewOnUnloadedCommand = new DelegateCommand<UserControl>(delegate(UserControl control)
            {
                //页面移除的时候触发
            });

            ItemSelectedCommand = new DelegateCommand<ListBox>(delegate(ListBox box)
            {
                Debug.WriteLine($"UdpServerViewModel => {box.SelectedItem}");
            });

            StartServerCommand = new DelegateCommand(delegate
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
            });
        }

        private void NewSessionConnected(AppSession session)
        {
            Debug.WriteLine($"UdpServerViewModel => {session.SessionID} Connected");
            ConnectedLinks.Add($"{session.RemoteEndPoint.Address}:{session.RemoteEndPoint.Port}");
        }

        private void NewRequestReceived(AppSession session, StringRequestInfo requestInfo)
        {
            var body = requestInfo.Body;
            Debug.WriteLine($"UdpServerViewModel => {body}");
        }
    }
}