using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using CSharpDemo.Model;

namespace CSharpDemo.Utils
{
    public class CorrelatorFrameParser : IFrameParserStrategy
    {
        private const byte Sync1 = 0xA3;
        private const byte Sync2 = 0x20;
        private const int HeaderLength = 4;
        private const int MinFrameLength = 12;

        public bool TryReadFrame(SerialPort serialPort, out byte[] frame)
        {
            frame = null;

            if (serialPort.BytesToRead < HeaderLength)
                return false;

            var header = new byte[HeaderLength];
            serialPort.Read(header, 0, header.Length);

            if (header[0] != Sync1 || header[1] != Sync2)
            {
                serialPort.DiscardInBuffer();
                return false;
            }

            var dataLength = (header[2] << 8) | header[3];
            if (dataLength < MinFrameLength)
            {
                serialPort.DiscardInBuffer();
                return false;
            }

            var totalLength = dataLength + 6;

            if (!WaitForBytes(serialPort, totalLength - HeaderLength))
                return false;

            frame = new byte[totalLength];
            Buffer.BlockCopy(header, 0, frame, 0, header.Length);
            serialPort.Read(frame, header.Length, totalLength - header.Length);

            return true;
        }

        private bool WaitForBytes(SerialPort serialPort, int count)
        {
            var timeout = DateTime.Now.AddMilliseconds(200);

            while (serialPort.BytesToRead < count)
            {
                if (DateTime.Now > timeout)
                {
                    serialPort.DiscardInBuffer();
                    return false;
                }

                Thread.Sleep(1);
            }

            return true;
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