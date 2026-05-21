using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using CSharpDemo.Model;
using CSharpDemo.Utils;
using Prism.Commands;
using Prism.Mvvm;
using MessageBox = System.Windows.MessageBox;
using Tag = CSharpDemo.Tags.Tag;

namespace CSharpDemo.ViewModels
{
    public class SerialPortViewModel : BindableBase
    {
        #region VM

        private ObservableCollection<string> _responseCollection = new ObservableCollection<string>();

        public ObservableCollection<string> ResponseCollection
        {
            get => _responseCollection;
            set => SetProperty(ref _responseCollection, value);
        }

        private string _userInputHex = "A3-20-00-13-00-00-00-00-00-00-01-FF-FF-0A-82-01-30-00-00-01-00-01-00-7D-87";

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

        private readonly SerialPortManager _portManager = new SerialPortManager();
        private string _portName = "COM5";
        private string _baudRate = "230400";
        private string _dataBits = "8";
        private string _parity = "None";
        private string _stopBit = "1";
        private readonly CorrelatorData _dataModel = new CorrelatorData();

        #endregion

        public SerialPortViewModel()
        {
            PortArray = _portManager.GetPorts();
            BaudRateList = new List<string>
            {
                "9600", "14400", "19200", "38400", "56000", "57600", "115200", "128000", "230400"
            };
            DataBitList = new List<string> { "5", "6", "7", "8" };
            ParityList = new List<string> { "None", "Odd", "Even", "Mark", "Space" };
            StopBitList = new List<string> { "1", "2" };

            PortNameItemSelectedCommand = new DelegateCommand<string>(item => { _portName = item; });
            BaudRateItemSelectedCommand = new DelegateCommand<string>(item => { _baudRate = item; });
            DataBitItemSelectedCommand = new DelegateCommand<string>(item => { _dataBits = item; });
            CheckModeItemSelectedCommand = new DelegateCommand<string>(item => { _parity = item; });
            StopBitItemSelectedCommand = new DelegateCommand<string>(item => { _stopBit = item; });

            OpenSerialPortCommand = new DelegateCommand(OpenSerialPort);
            ClearMessageCommand = new DelegateCommand(delegate { ResponseCollection.Clear(); });
            SendMessageCommand = new DelegateCommand(SendMessage);

            _portManager.DataReceivedEvent += delegate((int, string, List<Tag>) args)
            {
                switch (args.Item1)
                {
                    case 0: // 设备状态、电量

                        break;
                    case 1: // 加速度计
                        HandleCorrelatorData(args.Item2, args.Item3);
                        break;
                }
            };
        }

        private void OpenSerialPort()
        {
            if (_portManager.IsOpen)
            {
                _portManager.Close();

                StateColorBrush = "LightGray";
                ButtonContent = "打开串口";
                ComboBoxEnabled = true;
            }
            else
            {
                if (!_portManager.GetPorts().Any())
                {
                    MessageBox.Show("没有可用的串口", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_portManager.SetConfiguration(_portName, _baudRate, _parity, _dataBits, _stopBit))
                {
                    _portManager.Open();
                    StateColorBrush = "LimeGreen";
                    ButtonContent = "关闭串口";
                    ComboBoxEnabled = false;
                }
            }
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
        }

        private void HandleCorrelatorData(string devCode, List<Tag> tags)
        {
            //处理接到的噪声数据
            var noiseTag = tags.GetUploadNoiseTag();
            if (noiseTag != null)
            {
                var num = noiseTag.Len / 3;
                var realData = new double[num];
                for (var i = 0; i < num; i++)
                {
                    var dStr = new byte[3];
                    Array.Copy(noiseTag.DataValue, i * 3, dStr, 0, 3);

                    realData[i] = dStr.HexToDouble();
                }

                if (devCode.Equals(RuntimeCache.Dev1))
                {
                    //接收到数据之后时间重新赋值
                    _dataModel.RedDevCode = RuntimeCache.Dev1;
                    _dataModel.ReceiveRedSensorDataTime = DateTime.Now;
                    _dataModel.RedDeviceData = realData;
                }
                else
                {
                    //接收到数据之后时间重新赋值
                    _dataModel.BlueDevCode = RuntimeCache.Dev2;
                    _dataModel.ReceiveBlueSensorDataTime = DateTime.Now;
                    _dataModel.BlueDeviceData = realData;
                }
            }
        }
    }
}