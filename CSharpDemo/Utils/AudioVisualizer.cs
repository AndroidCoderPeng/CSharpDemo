using System;
using System.Collections.Generic;
using System.Numerics;
using Accord.Math;
using CSharpDemo.Model;

namespace CSharpDemo.Utils
{
    public class AudioVisualizer
    {
        /// <summary>
        /// 界面刷新的音频帧
        /// </summary>
        private readonly double[] _frameBuffer;

        private readonly double _sampleRate;
        private Complex[] _fftBuffer;
        private double _minBass = double.MaxValue;
        private double _maxBass = double.MinValue;
        private double _minHigh = double.MaxValue;
        private double _maxHigh = double.MinValue;
        private const int HistorySize = 60;
        private readonly Queue<double> _bassHistory = new Queue<double>();
        private readonly Queue<double> _highHistory = new Queue<double>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <param name="size">控制频谱数量，数量越多，界面显示波动的频谱越多，建议256就好</param>
        /// <exception cref="ArgumentException"></exception>
        public AudioVisualizer(double sampleRate, int size = 256)
        {
            if (!size.IsPowerOfTwo())
            {
                throw new ArgumentException("长度必须是 2 的 n 次幂");
            }

            _sampleRate = sampleRate;
            _frameBuffer = new double[size];
        }

        /// <summary>
        /// 保持固定大小的数据窗口，始终显示最新的音频采样
        /// </summary>
        /// <param name="audioData"></param>
        public void PushAudioData(float[] audioData)
        {
            if (audioData.Length > _frameBuffer.Length)
            {
                // 数据超长时：只保留最新的 SampleData.Length 个采样（从尾部截取）
                Array.Copy(audioData, audioData.Length - _frameBuffer.Length, _frameBuffer, 0, _frameBuffer.Length);
            }
            else
            {
                // 数据不足时：将旧数据左移腾出空间，新数据追加到末尾
                Array.Copy(_frameBuffer, audioData.Length, _frameBuffer, 0, _frameBuffer.Length - audioData.Length);
                Array.Copy(audioData, 0, _frameBuffer, _frameBuffer.Length - audioData.Length, audioData.Length);
            }
        }

        /// <summary>
        /// 获取频谱数据，也就是音频对应的时域数据
        /// </summary>
        /// <returns></returns>
        public TimeDomainData GetTimeDomain()
        {
            var len = _frameBuffer.Length;
            if (len <= 1)
            {
                return new TimeDomainData
                {
                    TimeAxis = Array.Empty<double>(),
                    Amplitude = Array.Empty<double>()
                };
            }

            var timeAxis = new double[len];
            var sampleInterval = 1.0 / _sampleRate;
            for (var i = 0; i < len; i++)
            {
                timeAxis[i] = i * sampleInterval;
            }

            return new TimeDomainData
            {
                TimeAxis = timeAxis,
                Amplitude = (double[])_frameBuffer.Clone()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="freqData"></param>
        /// <returns></returns>
        public double CalculateBassScale(FrequencyDomainData freqData)
        {
            if (freqData?.Frequencies == null || freqData.Magnitudes == null || freqData.Frequencies.Length == 0)
            {
                return 1.0;
            }

            const double bassThreshold = 200;
            var count = 0;
            double sum = 0;

            for (var i = 0; i < freqData.Frequencies.Length; i++)
            {
                if (freqData.Frequencies[i] <= bassThreshold && freqData.Frequencies[i] >= 0)
                {
                    sum += Math.Abs(freqData.Magnitudes[i]);
                    count++;
                }
            }

            var avgBass = count > 0 ? sum / count : 0;

            _bassHistory.Enqueue(avgBass);
            if (_bassHistory.Count > HistorySize)
            {
                _bassHistory.Dequeue();
            }

            if (_bassHistory.Count > 10)
            {
                _minBass = double.MaxValue;
                _maxBass = double.MinValue;
                foreach (var val in _bassHistory)
                {
                    if (val < _minBass) _minBass = val;
                    if (val > _maxBass) _maxBass = val;
                }
            }

            var range = _maxBass - _minBass;
            if (range < 0.0001)
            {
                return 1.0;
            }

            var normalized = (avgBass - _minBass) / range;
            var scale = 0.8 + normalized * 0.8;

            return Math.Max(0.8, Math.Min(scale, 1.6));
        }

        public double CalculateHighScale(FrequencyDomainData freqData)
        {
            if (freqData?.Frequencies == null || freqData.Magnitudes == null || freqData.Frequencies.Length == 0)
            {
                return 1.0;
            }

            const double highThreshold = 1500;
            var count = 0;
            double sum = 0;

            for (var i = 0; i < freqData.Frequencies.Length; i++)
            {
                if (freqData.Frequencies[i] >= highThreshold)
                {
                    sum += Math.Abs(freqData.Magnitudes[i]);
                    count++;
                }
            }

            var avgHigh = count > 0 ? sum / count : 0;

            _highHistory.Enqueue(avgHigh);
            if (_highHistory.Count > HistorySize)
            {
                _highHistory.Dequeue();
            }

            if (_highHistory.Count > 10)
            {
                _minHigh = double.MaxValue;
                _maxHigh = double.MinValue;
                foreach (var val in _highHistory)
                {
                    if (val < _minHigh) _minHigh = val;
                    if (val > _maxHigh) _maxHigh = val;
                }
            }

            var range = _maxHigh - _minHigh;
            if (range < 0.0001)
            {
                return 1.0;
            }

            var normalized = (avgHigh - _minHigh) / range;
            var scale = 0.8 + normalized * 0.8;

            return Math.Max(0.8, Math.Min(scale, 1.6));
        }

        /// <summary>
        /// 获取频谱数据，也就是音频对应的频域数据
        /// </summary>
        /// <returns></returns>
        public FrequencyDomainData GetFrequencyDomain()
        {
            var len = _frameBuffer.Length;
            if (len <= 1)
            {
                return new FrequencyDomainData
                {
                    Frequencies = Array.Empty<double>(),
                    Magnitudes = Array.Empty<double>()
                };
            }

            _fftBuffer = new Complex[len];
            for (var i = 0; i < len; i++)
            {
                _fftBuffer[i] = new Complex(_frameBuffer[i], 0);
            }

            var fftSize = _fftBuffer.Length;
            FourierTransform.FFT(_fftBuffer, FourierTransform.Direction.Forward);

            // 傅里叶变换结果左右对称, 只需要取一半
            var binCount = fftSize / 2;
            var frequencies = new double[binCount];
            var magnitudes = new double[binCount];
            for (var i = 0; i < binCount; i++)
            {
                frequencies[i] = i * _sampleRate / fftSize;
                magnitudes[i] = _fftBuffer[i].Magnitude / fftSize;
            }

            return new FrequencyDomainData
            {
                Frequencies = frequencies,
                Magnitudes = magnitudes
            };
        }

        /// <summary>
        /// 数据模糊
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="radius">模糊半径</param>
        /// <returns>结果</returns>
        public FrequencyDomainData MakeSmooth(FrequencyDomainData data, int radius)
        {
            if (data?.Magnitudes == null || data.Magnitudes.Length == 0)
            {
                return data;
            }

            var magnitudes = data.Magnitudes;
            var smoothed = new double[magnitudes.Length];
            var weights = radius.GetWeights();

            for (var i = 0; i < magnitudes.Length; i++)
            {
                var start = Math.Max(0, i - radius);
                var end = Math.Min(magnitudes.Length - 1, i + radius);

                double sum = 0;
                double weightSum = 0;
                for (var j = start; j <= end; j++)
                {
                    var weightIndex = j - start;
                    sum += magnitudes[j] * weights[weightIndex];
                    weightSum += weights[weightIndex];
                }

                smoothed[i] = sum / weightSum;
            }

            return new FrequencyDomainData
            {
                Frequencies = data.Frequencies,
                Magnitudes = smoothed
            };
        }

        public TimeDomainData MakeSmooth(TimeDomainData data, int radius)
        {
            if (data?.Amplitude == null || data.Amplitude.Length == 0)
            {
                return data;
            }

            var amplitude = data.Amplitude;
            var smoothed = new double[amplitude.Length];
            var weights = radius.GetWeights();

            for (var i = 0; i < amplitude.Length; i++)
            {
                var start = Math.Max(0, i - radius);
                var end = Math.Min(amplitude.Length - 1, i + radius);

                double sum = 0;
                double weightSum = 0;
                for (var j = start; j <= end; j++)
                {
                    var weightIndex = j - start;
                    sum += amplitude[j] * weights[weightIndex];
                    weightSum += weights[weightIndex];
                }

                smoothed[i] = sum / weightSum;
            }

            return new TimeDomainData
            {
                TimeAxis = data.TimeAxis,
                Amplitude = smoothed
            };
        }
    }
}