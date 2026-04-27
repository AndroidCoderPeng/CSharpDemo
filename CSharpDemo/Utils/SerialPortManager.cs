using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using CSharpDemo.Tags;

namespace CSharpDemo.Utils
{
    internal static class FrameConst
    {
        public const byte Sync1 = 0xA3;
        public const byte Sync2 = 0x20;

        public const int HeaderLength = 4;
        public const int MinFrameLength = 12;

        public const int StatusFrameLength = 32;
        public const int CorrelatorFrameLength = 11293;
        public const int NoiseListenFrameLength = 15024;
    }

    public class SerialPortManager
    {
        #region 变量

        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        private readonly SerialPort _serialPort = new SerialPort();
        public event Action<(int, string, List<Tag>)> DataReceivedEvent;

        #endregion

        public SerialPortManager()
        {
            BoundSerialPortEvents();
        }

        public SerialPortManager(string portName, int baudRate, string parity, int dataBits, string stopBits)
        {
            PortName = portName;
            BaudRate = baudRate;
            Parity = (Parity)Enum.Parse(typeof(Parity), parity);
            DataBits = dataBits;
            StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
            BoundSerialPortEvents();
        }

        public string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }

        public bool IsOpen => _serialPort.IsOpen;

        public void Open()
        {
            if (_serialPort.IsOpen) return;

            _serialPort.PortName = PortName;
            _serialPort.BaudRate = BaudRate;
            _serialPort.Parity = Parity;
            _serialPort.DataBits = DataBits;
            _serialPort.StopBits = StopBits;

            _serialPort.Open();
        }

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

        public void Write(byte[] buffer)
        {
            if (!_serialPort.IsOpen) _serialPort.Open();
            _serialPort.Write(buffer, 0, buffer.Length);
        }

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
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (!TryParseFrame(out var frame))
                    return;

                if (!CrcCodeHub.CheckCrc16Code(frame))
                {
                    Console.WriteLine(@"CRC校验失败");
                    return;
                }

                HandleFrame(frame);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private bool TryParseFrame(out byte[] frame)
        {
            frame = null;

            if (_serialPort.BytesToRead < FrameConst.HeaderLength)
                return false;

            // 读取头部
            var header = new byte[FrameConst.HeaderLength];
            _serialPort.Read(header, 0, header.Length);

            if (header[0] != FrameConst.Sync1 || header[1] != FrameConst.Sync2)
            {
                Console.WriteLine(@"串口数据头部校验失败");
                _serialPort.DiscardInBuffer();
                return false;
            }

            var length = (header[2] << 8) | header[3];
            if (length < FrameConst.MinFrameLength)
            {
                _serialPort.DiscardInBuffer();
                return false;
            }

            var totalLength = length + 6;

            if (!WaitForBytes(totalLength))
                return false;

            frame = new byte[totalLength];
            Buffer.BlockCopy(header, 0, frame, 0, header.Length);
            _serialPort.Read(frame, header.Length, totalLength - header.Length);

            return true;
        }

        private bool WaitForBytes(int count)
        {
            var timeout = DateTime.Now.AddMilliseconds(200);

            while (_serialPort.BytesToRead < count)
            {
                if (DateTime.Now > timeout)
                {
                    Console.WriteLine(@"串口数据接收超时");
                    _serialPort.DiscardInBuffer();
                    return false;
                }

                Thread.Sleep(1);
            }

            return true;
        }

        private void HandleFrame(byte[] frame)
        {
            var deviceId = ParseDeviceId(frame);
            var tags = ParseTags(frame);

            if (frame.Length == FrameConst.StatusFrameLength)
            {
                DataReceivedEvent?.Invoke((0, deviceId, tags));
            }
            else if (frame.Length == FrameConst.CorrelatorFrameLength)
            {
                DataReceivedEvent?.Invoke((1, deviceId, tags));
            }
            else if (frame.Length == FrameConst.NoiseListenFrameLength)
            {
                DataReceivedEvent?.Invoke((2, deviceId, tags));
            }
            else
            {
                Console.WriteLine($@"未知帧长度：{frame.Length}");
            }
        }

        private string ParseDeviceId(byte[] frame)
        {
            var idBytes = new byte[6];
            Buffer.BlockCopy(frame, 4, idBytes, 0, 6);
            return BitConverter.ToString(idBytes).Replace("-", "");
        }

        private List<Tag> ParseTags(byte[] frame)
        {
            var tagBytes = new byte[frame.Length - 18];
            Buffer.BlockCopy(frame, 16, tagBytes, 0, tagBytes.Length);
            return tagBytes.GetTags();
        }
    }
}