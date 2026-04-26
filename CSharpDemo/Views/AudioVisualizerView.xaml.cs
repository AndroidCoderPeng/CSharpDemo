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
        private Color _color1;
        private Color _color2;
        private double _rotation; // 旋转角度
        private double _bassScale;

        public AudioVisualizerView(IAppDataService dataService)
        {
            InitializeComponent();

            _allColors = dataService.GetHsvColors(); // 获取所有的渐变颜色 (HSV 颜色)
            _visualizer = new AudioVisualizer(SampleRate, false);
            _visualizer.TimeDomainEvent += Handle_TimeDomainEvent;
            _visualizer.FrequencyDomainEvent += Handle_FrequencyDomainEvent;
            _visualizer.RenderCountEvent += Handle_RenderCountEvent;

            SelectAudioButton.Click += SelectAudioButton_Click;
        }

        private void Handle_TimeDomainEvent(TimeDomainData timeDomain)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                _bassScale.DrawGradientBorder(
                    TopBorder, BottomBorder, LeftBorder, RightBorder,
                    Color.FromArgb(0, _color1.R, _color1.G, _color1.B), _color2, 10
                );

                var curveBrush = new SolidColorBrush(_color1);
                timeDomain.DrawGradientCurve(
                    AudioCurvePanel.ActualWidth, AudioCurvePanel.ActualHeight,
                    AudioCurvePath, curveBrush, 0
                );
            }));
        }

        private void Handle_FrequencyDomainEvent(FrequencyDomainData frequencyDomain)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                _bassScale = _visualizer.CalculateBassScale(frequencyDomain);
                var highScale = _visualizer.CalculateHighScale(frequencyDomain);
                var baseRadius = Math.Min(AudioCircularPanel.ActualWidth, AudioCircularPanel.ActualHeight) / 3;
                var radius = baseRadius + highScale * _bassScale;

                frequencyDomain.DrawCircularGradientStrips(
                    AudioCircularPanel.ActualHeight, CircularPath, _color1, _color2,
                    CircularPath.ActualWidth / 2,
                    CircularPath.ActualHeight / 2,
                    radius, 1, _rotation
                );

                frequencyDomain.DrawGradientStrips(
                    AudioStripPanel.ActualWidth, AudioStripPanel.ActualHeight,
                    StripsPath, _color1, _color2, 0, 2
                );
            }));
        }

        private void Handle_RenderCountEvent(int renderCount)
        {
            _rotation += .1;
            _color1 = _allColors[renderCount % _allColors.Length];
            _color2 = _allColors[(renderCount + 200) % _allColors.Length];
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