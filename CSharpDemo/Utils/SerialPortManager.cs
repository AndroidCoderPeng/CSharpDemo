using System;
using System.IO.Ports;

namespace CSharpDemo.Utils
{
    public class SerialPortManager : IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly IFrameParserStrategy _parser;
        private readonly object _lockObject = new object();
        private bool _isDisposed;

        public event Action<byte[]> RawDataReceivedEvent;
        public event Action<string> ErrorEvent;

        public SerialPortManager(IFrameParserStrategy parserStrategy = null)
        {
            _parser = parserStrategy ?? throw new ArgumentNullException(nameof(parserStrategy), @"必须提供帧解析策略实现");
            _serialPort = new SerialPort();
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }
        
        public bool SetConfiguration(string portName, string baudRate, string parity, string dataBits, string stopBit)
        {
            if (IsOpen)
            {
                ErrorEvent?.Invoke("串口已打开，请先关闭串口再进行配置");
                return false;
            }

            try
            {
                lock (_lockObject)
                {
                    _serialPort.PortName = portName;
                    _serialPort.BaudRate = int.Parse(baudRate);
                    _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                    _serialPort.DataBits = int.Parse(dataBits);
                    _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBit);
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke($"串口配置解析失败: {ex.Message}");
                return false;
            }
        }

        public bool IsOpen
        {
            get
            {
                lock (_lockObject)
                {
                    return _serialPort.IsOpen;
                }
            }
        }

        public SerialPort GetCurrentPort()
        {
            lock (_lockObject)
            {
                return !_serialPort.IsOpen ? null : _serialPort;
            }
        }

        public void Open()
        {
            lock (_lockObject)
            {
                if (_serialPort.IsOpen) return;
                _serialPort.Open();
            }
        }

        public void Close()
        {
            lock (_lockObject)
            {
                if (!_serialPort.IsOpen) return;
                _serialPort.Close();
            }
        }

        public void Write(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                ErrorEvent?.Invoke("发送数据为空");
                return;
            }

            lock (_lockObject)
            {
                if (!_serialPort.IsOpen)
                {
                    ErrorEvent?.Invoke("串口未打开");
                    return;
                }

                _serialPort.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// 数据接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_isDisposed) return;

            try
            {
                byte[] frame;
                lock (_lockObject)
                {
                    if (!_serialPort.IsOpen) return;
                    if (!_parser.TryReadFrame(_serialPort, out frame)) return;
                }

                if (!_parser.ValidateFrame(frame)) return;

                RawDataReceivedEvent?.Invoke(frame);
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke($"数据接收处理失败: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }

            _serialPort.DataReceived -= SerialPort_DataReceived;
            RawDataReceivedEvent = null;
            ErrorEvent = null;
            _serialPort.Dispose();
        }
    }
}