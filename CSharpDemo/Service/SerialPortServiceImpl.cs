using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CSharpDemo.Utils;

namespace CSharpDemo.Service
{
    public class SerialPortServiceImpl : ISerialPortService
    {
        private class Subscription
        {
            public Action<byte[]> OnDataReceived { get; set; }
            public Action<string> OnError { get; set; }
        }

        private class PortSubscription : IDisposable
        {
            private readonly string _portKey;
            private readonly SerialPortServiceImpl _service;
            private readonly Subscription _subscription;

            public PortSubscription(string portKey, SerialPortServiceImpl service, Subscription subscription)
            {
                _portKey = portKey;
                _service = service;
                _subscription = subscription;
            }

            public void Dispose()
            {
                _service.Unsubscribe(_portKey, _subscription);
            }
        }

        private readonly ConcurrentDictionary<string, SerialPortManager> _portManagers;
        private readonly ConcurrentDictionary<string, List<Subscription>> _subscribers;
        private readonly object _lockObject = new object();
        private bool _isDisposed;

        public SerialPortServiceImpl()
        {
            _portManagers = new ConcurrentDictionary<string, SerialPortManager>();
            _subscribers = new ConcurrentDictionary<string, List<Subscription>>();
        }

        public SerialPortManager GetOrCreateManager(string portName, IFrameParserStrategy parser)
        {
            if (string.IsNullOrEmpty(portName))
                throw new ArgumentException(@"端口名称不能为空", nameof(portName));

            if (parser == null)
                throw new ArgumentNullException(nameof(parser), @"必须提供帧解析策略实现");

            var key = portName.ToUpper();

            return _portManagers.GetOrAdd(key, _ =>
            {
                var manager = new SerialPortManager(parser);

                // 绑定数据接收事件，分发给所有订阅者
                manager.RawDataReceivedEvent += (data) => DispatchData(key, data);
                manager.ErrorEvent += (error) => DispatchError(key, error);

                // 初始化订阅者列表
                lock (_lockObject)
                {
                    _subscribers.GetOrAdd(key, new List<Subscription>());
                }

                return manager;
            });
        }

        public IDisposable Subscribe(string portName, Action<byte[]> onDataReceived, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(portName))
                throw new ArgumentException(@"端口名称不能为空", nameof(portName));

            if (onDataReceived == null)
                throw new ArgumentNullException(nameof(onDataReceived));

            var key = portName.ToUpper();

            if (!_portManagers.ContainsKey(key))
            {
                throw new InvalidOperationException($"端口 {portName} 未初始化，请先调用 GetOrCreateManager");
            }

            var subscription = new Subscription
            {
                OnDataReceived = onDataReceived,
                OnError = onError
            };

            lock (_lockObject)
            {
                if (!_subscribers.ContainsKey(key))
                {
                    _subscribers[key] = new List<Subscription>();
                }

                _subscribers[key].Add(subscription);
            }

            return new PortSubscription(key, this, subscription);
        }

        public string[] GetOpenPorts()
        {
            return _portManagers.Where(kvp => kvp.Value.IsOpen).Select(kvp => kvp.Key).ToArray();
        }

        /// <summary>
        /// 分发数据给所有订阅者
        /// </summary>
        private void DispatchData(string portKey, byte[] data)
        {
            List<Subscription> subscribers;
            lock (_lockObject)
            {
                if (!_subscribers.TryGetValue(portKey, out subscribers)) return;

                subscribers = subscribers.ToList();
            }

            foreach (var subscriber in subscribers)
            {
                try
                {
                    subscriber.OnDataReceived?.Invoke(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"[串口 {portKey}] 订阅者处理数据异常: {ex.Message}");
                }
            }
        }

        public void ClearSubscribers(string portName)
        {
            var key = portName.ToUpper();

            lock (_lockObject)
            {
                if (_subscribers.TryGetValue(key, out var subscribers))
                {
                    subscribers.Clear();
                }
            }
        }

        /// <summary>
        /// 分发错误给所有订阅者
        /// </summary>
        private void DispatchError(string portKey, string error)
        {
            List<Subscription> subscribers;
            lock (_lockObject)
            {
                if (!_subscribers.TryGetValue(portKey, out subscribers)) return;

                subscribers = subscribers.ToList();
            }

            foreach (var subscriber in subscribers)
            {
                try
                {
                    subscriber.OnError?.Invoke(error);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"[串口 {portKey}] 订阅者处理错误异常: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        private void Unsubscribe(string portKey, Subscription subscription)
        {
            lock (_lockObject)
            {
                if (_subscribers.TryGetValue(portKey, out var subscribers))
                {
                    subscribers.Remove(subscription);
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            foreach (var manager in _portManagers.Values)
            {
                try
                {
                    if (manager.IsOpen)
                    {
                        manager.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"[串口 {manager.GetCurrentPort()}] 关闭串口异常: {ex.Message}");
                }

                manager.Dispose();
            }

            _portManagers.Clear();

            lock (_lockObject)
            {
                _subscribers.Clear();
            }
        }
    }
}