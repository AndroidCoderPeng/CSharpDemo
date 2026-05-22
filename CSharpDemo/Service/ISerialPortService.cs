using System;
using CSharpDemo.Utils;

namespace CSharpDemo.Service
{
    public interface ISerialPortService : IDisposable
    {
        /// <summary>
        /// 获取或创建串口管理器
        /// </summary>
        SerialPortManager GetOrCreateManager(string portName, IFrameParserStrategy parser);

        /// <summary>
        /// 订阅指定端口的数据
        /// </summary>
        IDisposable Subscribe(string portName, Action<byte[]> onDataReceived, Action<string> onError = null);

        /// <summary>
        /// 获取所有已打开的端口
        /// </summary>
        string[] GetOpenPorts();

        /// <summary>
        /// 移除端口的所有订阅者（但不关闭物理串口）
        /// </summary>
        void ClearSubscribers(string portName);
    }
}