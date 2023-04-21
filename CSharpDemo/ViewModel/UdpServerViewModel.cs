using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CSharpDemo.Model;
using CSharpDemo.Utils;
using CSharpDemo.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
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

                    Application.Current.Dispatcher.Invoke(() => { Messages.Add($"接收到信息：{message}"); });
                });
            };

            //获取本地IP
            var hostAddress = SystemHelper.GetHostAddress();

            var config = new TouchSocketConfig();
            config.SetBindIPHost(new IPHost(hostAddress + ":" + 7777));

            //载入配置
            _udpSession.Setup(config).Start();
            Messages.Add($"服务端{hostAddress}:7777已启动");

            Messages.Add(GetTestMessage());

            ItemSelectedCommand = new RelayCommand<ListBox>(it =>
            {
                var item = it.SelectedItem.ToString();
                //接收到信息：{"position":[{"x":0.19834815,"y":0.43050563},{"x":0.19834815,"y":0.15416667},{"x":0.77895296,"y":0.43050563},{"x":0.77895296,"y":0.15416667}],"color":"#FF0000","code":"11,12"}

                if (item.Contains("position"))
                {
                    var first = item.SplitFirst('：')[1];
                    new VideoReginWindow(first).Show();
                }
            });
        }

        private string GetTestMessage()
        {
            var builder = new StringBuilder();
            builder.Append("测试接收到信息：");
            var reginModel = new ReginModel
            {
                position = new List<Piont>
                {
                    new Piont { x = 0.19834815, y = 0.43050563 },
                    new Piont { x = 0.19834815, y = 0.15416667 },
                    new Piont { x = 0.77895296, y = 0.43050563 },
                    new Piont { x = 0.77895296, y = 0.15416667 }
                },
                color = "#FF0000",
                code = "11,12"
            };

            builder.Append(JsonConvert.SerializeObject(reginModel));
            return builder.ToString();
        }
    }
}