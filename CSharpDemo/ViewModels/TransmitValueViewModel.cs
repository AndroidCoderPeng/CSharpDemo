using System;
using System.Linq;
using System.Windows.Controls;
using CSharpDemo.Dialogs;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using NAudio.Wave;
using Prism.Commands;
using Prism.Mvvm;
using Window = System.Windows.Window;

namespace CSharpDemo.ViewModels
{
    public class TransmitValueViewModel : BindableBase
    {
        #region VM

        private string _delegateValue;

        public string DelegateValue
        {
            get => _delegateValue;
            set
            {
                _delegateValue = value;
                RaisePropertyChanged();
            }
        }

        private string _eventValue;

        public string EventValue
        {
            get => _eventValue;
            set
            {
                _eventValue = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region DelegateCommand

        public DelegateCommand<UserControl> DelegateCommand { get; }
        public DelegateCommand<UserControl> EventCommand { get; }
        public DelegateCommand<object> SelectWavFileCommand { get; }

        #endregion

        /// <summary>
        /// 委托：
        /// 1、什么页面需要值，就在此页面定义传值函数
        /// 2、在另一个页面定义委托，把值给委托
        /// </summary>
        public TransmitValueViewModel()
        {
            DelegateCommand = new DelegateCommand<UserControl>(HandleDelegate);
            EventCommand = new DelegateCommand<UserControl>(HandleEvent);
            SelectWavFileCommand = new DelegateCommand<object>(SelectWavFile);
        }

        private void HandleDelegate(UserControl control)
        {
            var dialog = new DelegateValueDialog(ShowDelegateValue)
            {
                Owner = Window.GetWindow(control)
            };
            dialog.ShowDialog();
        }

        private void HandleEvent(UserControl control)
        {
            var dialog = new EventValueDialog
            {
                Owner = Window.GetWindow(control)
            };
            dialog.ValueChangedEventHandler += Dialog_TextChangedEventHandler;
            dialog.ShowDialog();
        }

        private void ShowDelegateValue(string value)
        {
            DelegateValue = value;
        }

        private void Dialog_TextChangedEventHandler(object sender, string value)
        {
            EventValue = value;
        }

        private void SelectWavFile(object filePath)
        {
            using (var reader = new WaveFileReader((string)filePath))
            {
                var waveFormat = reader.WaveFormat;
                //常见的位数有8位、16位、24位和32位
                var bitsPerSample = waveFormat.BitsPerSample;
                if (bitsPerSample == 8 && waveFormat.Channels == 1)
                {
                    if (waveFormat.Channels != 1)
                    {
                        throw new InvalidOperationException("This example only supports 8-bit PCM mono WAV files.");
                    }

                    //8位PCM，每个采样点1个字节
                    var byteBuffer = new byte[reader.Length];
                    var bytesRead = reader.Read(byteBuffer, 0, byteBuffer.Length);
                    if (bytesRead != byteBuffer.Length)
                    {
                        throw new InvalidOperationException("Failed to read the expected number of samples.");
                    }

                    if (byteBuffer.Length % 4 != 0)
                    {
                        throw new ArgumentException();
                    }

                    var sampleBytes = new int[byteBuffer.Length / 4];
                    for (var i = 0; i < sampleBytes.Length; i++)
                    {
                        sampleBytes[i] = BitConverter.ToInt32(byteBuffer, i * 4);
                    }

                    FastFourierTransform(sampleBytes, waveFormat);
                }
                else if (bitsPerSample == 16)
                {
                    // 计算总样本数（每个样本2个字节）  
                    var totalBytes = reader.Length;
                    var totalSamples = (int)(totalBytes / 2);

                    // 为采样数据分配byte数组  
                    var byteBuffer = new byte[totalBytes];
                    var bytesRead = reader.Read(byteBuffer, 0, byteBuffer.Length);
                    if (bytesRead != byteBuffer.Length)
                    {
                        throw new InvalidOperationException("Failed to read the expected number of bytes.");
                    }

                    var sampleBytes = new int[totalSamples];
                    Buffer.BlockCopy(byteBuffer, 0, sampleBytes, 0, byteBuffer.Length);

                    FastFourierTransform(sampleBytes, waveFormat);
                }
                else if (bitsPerSample == 24)
                {
                    //24位PCM，每个采样点3个字节
                    var totalBytes = reader.Length;
                    var totalSamples = (int)(totalBytes / 3);

                    // 为采样数据分配byte数组  
                    var byteBuffer = new byte[totalBytes];
                    var bytesRead = reader.Read(byteBuffer, 0, byteBuffer.Length);
                    if (bytesRead != byteBuffer.Length)
                    {
                        throw new InvalidOperationException("Failed to read the expected number of bytes.");
                    }

                    var sampleBytes = new int[totalSamples];
                    for (var i = 0; i < totalSamples; i++)
                    {
                        //从3个字节中提取24位值  
                        sampleBytes[i] = (byteBuffer[i * 3] & 0xFF) |
                                         ((byteBuffer[i * 3 + 1] & 0xFF) << 8) |
                                         ((byteBuffer[i * 3 + 2] & 0xFF) << 16);
                    }

                    FastFourierTransform(sampleBytes, waveFormat);
                }
                else if (bitsPerSample == 32)
                {
                    //32位PCM，每个采样点4个字节
                    var totalBytes = reader.Length;
                    var totalSamples = (int)(totalBytes / 4);

                    // 为采样数据分配byte数组  
                    var byteBuffer = new byte[totalBytes];
                    var bytesRead = reader.Read(byteBuffer, 0, byteBuffer.Length);
                    if (bytesRead != byteBuffer.Length)
                    {
                        throw new InvalidOperationException("Failed to read the expected number of bytes.");
                    }

                    var sampleBytes = new int[totalSamples];
                    for (var i = 0; i < totalSamples; i++)
                    {
                        sampleBytes[i] = (byteBuffer[i * 4] & 0xFF) |
                                         ((byteBuffer[i * 4 + 1] & 0xFF) << 8) |
                                         ((byteBuffer[i * 4 + 2] & 0xFF) << 16) |
                                         ((byteBuffer[i * 4 + 3] & 0xFF) << 24);
                    }

                    FastFourierTransform(sampleBytes, waveFormat);
                }
            }
        }

        private void FastFourierTransform(int[] samples, WaveFormat format)
        {
            // 将byte数组转换为float数组，并进行归一化处理  
            var floatSamples = Array.ConvertAll(samples, i => i / 32768f);
            if ((floatSamples.Length & floatSamples.Length - 1) == 0)
            {
                throw new ArgumentException("长度必须是 2 的 n 次幂");
            }

            //将采样数据转为频域（FFT计算）
            var complexSamples = floatSamples.Select(f => new Complex32(f, 0)).ToArray();
            Fourier.Forward(complexSamples, FourierOptions.Matlab);
            var fftLength = complexSamples.Length;
            // 获取频域数据（只关心正频率部分）  
            for (var i = 0; i < complexSamples.Length / 2; i++)
            {
                // 计算频率（X轴）
                var frequency = i * (format.SampleRate / (float)fftLength);

                // 计算幅度（模）（Y轴）  
                var amplitude = Math.Sqrt(
                    Math.Pow(complexSamples[i].Real, 2) + Math.Pow(complexSamples[i].Imaginary, 2)
                );

                // 输出频率和幅度
                Console.WriteLine($@"Frequency: {frequency} Hz, Amplitude: {amplitude}");
                //Frequency: 3738.62954545455 Hz, Amplitude: 21315.7203959894
            }
        }
    }
}