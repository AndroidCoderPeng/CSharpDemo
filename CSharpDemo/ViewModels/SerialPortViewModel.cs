using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;
using CorrelatorSingle;
using CSharpDemo.Model;
using CSharpDemo.Utils;
using HandyControl.Controls;
using MathWorks.MATLAB.NET.Arrays;
using Prism.Commands;
using Prism.Mvvm;
using MessageBox = System.Windows.MessageBox;
using Tag = CSharpDemo.Tags.Tag;

namespace CSharpDemo.ViewModels
{
    public class SerialPortViewModel : BindableBase
    {
        #region VM

        private string[] _portArray;
        private List<int> _baudRateList;
        private List<int> _dataBitList;
        private List<Parity> _parityList;
        private List<int> _stopBitList;
        private ObservableCollection<string> _responseCollection = new ObservableCollection<string>();
        private ObservableCollection<string> _logCollection = new ObservableCollection<string>();
        private string _portName = "COM3";
        private int _baudRate = 230400;
        private int _dataBits = 8;
        private Parity _parity = Parity.None;
        private int _stopBit = 1;
        private string _stateColorBrush = "DarkGray";
        private string _userInputHex = "A3-20-00-13-00-00-00-00-00-00-01-FF-FF-0A-82-01-30-00-00-01-00-01-00-7D-87";
        private bool _portNameComboBoxIsEnabled = true;
        private bool _baudRateComboBoxIsEnabled = true;
        private bool _dataBitComboBoxIsEnabled = true;
        private bool _parityComboBoxIsEnabled = true;
        private bool _stopBitComboBoxIsEnabled = true;

        /// <summary>
        /// 端口
        /// </summary>
        public string[] PortArray
        {
            get => _portArray;
            private set
            {
                _portArray = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 校验模式
        /// </summary>
        public List<Parity> ParityList
        {
            get => _parityList;
            private set
            {
                _parityList = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 停止位
        /// </summary>
        public List<int> StopBitList
        {
            get => _stopBitList;
            private set
            {
                _stopBitList = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 波特率
        /// </summary>
        public List<int> BaudRateList
        {
            get => _baudRateList;
            private set
            {
                _baudRateList = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 数据位
        /// </summary>
        public List<int> DataBitList
        {
            get => _dataBitList;
            private set
            {
                _dataBitList = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 串口返回值消息集合
        /// </summary>
        public ObservableCollection<string> ResponseCollection
        {
            get => _responseCollection;
            set
            {
                _responseCollection = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> LogCollection
        {
            get => _logCollection;
            set
            {
                _logCollection = value;
                RaisePropertyChanged();
            }
        }

        public string PortName
        {
            get => _portName;
            set
            {
                _portName = value;
                RaisePropertyChanged();
            }
        }

        public int BaudRate
        {
            get => _baudRate;
            set
            {
                _baudRate = value;
                RaisePropertyChanged();
            }
        }

        public int DataBit
        {
            get => _dataBits;
            set
            {
                _dataBits = value;
                RaisePropertyChanged();
            }
        }

        public Parity Parity
        {
            get => _parity;
            set
            {
                _parity = value;
                RaisePropertyChanged();
            }
        }

        public int StopBit
        {
            get => _stopBit;
            set
            {
                _stopBit = value;
                RaisePropertyChanged();
            }
        }


        public string StateColorBrush
        {
            get => _stateColorBrush;
            private set
            {
                _stateColorBrush = value;
                RaisePropertyChanged();
            }
        }

        public string UserInputHex
        {
            get => _userInputHex;
            set
            {
                _userInputHex = value;
                RaisePropertyChanged();
            }
        }

        public bool PortNameComboBoxIsEnabled
        {
            get => _portNameComboBoxIsEnabled;
            set
            {
                _portNameComboBoxIsEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool BaudRateComboBoxIsEnabled
        {
            get => _baudRateComboBoxIsEnabled;
            set
            {
                _baudRateComboBoxIsEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool DataBitComboBoxIsEnabled
        {
            get => _dataBitComboBoxIsEnabled;
            set
            {
                _dataBitComboBoxIsEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool ParityComboBoxIsEnabled
        {
            get => _parityComboBoxIsEnabled;
            set
            {
                _parityComboBoxIsEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool StopBitComboBoxIsEnabled
        {
            get => _stopBitComboBoxIsEnabled;
            set
            {
                _stopBitComboBoxIsEnabled = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region DelegateCommand

        public DelegateCommand<ComboBox> PortItemSelectedCommand { get; }
        public DelegateCommand<ComboBox> BaudRateItemSelectedCommand { get; }
        public DelegateCommand<ComboBox> DataBitItemSelectedCommand { get; }
        public DelegateCommand<ComboBox> CheckModeItemSelectedCommand { get; }
        public DelegateCommand<ComboBox> StopBitItemSelectedCommand { get; }
        public DelegateCommand OpenSerialPortCommand { get; }
        public DelegateCommand CloseSerialPortCommand { get; }
        public DelegateCommand ClearMessageCommand { get; }
        public DelegateCommand SendMessageCommand { get; }
        public DelegateCommand CalculateCommand { get; }

        #endregion

        #region 变量

        private readonly SerialPortManager _serialPortManager = new SerialPortManager();
        private static readonly Lazy<Correlator> LazyCorrelator = new Lazy<Correlator>(() => new Correlator());
        private readonly BackgroundWorker _backgroundWorker;
        private CorrelatorDataModel _dataModel;

        #endregion

        public SerialPortViewModel()
        {
            PortArray = _serialPortManager.GetPorts();
            BaudRateList = new List<int> { 9600, 14400, 19200, 38400, 56000, 57600, 115200, 128000, 230400 };
            DataBitList = new List<int> { 5, 6, 7, 8 };
            ParityList = new List<Parity> { Parity.None, Parity.Odd, Parity.Even, Parity.Mark, Parity.Space };
            StopBitList = new List<int> { 1, 2 };

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += Worker_OnDoWork;
            _backgroundWorker.ProgressChanged += Worker_OnProgressChanged;
            _backgroundWorker.RunWorkerCompleted += Worker_OnRunWorkerCompleted;

            PortItemSelectedCommand = new DelegateCommand<ComboBox>(delegate(ComboBox box)
            {
                PortName = box.SelectedItem.ToString();
            });

            BaudRateItemSelectedCommand = new DelegateCommand<ComboBox>(delegate(ComboBox box)
            {
                BaudRate = int.Parse(box.SelectedItem.ToString());
            });

            DataBitItemSelectedCommand = new DelegateCommand<ComboBox>(delegate(ComboBox box)
            {
                DataBit = int.Parse(box.SelectedItem.ToString());
            });

            CheckModeItemSelectedCommand = new DelegateCommand<ComboBox>(delegate(ComboBox box)
            {
                Parity = (Parity)box.SelectedItem;
            });

            StopBitItemSelectedCommand = new DelegateCommand<ComboBox>(delegate(ComboBox box)
            {
                StopBit = int.Parse(box.SelectedItem.ToString());
            });

            OpenSerialPortCommand = new DelegateCommand(delegate
            {
                _serialPortManager.PortName = _portName;
                _serialPortManager.BaudRate = _baudRate;
                _serialPortManager.DataBits = _dataBits;
                _serialPortManager.Parity = _parity;
                _serialPortManager.StopBits = (StopBits)_stopBit;

                _serialPortManager.Open();
                if (_serialPortManager.IsOpen)
                {
                    StateColorBrush = "LimeGreen";
                    PortNameComboBoxIsEnabled = false;
                    BaudRateComboBoxIsEnabled = false;
                    DataBitComboBoxIsEnabled = false;
                    ParityComboBoxIsEnabled = false;
                    StopBitComboBoxIsEnabled = false;
                }
            });

            CloseSerialPortCommand = new DelegateCommand(delegate
            {
                _serialPortManager.Close();
                if (!_serialPortManager.IsOpen)
                {
                    StateColorBrush = "LightGray";
                    PortNameComboBoxIsEnabled = true;
                    BaudRateComboBoxIsEnabled = true;
                    DataBitComboBoxIsEnabled = true;
                    ParityComboBoxIsEnabled = true;
                    StopBitComboBoxIsEnabled = true;
                }
            });

            ClearMessageCommand = new DelegateCommand(delegate
            {
                ResponseCollection.Clear();
                LogCollection.Clear();
            });

            SendMessageCommand = new DelegateCommand(delegate
            {
                if (!_serialPortManager.IsOpen)
                {
                    MessageBox.Show("串口未打开", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_userInputHex.Equals(""))
                {
                    MessageBox.Show("不能发送空消息", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string[] bytes;
                if (_userInputHex.Contains(" "))
                {
                    bytes = _userInputHex.Split(' ');
                }
                else if (_userInputHex.Contains("-"))
                {
                    bytes = _userInputHex.Split('-');
                }
                else
                {
                    //每两个字符作为一个Hex
                    var dataValue = _userInputHex.ToList();
                    var temp = new List<string>();
                    for (var i = 0; i < dataValue.Count; i += 2)
                    {
                        var builder = new StringBuilder();
                        var hex = builder.Append(dataValue[i]).Append(dataValue[i + 1]);
                        temp.Add(hex.ToString());
                    }

                    bytes = temp.ToArray();
                }

                var cmd = new byte[bytes.Length];
                for (var i = 0; i < bytes.Length; i++)
                {
                    cmd[i] = Convert.ToByte(bytes[i], 16);
                }

                _serialPortManager.Write(cmd);
            });

            CalculateCommand = new DelegateCommand(delegate
            {
                LogCollection.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}开始计算");

                if (_dataModel == null)
                {
                    return;
                }

                _backgroundWorker.RunWorkerAsync();
            });

            _serialPortManager.DataReceivedAction += delegate(byte[] bytes)
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    ResponseCollection.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}收到串口数据，长度是：{bytes.Length}");

                    var deviceIdBytes = new byte[6];
                    Array.Copy(bytes, 4, deviceIdBytes, 0, 6);
                    var deviceId = BitConverter.ToString(deviceIdBytes).Replace("-", "");

                    var tagBytes = new byte[bytes.Length - 18];
                    Array.Copy(bytes, 16, tagBytes, 0, bytes.Length - 18);
                    var tags = tagBytes.GetTags();
                    switch (bytes.Length)
                    {
                        case 32: //设备状态、电量

                            break;
                        case 30:

                            break;
                        case 22543:
                            HandleCorrelatorData(deviceId, tags);
                            break;
                        case 15024:

                            break;
                    }
                });
            };
        }

        private void HandleCorrelatorData(string devCode, List<Tag> tags)
        {
            if (_dataModel == null)
            {
                _dataModel = new CorrelatorDataModel();
            }

            LogCollection.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}开始处理数据");
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

                _dataModel.DevCode = devCode;
                if (devCode.Equals(RuntimeCache.Dev1))
                {
                    //接收到数据之后时间重新赋值
                    _dataModel.LeftReceiveDataTime = DateTime.Now;
                    _dataModel.LeftDeviceDataArray = realData;
                    LogCollection.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}Dev1数据处理完成");
                }
                else
                {
                    //接收到数据之后时间重新赋值
                    _dataModel.RightReceiveDataTime = DateTime.Now;
                    _dataModel.RightDeviceDataArray = realData;
                    LogCollection.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}Dev2数据处理完成");
                }
            }
        }

        private void Worker_OnDoWork(object sender, DoWorkEventArgs e)
        {
            var array = LazyCorrelator.Value.locating(11,
                (MWNumericArray)_dataModel.LeftDeviceDataArray, (MWNumericArray)_dataModel.RightDeviceDataArray,
                7500,
                int.Parse("150"), int.Parse("1130"),
                0, 0,
                0, 0,
                "",
                int.Parse("300"), int.Parse("300"),
                1, -1,
                -1, -1,
                int.Parse("100"), int.Parse("3000"));
        }

        private void Worker_OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void Worker_OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LogCollection.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}结束计算");
            _dataModel = null;
        }
    }
}