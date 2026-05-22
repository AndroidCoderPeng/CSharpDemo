using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using CSharpDemo.Model;

namespace CSharpDemo.Utils
{
    public class CorrelatorFrameParser : IFrameParserStrategy
    {
        private const byte Sync1 = 0xA3;
        private const byte Sync2 = 0x20;
        private const int HeaderLength = 4;
        private const int MinFrameLength = 12;
        private const int MaxFrameLength = 65535 + 6; // 最大帧长度

        // 接收缓冲区
        private readonly List<byte> _receiveBuffer = new List<byte>();
        private readonly object _bufferLock = new object();

        public bool TryReadFrame(SerialPort serialPort, out byte[] frame)
        {
            frame = null;

            // 读取所有可用数据到缓冲区
            var bytesToRead = serialPort.BytesToRead;
            if (bytesToRead <= 0) return TryParseFrameFromBuffer(out frame);
            
            var buffer = new byte[bytesToRead];
            serialPort.Read(buffer, 0, buffer.Length);

            lock (_bufferLock)
            {
                _receiveBuffer.AddRange(buffer);
            }

            // 从缓冲区中尝试解析帧
            return TryParseFrameFromBuffer(out frame);
        }

        private bool TryParseFrameFromBuffer(out byte[] frame)
        {
            frame = null;

            lock (_bufferLock)
            {
                if (_receiveBuffer.Count < HeaderLength)
                    return false;

                // 查找帧头
                var headerIndex = FindFrameHeader();
                if (headerIndex < 0)
                {
                    // 没有找到帧头，清空缓冲区
                    _receiveBuffer.Clear();
                    return false;
                }

                // 跳过帧头前的无效数据
                if (headerIndex > 0)
                {
                    _receiveBuffer.RemoveRange(0, headerIndex);
                }

                // 检查是否有足够数据读取长度域
                if (_receiveBuffer.Count < HeaderLength) return false;

                // 解析数据长度
                var dataLength = (_receiveBuffer[2] << 8) | _receiveBuffer[3];

                // 验证长度合法性
                if (dataLength < MinFrameLength || dataLength > MaxFrameLength - 6)
                {
                    Console.WriteLine($@"数据长度异常: {dataLength}");
                    _receiveBuffer.Clear();
                    return false;
                }

                var totalLength = dataLength + 6;

                // 数据未接收完整，等待更多数据
                if (_receiveBuffer.Count < totalLength) return false; 

                // 提取完整帧
                frame = new byte[totalLength];
                _receiveBuffer.CopyTo(0, frame, 0, totalLength);

                // 从缓冲区移除已处理的帧
                _receiveBuffer.RemoveRange(0, totalLength);

                return true;
            }
        }
        
        private int FindFrameHeader()
        {
            for (var i = 0; i <= _receiveBuffer.Count - HeaderLength; i++)
            {
                if (_receiveBuffer[i] == Sync1 && _receiveBuffer[i + 1] == Sync2)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool ValidateFrame(byte[] frame)
        {
            if (frame == null || frame.Length < MinFrameLength)
                return false;

            return CrcCode.CheckCrc16(frame);
        }

        public T ParseFrame<T>(byte[] frame)
        {
            if (frame == null) return default;

            // 处理单个对象
            if (typeof(T) == typeof(BasePacket))
            {
                var packets = GetDataPackets(frame);
                return packets.Count > 0 ? (T)(object)packets.First() : default;
            }

            // 处理集合
            if (typeof(T) == typeof(List<BasePacket>))
            {
                var packets = GetDataPackets(frame);
                return (T)(object)packets;
            }

            throw new NotSupportedException($"不支持的类型: {typeof(T).Name}");
        }

        private List<BasePacket> GetDataPackets(byte[] bytes)
        {
            var packets = new List<BasePacket>();
            try
            {
                var i = 0;
                while (i < bytes.Length)
                {
                    // 检查是否有足够的数据读取头部
                    if (i + 6 > bytes.Length) break;

                    var packet = new BasePacket();

                    var oidBytes = new byte[4];
                    Array.Copy(bytes, i, oidBytes, 0, 4);
                    packet.Oid = oidBytes.ConvertToString();

                    // value域的长度
                    var lengthBytes = new byte[2];
                    Array.Copy(bytes, i + 4, lengthBytes, 0, 2);
                    Array.Reverse(lengthBytes);
                    packet.Length = BitConverter.ToInt16(lengthBytes, 0);

                    // 检查是否有足够的数据读取Value域
                    if (i + 6 + packet.Length > bytes.Length) break;

                    var valueBytes = new byte[packet.Length];
                    Array.Copy(bytes, i + 6, valueBytes, 0, packet.Length);
                    packet.DataValue = valueBytes;

                    i += 6 + packet.Length;
                    packets.Add(packet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"解析数据帧异常: {ex.Message}");
            }

            return packets;
        }
    }
}