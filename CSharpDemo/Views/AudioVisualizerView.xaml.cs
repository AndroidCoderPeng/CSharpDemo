using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CSharpDemo.Model;
using CSharpDemo.Service;
using CSharpDemo.Utils;
using Microsoft.Win32;
using NAudio.Wave;

namespace CSharpDemo.Views
{
    public partial class AudioVisualizerView : UserControl
    {
        private const int SampleRate = 7500;
        private readonly Color[] _allColors;
        private readonly AudioVisualizer _visualizer; // 可视化
        private FrequencyDomainData _frequencyDomain; // 频域数据
        private TimeDomainData _timeDomain; // 时域数据
        private int _colorIndex;
        private double _rotation; // 旋转角度

        public AudioVisualizerView(IAppDataService dataService)
        {
            InitializeComponent();

            _allColors = dataService.GetHsvColors(); // 获取所有的渐变颜色 (HSV 颜色)
            _visualizer = new AudioVisualizer(SampleRate, 512);

            SelectAudioButton.Click += SelectAudioButton_Click;
        }

        private void SelectAudioButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                // 设置默认格式
                DefaultExt = ".mp3",
                Filter = "音频文件(*.mp3)|*.mp3"
            };
            var result = fileDialog.ShowDialog();
            if (result != true) return;

            var audioFilePath = fileDialog.FileName;
            AudioFilePathTextBox.Text = audioFilePath;

            // 读取音频文件并进行FFT转换
            ReadAudioFile(audioFilePath);
        }

        /// <summary>
        /// WAV - WaveFileReader
        /// MP3 - Mp3FileReader
        /// </summary>
        /// <param name="path"></param>
        private void ReadAudioFile(string path)
        {
            try
            {
                using (var reader = new Mp3FileReader(path))
                {
                    var bitsPerSample = reader.WaveFormat.BitsPerSample;
                    var channels = reader.WaveFormat.Channels;
                    var sampleRate = reader.WaveFormat.SampleRate;

                    // 音频信息: 采样率=44100Hz, 位深=16位, 声道数=2
                    Console.WriteLine($@"音频信息: 采样率={sampleRate}Hz, 位深={bitsPerSample}位, 声道数={channels}");

                    // 创建重采样流，将音频转换为目标采样率和单声道
                    var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1);
                    using (var resampler = new MediaFoundationResampler(reader, waveFormat))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            var sampleCount = bytesRead / 4;
                            var samples = new float[sampleCount];

                            for (var i = 0; i < sampleCount; i++)
                            {
                                // 输出格式为 IEEE Float（32位浮点），每个样本占 4 字节
                                samples[i] = BitConverter.ToSingle(buffer, i * 4);
                            }
                            
                            _visualizer.PushAudioData(samples);
                            // var frequencyDomain = _visualizer.GetFrequencyDomain();
                            // var timeDomain = _visualizer.GetTimeDomain();
                            //
                            // _frequencyDomain = frequencyDomain;
                            // _timeDomain = timeDomain;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取音频文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 用来刷新频谱数据以及实现频谱数据缓动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTimer_Tick(object sender, EventArgs e)
        {
            // 获取频域数据
            var frequencyDomain = _visualizer.GetFrequencyDomain();
            var sFrequencyDomain = _visualizer.MakeSmooth(frequencyDomain, 2); // 平滑频谱数据

            if (_frequencyDomain == null)
            {
                _frequencyDomain = sFrequencyDomain;
                return;
            }

            for (var i = 0; i < sFrequencyDomain.Magnitudes.Length; i++)
            {
                var oldData = _frequencyDomain.Magnitudes[i];
                var newData = sFrequencyDomain.Magnitudes[i];

                // 计算旧频谱数据和新频谱数据之间的 "中间值"，每次向目标值移动 20%
                var deltaData = oldData + (newData - oldData) * 0.2;
                _frequencyDomain.Magnitudes[i] = deltaData;
            }

            // 获取时域数据
            var timeDomain = _visualizer.GetTimeDomain();
            var sTimeDomain = _visualizer.MakeSmooth(timeDomain, 2); // 平滑时域数据

            if (_timeDomain == null)
            {
                _timeDomain = sTimeDomain;
                return;
            }

            for (var i = 0; i < _timeDomain.Amplitude.Length; i++)
            {
                var oldData = _timeDomain.Amplitude[i];
                var newData = sTimeDomain.Amplitude[i];

                var deltaData = oldData + (newData - oldData) * 0.2;
                _timeDomain.Amplitude[i] = deltaData;
            }
        }

        private void DrawingTimer_Tick(object sender, EventArgs e)
        {
            if (_frequencyDomain == null || _timeDomain == null)
            {
                return;
            }

            _colorIndex++;
            var color1 = _allColors[_colorIndex % _allColors.Length];
            var color2 = _allColors[(_colorIndex + 200) % _allColors.Length];

            var bassScale = _visualizer.CalculateBassScale(_frequencyDomain); // 获取低音系数
            var highScale = _visualizer.CalculateHighScale(_frequencyDomain); // 获取高音系数
            // Console.WriteLine($@"bassScale: {bassScale}, highScale: {highScale}");

            //圆形波动图
            _rotation += .1;
            var baseRadius = Math.Min(AudioCircularPanel.ActualWidth, AudioCircularPanel.ActualHeight) / 3;
            var radius = baseRadius + highScale * bassScale;
            // Console.WriteLine($@"radius: {radius}");
            _frequencyDomain.DrawCircularGradientStrips(
                AudioCircularPanel.ActualHeight, CircularPath, color1, color2,
                CircularPath.ActualWidth / 2,
                CircularPath.ActualHeight / 2,
                radius, 1, _rotation
            );

            // 四周边框
            bassScale.DrawGradientBorder(
                TopBorder, BottomBorder, LeftBorder, RightBorder,
                Color.FromArgb(0, color1.R, color1.G, color1.B), color2, 10
            );

            //波形曲线
            var curveBrush = new SolidColorBrush(color1);
            _timeDomain.DrawGradientCurve(
                AudioCurvePanel.ActualWidth, AudioCurvePanel.ActualHeight, AudioCurvePath, curveBrush, 0
            );

            //长条形波动图
            _frequencyDomain.DrawGradientStrips(
                AudioStripPanel.ActualWidth, AudioStripPanel.ActualHeight, StripsPath, color1, color2, 0, 2
            );
        }
    }
}