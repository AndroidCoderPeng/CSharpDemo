using System;
using System.Linq;
using System.Numerics;
using Accord.Math;

namespace CSharpDemo.Utils
{
    public class AudioVisualizer
    {
        /// <summary>
        /// 界面刷新的音频帧
        /// </summary>
        public double[] FrameBuffer { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">控制频谱数量，数量越多，界面显示波动的频谱越多，建议256就好</param>
        /// <exception cref="ArgumentException"></exception>
        public AudioVisualizer(int size = 256)
        {
            if (!size.IsPowerOfTwo())
            {
                throw new ArgumentException("长度必须是 2 的 n 次幂");
            }

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
        /// 获取频谱数据 (数据已经删去共轭部分)
        /// </summary>
        /// <returns></returns>
        public double[] GetSpectrumData()
        {
            var len = FrameBuffer.Length;
            var data = new Complex[len];

            for (var i = 0; i < len; i++)
            {
                data[i] = new Complex(FrameBuffer[i], 0);
            }

            FourierTransform.FFT(data, FourierTransform.Direction.Forward);

            var halfLen = len / 2;
            var result = new double[halfLen]; // 傅里叶变换结果左右对称, 只需要取一半
            for (var i = 0; i < halfLen; i++)
            {
                result[i] = data[i].Magnitude / len;
            }

            // var window = new Bartlett();
            // window.Create(halfLen);
            // window.ApplyInPlace(result);

            return result;
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

        /// <summary>
        /// 简单的数据模糊
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="radius">模糊半径</param>
        /// <returns>结果</returns>
        public static double[] MakeSmooth(double[] data, int radius)
        {
            var weights = GetWeights(radius);
            var buffer = new double[1 + radius * 2];

            var result = new double[data.Length];
            if (data.Length < radius)
            {
                data.Average();
                result.SetValue(data, 0);
                return result;
            }

            for (var i = 0; i < radius; i++)
            {
                // Array.Fill(buffer, data[i], 0, radius + 1); // 填充缺省
                for (var j = 0; j < radius; j++) // 
                {
                    buffer[radius + 1 + j] = data[i + j];
                }

                ApplyWeights(buffer, weights);
                result[i] = Enumerable.Sum(buffer);
            }

            for (var i = radius; i < data.Length - radius; i++)
            {
                for (var j = 0; j < radius; j++) // 
                {
                    buffer[j] = data[i - j];
                }

                buffer[radius] = data[i];

                for (var j = 0; j < radius; j++) // 
                {
                    buffer[radius + j + 1] = data[i + j];
                }

                ApplyWeights(buffer, weights);
                result[i] = Enumerable.Sum(buffer);
            }

            for (var i = data.Length - radius; i < data.Length; i++)
            {
                // Array.Fill(buffer, data[i], 0, radius + 1); // 填充缺省
                for (var j = 0; j < radius; j++) // 
                {
                    buffer[radius + 1 + j] = data[i - j];
                }

                ApplyWeights(buffer, weights);
                result[i] = Enumerable.Sum(buffer);
            }

            return result;
        }

        private static double[] GetWeights(int radius)
        {
            double Gaussian(double x) => Math.Pow(Math.E, -4 * x * x); // 高斯函数

            var len = 1 + radius * 2; // 长度
            var end = len - 1; // 最后的索引
            var radiusF = (double)radius; // 半径浮点数
            var weights = new double[len]; // 权重

            for (var i = 0; i <= radius; i++) // 先把右边的权重算出来
            {
                weights[radius + i] = Gaussian(i / radiusF);
            }

            for (var i = 0; i < radius; i++) // 把右边的权重拷贝到左边
            {
                weights[i] = weights[end - i];
            }

            var total = Enumerable.Sum(weights);
            for (var i = 0; i < len; i++) // 使权重合为 0
                weights[i] /= total;

            return weights;
        }

        private static void ApplyWeights(double[] buffer, double[] weights)
        {
            var len = buffer.Length;
            for (var i = 0; i < len; i++)
            {
                buffer[i] *= weights[i];
            }
        }
    }
}