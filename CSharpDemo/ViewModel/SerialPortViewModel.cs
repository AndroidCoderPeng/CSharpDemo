using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;
using CSharpDemo.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HandyControl.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CSharpDemo.ViewModel
{
    public class SerialPortViewModel : ViewModelBase
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
        private string _userInputHex = "A3 20 00 13 21 17 00 08 22 01 01 22 01 0A 82 01 60 00 02 00 00 01 00 57 11";

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

        #endregion

        #region RelayCommand

        public RelayCommand<ComboBox> PortItemSelectedCommand { get; }
        public RelayCommand<ComboBox> BaudRateItemSelectedCommand { get; }
        public RelayCommand<ComboBox> DataBitItemSelectedCommand { get; }
        public RelayCommand<ComboBox> CheckModeItemSelectedCommand { get; }
        public RelayCommand<ComboBox> StopBitItemSelectedCommand { get; }
        public RelayCommand OpenSerialPortCommand { get; }
        public RelayCommand CloseSerialPortCommand { get; }
        public RelayCommand ClearMessageCommand { get; }
        public RelayCommand SendMessageCommand { get; }

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

            PortItemSelectedCommand = new RelayCommand<ComboBox>(delegate(ComboBox box)
            {
                PortName = box.SelectedItem.ToString();
            });

            BaudRateItemSelectedCommand = new RelayCommand<ComboBox>(delegate(ComboBox box)
            {
                BaudRate = int.Parse(box.SelectedItem.ToString());
            });

            DataBitItemSelectedCommand = new RelayCommand<ComboBox>(delegate(ComboBox box)
            {
                DataBit = int.Parse(box.SelectedItem.ToString());
            });

            CheckModeItemSelectedCommand = new RelayCommand<ComboBox>(delegate(ComboBox box)
            {
                Parity = (Parity)box.SelectedItem;
            });

            StopBitItemSelectedCommand = new RelayCommand<ComboBox>(delegate(ComboBox box)
            {
                StopBit = int.Parse(box.SelectedItem.ToString());
            });

            OpenSerialPortCommand = new RelayCommand(delegate
            {
                _serialPortManager.PortName = _portName;
                _serialPortManager.BaudRate = _baudRate;
                _serialPortManager.DataBits = _dataBits;
                _serialPortManager.Parity = _parity;
                _serialPortManager.StopBits = (StopBits)_stopBit;

                _serialPortManager.Open();
                StateColorBrush = _serialPortManager.IsOpen ? "LimeGreen" : "LightGray";
            });

            CloseSerialPortCommand = new RelayCommand(delegate
            {
                _serialPortManager.Close();
                StateColorBrush = "LightGray";
            });

            ClearMessageCommand = new RelayCommand(delegate { ResponseCollection.Clear(); });

            SendMessageCommand = new RelayCommand(delegate
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

                //A3 20 00 13 21 17 00 08 22 01 01 22 01 0A 82 01 60 00 02 00 00 01 00 57 11
                _serialPortManager.Write(new byte[]
                {
                    0xA3, 0x20, 0x00, 0x13, 0x21, 0x17, 0x00, 0x08, 0x22, 0x01, 0x01, 0x22, 0x01, 0x0A, 0x82, 0x01,
                    0x60, 0x00, 0x02, 0x00, 0x00, 0x01, 0x00, 0x57, 0x11
                });
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