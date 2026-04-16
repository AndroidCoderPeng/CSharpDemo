using System;
using System.Numerics;
using Accord.Math;
using CSharpDemo.Model;

namespace CSharpDemo.Utils
{
    public class AudioVisualizer
    {
        private readonly double _sampleRate;
        private Complex[] _fftBuffer;

        /// <summary>
        /// 界面刷新的音频帧
        /// </summary>
        public double[] FrameBuffer { get; }

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
            FrameBuffer = new double[size];
        }

        /// <summary>
        /// 保持固定大小的数据窗口，始终显示最新的音频采样
        /// </summary>
        /// <param name="audioData"></param>
        public void PushAudioData(float[] audioData)
        {
            if (audioData.Length > FrameBuffer.Length)
            {
                // 数据超长时：只保留最新的 SampleData.Length 个采样（从尾部截取）
                Array.Copy(audioData, audioData.Length - FrameBuffer.Length, FrameBuffer, 0, FrameBuffer.Length);
            }
            else
            {
                // 数据不足时：将旧数据左移腾出空间，新数据追加到末尾
                Array.Copy(FrameBuffer, audioData.Length, FrameBuffer, 0, FrameBuffer.Length - audioData.Length);
                Array.Copy(audioData, 0, FrameBuffer, FrameBuffer.Length - audioData.Length, audioData.Length);
            }
        }

        /// <summary>
        /// 获取频谱数据，也就是音频对应的频域数据
        /// </summary>
        /// <returns></returns>
        public FrequencyDomainData GetFrequencyDomain()
        {
            var len = FrameBuffer.Length;
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
                _fftBuffer[i] = new Complex(FrameBuffer[i], 0);
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
        public static FrequencyDomainData MakeSmooth(FrequencyDomainData data, int radius)
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

        /// <summary>
        /// 取指定频率内的频谱数据
        /// </summary>
        /// <param name="spectrum">源频谱数据</param>
        /// <param name="sampleRate">采样率</param>
        /// <param name="frequency">目标频率</param>
        /// <returns></returns>
        public static double[] TakeSpectrumOfFrequency(double[] spectrum, double sampleRate, double frequency)
        {
            var frequencyPerSample = sampleRate / spectrum.Length;

            var lengthInNeed = (int)Math.Min(frequency / frequencyPerSample, spectrum.Length);
            var result = new double[lengthInNeed];
            Array.Copy(spectrum, 0, result, 0, lengthInNeed);
            return result;
        }
    }
}