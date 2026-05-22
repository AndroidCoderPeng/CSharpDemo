using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using CSharpDemo.Model;
using CSharpDemo.Service;
using CSharpDemo.Utils;
using Prism.Commands;
using Prism.Mvvm;
using MessageBox = System.Windows.MessageBox;

namespace CSharpDemo.ViewModels
{
    public class SerialPortViewModel : BindableBase, IDisposable
    {
        #region VM

        private ObservableCollection<string> _responseCollection = new ObservableCollection<string>();

        public ObservableCollection<string> ResponseCollection
        {
            get => _responseCollection;
            set => SetProperty(ref _responseCollection, value);
        }

        private string _userInputHex;

        public string UserInputHex
        {
            get => _userInputHex;
            set => SetProperty(ref _userInputHex, value);
        }

        private string[] _portArray;

        public string[] PortArray
        {
            get => _portArray;
            set => SetProperty(ref _portArray, value);
        }

        private List<string> _baudRateList;

        public List<string> BaudRateList
        {
            get => _baudRateList;
            set => SetProperty(ref _baudRateList, value);
        }

        private List<string> _dataBitList;

        public List<string> DataBitList
        {
            get => _dataBitList;
            set => SetProperty(ref _dataBitList, value);
        }

        private List<string> _parityList;

        public List<string> ParityList
        {
            get => _parityList;
            set => SetProperty(ref _parityList, value);
        }

        private List<string> _stopBitList;

        public List<string> StopBitList
        {
            get => _stopBitList;
            set => SetProperty(ref _stopBitList, value);
        }

        private string _stateColorBrush = "DarkGray";

        public string StateColorBrush
        {
            get => _stateColorBrush;
            set => SetProperty(ref _stateColorBrush, value);
        }

        private bool _comboBoxEnabled = true;

        public bool ComboBoxEnabled
        {
            get => _comboBoxEnabled;
            set => SetProperty(ref _comboBoxEnabled, value);
        }

        private string _buttonContent = "打开串口";

        public string ButtonContent
        {
            get => _buttonContent;
            set => SetProperty(ref _buttonContent, value);
        }

        #endregion

        #region DelegateCommand

        public DelegateCommand<object> ItemSelectedCommand { get; }
        public DelegateCommand<string> PortNameItemSelectedCommand { get; }
        public DelegateCommand<string> BaudRateItemSelectedCommand { get; }
        public DelegateCommand<string> DataBitItemSelectedCommand { get; }
        public DelegateCommand<string> CheckModeItemSelectedCommand { get; }
        public DelegateCommand<string> StopBitItemSelectedCommand { get; }
        public DelegateCommand OpenSerialPortCommand { get; }
        public DelegateCommand ClearMessageCommand { get; }
        public DelegateCommand SendMessageCommand { get; }

        #endregion

        #region 变量

        private readonly IAppDataService _dataService;
        private readonly ISerialPortService _serialPortService;
        private readonly IFrameParserStrategy _frameParser;
        private SerialPortManager _portManager;
        private IDisposable _subscription;
        private string _portName = "";
        private string _baudRate = "230400";
        private string _dataBits = "8";
        private string _parity = "None";
        private string _stopBit = "1";

        #endregion

        public SerialPortViewModel(IAppDataService dataService, ISerialPortService serialPortService)
        {
            _dataService = dataService;
            _serialPortService = serialPortService;
            _frameParser = new CorrelatorFrameParser();

            PortArray = SerialPortManager.GetAvailablePorts();
            BaudRateList = new List<string>
            {
                "9600", "14400", "19200", "38400", "56000", "57600", "115200", "128000", "230400"
            };
            DataBitList = new List<string> { "5", "6", "7", "8" };
            ParityList = new List<string> { "None", "Odd", "Even", "Mark", "Space" };
            StopBitList = new List<string> { "1", "2" };

            ItemSelectedCommand = new DelegateCommand<object>(CommandItemSelected);
            PortNameItemSelectedCommand = new DelegateCommand<string>(item => { _portName = item; });
            BaudRateItemSelectedCommand = new DelegateCommand<string>(item => { _baudRate = item; });
            DataBitItemSelectedCommand = new DelegateCommand<string>(item => { _dataBits = item; });
            CheckModeItemSelectedCommand = new DelegateCommand<string>(item => { _parity = item; });
            StopBitItemSelectedCommand = new DelegateCommand<string>(item => { _stopBit = item; });

            OpenSerialPortCommand = new DelegateCommand(OpenSerialPort);
            ClearMessageCommand = new DelegateCommand(delegate { ResponseCollection.Clear(); });
            SendMessageCommand = new DelegateCommand(SendMessage);
        }

        private void CommandItemSelected(object item)
        {
            if (item == null) return;
            switch (item)
            {
                case 0:
                    UserInputHex = "";
                    break;
                case 1:
                    UserInputHex = _dataService.GetStatusCollectCmd(0x01);
                    break;
                case 2:
                    UserInputHex = _dataService.GetStatusCollectCmd(0x02);
                    break;
                case 3:
                    UserInputHex = _dataService.GetCorrelatorWakeUpCmd();
                    break;
            }
        }

        private void OpenSerialPort()
        {
            if (_portManager != null && _portManager.IsOpen)
            {
                _portManager.Close();
                _subscription?.Dispose();
                _subscription = null;

                StateColorBrush = "LightGray";
                ButtonContent = "打开串口";
                ComboBoxEnabled = true;
            }
            else
            {
                if (string.IsNullOrEmpty(_portName))
                {
                    MessageBox.Show("请先选择串口", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var ports = SerialPortManager.GetAvailablePorts();
                if (!ports.Any())
                {
                    MessageBox.Show("没有可用的串口", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _portManager = _serialPortService.GetOrCreateManager(_portName, _frameParser);
                if (_portManager.SetConfiguration(_portName, _baudRate, _parity, _dataBits, _stopBit))
                {
                    _portManager.Open();
                    _subscription?.Dispose();
                    _subscription = _serialPortService.Subscribe(_portName, OnDataReceived, OnError);

                    StateColorBrush = "LimeGreen";
                    ButtonContent = "关闭串口";
                    ComboBoxEnabled = false;
                }
                else
                {
                    MessageBox.Show("串口打开失败", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnDataReceived(byte[] data)
        {
            try
            {
                if (data.Length > 256)
                {
                    var logBytes = new byte[256];
                    Array.Copy(data, 1, logBytes, 0, 256);
                    Console.WriteLine($@"报文回复 <=== {BitConverter.ToString(logBytes)}, 原始数据长度: {data.Length}, 其余省略...");
                }
                else
                {
                    Console.WriteLine($@"报文回复 <=== {BitConverter.ToString(data)}");
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var versionByte = new byte[1];
                    Array.Copy(data, 1, versionByte, 0, 1);
                    var value = BitConverter.ToString(versionByte);
                    var high = short.Parse(value) / 10;
                    var low = short.Parse(value) % 10;
                    var version = $"{high}.{low}";

                    var deviceCodeBytes = new byte[6];
                    Array.Copy(data, 4, deviceCodeBytes, 0, 6);
                    var deviceCode = BitConverter.ToString(deviceCodeBytes).Replace("-", "");

                    // 传感器报文内容
                    var packetBytes = new byte[data.Length - 18];
                    Array.Copy(data, 16, packetBytes, 0, data.Length - 18);
                    try
                    {
                        var packets = _frameParser.ParseFrame<List<BasePacket>>(packetBytes);
                        ResponseCollection.Add($"[{DateTime.Now:HH:mm:ss.fff}] 解析结果: Tag数量 => {packets.Count}");
                        switch (data.Length)
                        {
                            case 32: //设备状态、电量
                                var cellPacket = packets.Find(x => x.Oid.Equals(BasePacket.CellOid));
                                var cellHex = BitConverter.ToString(cellPacket.DataValue).Replace("-", "");
                                var cell = Convert.ToInt32(cellHex, 16).ToString();

                                var statePacket = packets.Find(x => x.Oid.Equals(BasePacket.ExceptionOid));
                                var state = statePacket.DataValue[0] == 1 ? "正常" : "异常";

                                ResponseCollection.Add(
                                    $"[{DateTime.Now:HH:mm:ss.fff}] 设备ID: {deviceCode}, 电量: {cell}%, 状态: {state}");
                                break;
                            case 11293: //数据采集
                                var timePacket = packets.Find(x => x.Oid.Equals(BasePacket.TimeOid));
                                var timeHex = BitConverter.ToString(timePacket.DataValue).Replace("-", "");
                                var temp = new List<string>();
                                for (var i = 0; i < timeHex.Length; i += 2)
                                {
                                    temp.Add(timeHex.Substring(i, 2));
                                }

                                var timeBuilder = new StringBuilder();
                                var year = $"{Convert.ToInt32(temp[0], 16) + 2000}";
                                var month = Convert.ToInt32(temp[1], 16).AppendLeftZero();
                                var day = Convert.ToInt32(temp[2], 16).AppendLeftZero();
                                var hour = Convert.ToInt32(temp[3], 16).AppendLeftZero();
                                var minute = Convert.ToInt32(temp[4], 16).AppendLeftZero();
                                var seconds = Convert.ToInt32(temp[5], 16).AppendLeftZero();
                                timeBuilder.Append(year).Append(month).Append(day).Append(hour).Append(minute)
                                    .Append(seconds);
                                var time = timeBuilder.ToString();
                                Console.WriteLine(time);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ResponseCollection.Add($"[{DateTime.Now:HH:mm:ss.fff}] 解析异常: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                ResponseCollection.Add($"[{DateTime.Now:HH:mm:ss.fff}] 解析异常: {ex.Message}");
            }
        }

        private void OnError(string error)
        {
            ResponseCollection.Add($"[{DateTime.Now:HH:mm:ss.fff}] 错误: {error}");
        }

        private void SendMessage()
        {
            if (!_portManager.IsOpen)
            {
                MessageBox.Show("串口未打开", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_userInputHex.Equals(""))
            {
                MessageBox.Show("不能发送空消息", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 清理输入并统一分隔符
            var cleanedHex = _userInputHex.Trim().Replace("-", " ");
            string[] bytes;

            // 统一处理空格分隔或连续字符的情况
            if (cleanedHex.Contains(" "))
            {
                // 过滤空字符串并分割
                bytes = cleanedHex.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                // 每两个字符作为一个Hex字节
                if (cleanedHex.Length % 2 != 0)
                {
                    MessageBox.Show("十六进制字符串长度必须为偶数", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bytes = Enumerable.Range(0, cleanedHex.Length / 2).Select(i =>
                    cleanedHex.Substring(i * 2, 2)
                ).ToArray();
            }

            var cmd = new byte[bytes.Length];
            for (var i = 0; i < bytes.Length; i++)
            {
                if (!byte.TryParse(bytes[i], NumberStyles.HexNumber, null, out cmd[i]))
                {
                    MessageBox.Show($"无效的十六进制值: {bytes[i]}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            _portManager.Write(cmd);
            Console.WriteLine($@"指令下发 ===> {_userInputHex}");
        }

        public void Dispose()
        {
            _portManager?.Dispose();
        }
    }
}