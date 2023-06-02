using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CSharpDemo.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace CSharpDemo.ViewModel
{
    public class UdpServerViewModel : ViewModelBase
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

        private readonly UdpSession _udpSession = new UdpSession();

        public UdpServerViewModel()
        {
            _udpSession.Received += delegate(EndPoint endpoint, ByteBlock byteBlock, IRequestInfo info)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var message = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);

                    Messages.Add($"接收到信息：{message}");
                });
            };

            //获取本地IP
            var hostAddress = SystemHelper.GetHostAddress();

            var config = new TouchSocketConfig();
            config.SetBindIPHost(new IPHost(hostAddress + ":" + 7777));

            //载入配置
            _udpSession.Setup(config).Start();
            Messages.Add($"服务端{hostAddress}:7777已启动");

            ItemSelectedCommand = new RelayCommand<ListBox>(it => { });
        }
    }
}