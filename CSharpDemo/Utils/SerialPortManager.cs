using System;
using System.IO.Ports;
using System.Threading;

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
        }

        public void UnBoundSerialPortEvents()
        {
            _serialPort.DataReceived -= SerialPort_DataReceived;
        }

        /// <summary>
        /// 数据接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (_serialPort.BytesToRead < 4)
            {
                return;
            }

            var headerBuff = new byte[2];
            _serialPort.Read(headerBuff, 0, 2); //读取数据
            if (headerBuff[0] != 0xA3 || headerBuff[1] != 0x20) //符合规范
            {
                _serialPort.DiscardInBuffer();
            }
            else
            {
                ReadFromSerialPort(headerBuff);
            }
        }

        private void ReadFromSerialPort(byte[] header)
        {
            var lengthBuffer = new byte[2];
            _serialPort.Read(lengthBuffer, 0, 2);
            var length = lengthBuffer.ConvertToInt();

            if (length < 12)
            {
                _serialPort.DiscardInBuffer(); //长度数据不符合，丢弃
            }
            else
            {
                while (_serialPort.BytesToRead < length + 2) //数据不够，要等待
                {
                    Thread.Sleep(20);
                }

                var result = new byte[length + 6];
                result[0] = header[0];
                result[1] = header[1];
                result[2] = lengthBuffer[0];
                result[3] = lengthBuffer[1];
                _serialPort.Read(result, 4, result.Length - 4);

                DataReceivedAction?.Invoke(result);
            }
        }
    }
}