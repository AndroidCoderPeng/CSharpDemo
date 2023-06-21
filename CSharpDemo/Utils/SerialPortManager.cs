using System;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CSharpDemo.Utils
{
    public class SerialPortManager
    {
        #region 变量

        private string _portName;
        private int _baudRate;
        private int _dataBits;
        private Parity _parity;
        private StopBits _stopBits;

        public string PortName
        {
            get => _portName;
            set => _portName = value;
        }

        public int BaudRate
        {
            get => _baudRate;
            set => _baudRate = value;
        }

        public Parity Parity
        {
            get => _parity;
            set => _parity = value;
        }

        public int DataBits
        {
            get => _dataBits;
            set => _dataBits = value;
        }

        public StopBits StopBits
        {
            get => _stopBits;
            set => _stopBits = value;
        }

        public event Action<byte[]> DataReceivedAction;
        public event SerialErrorReceivedEventHandler ErrorReceivedEventHandler;
        private readonly SerialPort _serialPort = new SerialPort();

        #endregion

        public SerialPortManager()
        {
            BoundSerialPortEvents();
        }

        public SerialPortManager(string portName, int baudRate, string parity, int dataBits, string stopBits)
        {
            _portName = portName;
            _baudRate = baudRate;
            _parity = (Parity)Enum.Parse(typeof(Parity), parity);
            _dataBits = dataBits;
            _stopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
            BoundSerialPortEvents();
        }

        public string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// 串口是否已打开
        /// </summary>
        public bool IsOpen => _serialPort.IsOpen;

        /// <summary>
        /// 打开串口
        /// </summary>
        public void Open()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.PortName = _portName;
                _serialPort.BaudRate = _baudRate;
                _serialPort.Parity = _parity;
                _serialPort.DataBits = _dataBits;
                _serialPort.StopBits = _stopBits;

                _serialPort.Open();
            }
        }

        /// <summary>
        /// 关闭端口
        /// </summary>
        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        /// <summary>
        /// 丢弃来自串行驱动程序的接收和发送缓冲区的数据
        /// </summary>
        public void DiscardBuffer()
        {
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
        }

        #region 写入数据

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Write(byte[] buffer, int offset, int count)
        {
            if (!_serialPort.IsOpen) _serialPort.Open();
            _serialPort.Write(buffer, offset, count);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="buffer">写入端口的字节数组</param>
        public void Write(byte[] buffer)
        {
            if (!_serialPort.IsOpen) _serialPort.Open();
            _serialPort.Write(buffer, 0, buffer.Length);
        }

        #endregion

        private void BoundSerialPortEvents()
        {
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.ErrorReceived += SerialPort_ErrorReceived;
        }

        public void UnBoundSerialPortEvents()
        {
            _serialPort.DataReceived -= SerialPort_DataReceived;
            _serialPort.ErrorReceived -= SerialPort_ErrorReceived;
        }

        /// <summary>
        /// 数据接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                var receivedData = new byte[_serialPort.BytesToRead];
                _serialPort.Read(receivedData, 0, receivedData.Length);
                if (receivedData.Any() && DataReceivedAction != null)
                {
                    DataReceivedAction(receivedData);
                }
            }
            else
            {
                MessageBox.Show("串口未打开", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 错误数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            ErrorReceivedEventHandler?.Invoke(sender, e);
        }
    }
}