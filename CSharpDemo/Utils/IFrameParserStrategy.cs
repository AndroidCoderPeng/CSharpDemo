using System.IO.Ports;

namespace CSharpDemo.Utils
{
    public interface IFrameParserStrategy
    {
        /// <summary>
        /// 尝试从串口读取并解析一帧数据
        /// </summary>
        /// <param name="serialPort">串口对象</param>
        /// <param name="frame">解析出的完整帧</param>
        /// <returns>是否成功解析</returns>
        bool TryReadFrame(SerialPort serialPort, out byte[] frame);

        /// <summary>
        /// 验证帧数据（CRC等校验）
        /// </summary>
        /// <param name="frame">完整帧数据</param>
        /// <returns>是否通过校验</returns>
        bool ValidateFrame(byte[] frame);

        /// <summary>
        /// 从帧数据中解析业务对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="frame">完整帧数据</param>
        /// <returns>解析后的业务对象</returns>
        T ParseFrame<T>(byte[] frame);
    }
}