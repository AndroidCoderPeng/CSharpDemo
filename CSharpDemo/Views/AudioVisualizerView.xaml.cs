using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CSharpDemo.Model;
using CSharpDemo.Service;
using CSharpDemo.Utils;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CSharpDemo.Views
{
    public partial class AudioVisualizerView : UserControl
    {
        private const int SampleRate = 7500;
        private readonly Color[] _allColors;
        private readonly AudioVisualizer _visualizer; // 可视化
        private IWavePlayer _wavePlayer;
        private FrequencyDomainData _frequencyDomain; // 频域数据
        private TimeDomainData _timeDomain; // 时域数据
        private int _colorIndex;
        private double _rotation; // 旋转角度

        public AudioVisualizerView(IAppDataService dataService)
        {
            InitializeComponent();

            _allColors = dataService.GetHsvColors(); // 获取所有的渐变颜色 (HSV 颜色)
            _visualizer = new AudioVisualizer(SampleRate);
            _visualizer.TimeDomainEvent += Handle_TimeDomainEvent;
            _visualizer.FrequencyDomainEvent += Handle_FrequencyDomainEvent;

            SelectAudioButton.Click += SelectAudioButton_Click;

            CompositionTarget.Rendering += Handle_RenderPathEvent;
        }

        private void Handle_TimeDomainEvent(TimeDomainData timeDomain)
        {
            // var sTimeDomain = _visualizer.MakeSmooth(timeDomain, 2);
            //
            // if (_timeDomain == null)
            // {
            //     _timeDomain = sTimeDomain;
            //     return;
            // }
            //
            // for (var i = 0; i < sTimeDomain.Amplitude.Length; i++)
            // {
            //     var oldData = _timeDomain.Amplitude[i];
            //     var newData = sTimeDomain.Amplitude[i];
            //     var deltaData = oldData + (newData - oldData) * 0.2;
            //     _timeDomain.Amplitude[i] = deltaData;
            // }
            _timeDomain = timeDomain;
        }

        private void Handle_FrequencyDomainEvent(FrequencyDomainData frequencyDomain)
        {
            // var sFrequencyDomain = _visualizer.MakeSmooth(frequencyDomain, 2);
            //
            // if (_frequencyDomain == null)
            // {
            //     _frequencyDomain = sFrequencyDomain;
            //     return;
            // }
            //
            // for (var i = 0; i < sFrequencyDomain.Magnitudes.Length; i++)
            // {
            //     var oldData = _frequencyDomain.Magnitudes[i];
            //     var newData = sFrequencyDomain.Magnitudes[i];
            //     var deltaData = oldData + (newData - oldData) * 0.2;
            //     _frequencyDomain.Magnitudes[i] = deltaData;
            // }
            _frequencyDomain = frequencyDomain;
        }

        private void Handle_RenderPathEvent(object sender, EventArgs e)
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

            ReadAndPlayAudioFile(audioFilePath);
        }

        /// <summary>
        /// WAV - WaveFileReader
        /// MP3 - Mp3FileReader
        /// </summary>
        /// <param name="path"></param>
        private void ReadAndPlayAudioFile(string path)
        {
            // 清理旧的资源
            StopAndCleanup();

            var mp3Reader = new Mp3FileReader(path);
            var sampleProvider = mp3Reader.ToSampleProvider();

            // 立体声 → 单声道
            var monoProvider = new StereoToMonoSampleProvider(sampleProvider)
            {
                LeftVolume = 0.5f,
                RightVolume = 0.5f
            };

            // 重采样为 7500Hz, 24位, 单声道
            var resampledProvider = new WdlResamplingSampleProvider(monoProvider, SampleRate);

            // 包装一层，在 Read 时回调数据
            var capturingProvider = new CapturingWaveProvider(resampledProvider);
            capturingProvider.DataCaptured += samples =>
            {
                if (samples == null || samples.Length == 0)
                {
                    return;
                }

                _visualizer.PushAudioData(samples);
            };

            _wavePlayer = new WaveOutEvent();
            _wavePlayer.Init(capturingProvider.ToWaveProvider16());
            _wavePlayer.Play();
        }

        /// <summary>
        /// 停止播放并清理资源
        /// </summary>
        private void StopAndCleanup()
        {
            if (_wavePlayer != null)
            {
                _wavePlayer.Stop();
                _wavePlayer.Dispose();
                _wavePlayer = null;
            }
        }
    }

    public class CapturingWaveProvider : ISampleProvider
    {
        private readonly ISampleProvider _provider;

        public event Action<float[]> DataCaptured;
        public WaveFormat WaveFormat => _provider.WaveFormat;

        public CapturingWaveProvider(ISampleProvider source)
        {
            _provider = source;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            // 从源读取实际音频数据
            var samplesRead = _provider.Read(buffer, offset, count);
            if (samplesRead > 0)
            {
                // 截取本次读到的新数据，回调给外部
                var captured = new float[samplesRead];
                Array.Copy(buffer, offset, captured, 0, samplesRead);
                DataCaptured?.Invoke(captured);
            }

            return samplesRead;
        }
    }
}