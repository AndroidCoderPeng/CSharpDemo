using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Windows;
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

    public class SerialPortManager : IDisposable
    {
        private readonly SerialPort _serialPort = new SerialPort();
        public event Action<(int, string, List<Tag>)> DataReceivedEvent;

        public bool SetConfiguration(string portName, string baudRate, string parity, string dataBits, string stopBit)
        {
            if (IsOpen)
            {
                MessageBox.Show("串口已打开，请先关闭串口再进行配置", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(portName) || string.IsNullOrEmpty(baudRate) || string.IsNullOrEmpty(parity) ||
                string.IsNullOrEmpty(dataBits) || string.IsNullOrEmpty(stopBit))
            {
                MessageBox.Show("串口配置不完整，请检查", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            _serialPort.PortName = portName;
            _serialPort.BaudRate = int.Parse(baudRate);
            _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
            _serialPort.DataBits = int.Parse(dataBits);
            _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBit);
            SubscribeSerialPortEvent();
            return true;
        }

        public string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }

        public bool IsOpen => _serialPort.IsOpen;

        public void Open()
        {
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

        private void SubscribeSerialPortEvent()
        {
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void UnsubscribeSerialPortEvent()
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

        public void Dispose()
        {
            DataReceivedEvent = null;
            _serialPort.Dispose();
        }
    }
}