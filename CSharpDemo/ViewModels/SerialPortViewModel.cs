using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;
using CSharpDemo.Utils;
using HandyControl.Controls;
using Prism.Commands;
using Prism.Mvvm;
using MessageBox = HandyControl.Controls.MessageBox;

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
        private string _portName = "COM3";
        private int _baudRate = 230400;
        private int _dataBits = 8;
        private Parity _parity = Parity.None;
        private int _stopBit = 1;
        private string _stateColorBrush = "DarkGray";
        private string _userInputHex = string.Empty;
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

        #endregion

        #region 变量

        private readonly SerialPortManager _serialPortManager = new SerialPortManager();

        #endregion

        public SerialPortViewModel()
        {
            PortArray = _serialPortManager.GetPorts();
            BaudRateList = new List<int> { 9600, 14400, 19200, 38400, 56000, 57600, 115200, 128000, 230400 };
            DataBitList = new List<int> { 5, 6, 7, 8 };
            ParityList = new List<Parity> { Parity.None, Parity.Odd, Parity.Even, Parity.Mark, Parity.Space };
            StopBitList = new List<int> { 1, 2 };

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

            ClearMessageCommand = new DelegateCommand(delegate { ResponseCollection.Clear(); });

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

            _serialPortManager.DataReceivedAction += delegate(byte[] bytes)
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    ResponseCollection.Add(BitConverter.ToString(bytes));
                });
            };

            _serialPortManager.ErrorReceivedEventHandler += delegate(object sender, SerialErrorReceivedEventArgs args)
            {
            };
        }
    }
}