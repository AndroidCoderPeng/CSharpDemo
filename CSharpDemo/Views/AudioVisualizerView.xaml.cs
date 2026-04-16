using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CSharpDemo.Model;
using CSharpDemo.Service;
using CSharpDemo.Utils;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace CSharpDemo.Views
{
    public partial class AudioVisualizerView : UserControl
    {
        private const int SampleRate = 7500;
        private readonly Color[] _allColors;
        private readonly AudioVisualizer _visualizer; // 可视化
        private readonly WasapiCapture _audioCapture; // 音频捕获
        private FrequencyDomainData _frequencyDomain; // 频域数据
        private TimeDomainData _timeDomain; // 时域数据
        private int _colorIndex;
        private double _rotation; // 旋转角度

        public AudioVisualizerView(IAppDataService dataService)
        {
            InitializeComponent();

            _allColors = dataService.GetHsvColors(); // 获取所有的渐变颜色 (HSV 颜色)
            _visualizer = new AudioVisualizer(SampleRate);

            _audioCapture = new WasapiLoopbackCapture(); // 捕获电脑发出的声音
            _audioCapture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1); // 7500Hz 采样率，单声道
            _audioCapture.DataAvailable += delegate(object o, WaveInEventArgs args)
            {
                var length = args.BytesRecorded / 4; // 采样的数量 (每一个采样是 4 字节)
                var audioBuffer = new float[length];
                for (var i = 0; i < length; i++)
                {
                    audioBuffer[i] = BitConverter.ToSingle(args.Buffer, i * 4);
                }

                // 将新的采样存储到 可视化器 中
                _visualizer.PushAudioData(audioBuffer);
            };

            _dataTimer.Tick += DataTimer_Tick; // 定时取频域（频谱）数据
            _drawingTimer.Tick += DrawingTimer_Tick; // 定时绘制频谱
        }

        private readonly DispatcherTimer _dataTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 25)
        };

        private readonly DispatcherTimer _drawingTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 25)
        };

        private void AudioWaveView_OnLoaded(object sender, RoutedEventArgs e)
        {
            _audioCapture.StartRecording();
            _dataTimer.Start();
            _drawingTimer.Start();
        }

        private void AudioWaveView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _drawingTimer.Stop();
            _dataTimer.Stop();
            _audioCapture.StopRecording();
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
                AudioWavePanel.ActualWidth, AudioWavePanel.ActualHeight, AudioWavePath, curveBrush, 0
            );

            //长条形波动图
            _frequencyDomain.DrawGradientStrips(
                AudioStripPanel.ActualWidth, AudioStripPanel.ActualHeight, StripsPath, color1, color2, 0, 2
            );
        }
    }
}