using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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
        private double _rotation;

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
            Interval = new TimeSpan(0, 0, 0, 0, 30)
        };

        private readonly DispatcherTimer _drawingTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 30)
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
            sFrequencyDomain.Magnitudes = sFrequencyDomain.Magnitudes.ToDecibels(); // 转换为分贝显示（更符合人耳感知）

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

            _rotation += .1;
            _colorIndex++;

            var color1 = _allColors[_colorIndex % _allColors.Length];
            var color2 = _allColors[(_colorIndex + 200) % _allColors.Length];

            // 获取低音系数
            var bassScale = _visualizer.CalculateBassScale(_frequencyDomain);

            //圆形波动图
            // var extraScale = Math.Min(CirclePath.ActualWidth, CirclePath.ActualHeight) / 6; //高音区
            // DrawCircleGradientStrips(
            //     CirclePath, color1, color2,
            //     _spectrumData, _spectrumData.Length,
            //     CirclePath.ActualWidth / 2, CirclePath.ActualHeight / 2,
            //     Math.Min(CirclePath.ActualWidth, CirclePath.ActualHeight) / 3 + extraScale * bassScale,
            //     1, _rotation, CirclePath.ActualHeight * 3
            // );

            // 四周边框
            bassScale.DrawGradientBorder(
                TopBorder, BottomBorder, LeftBorder, RightBorder,
                Color.FromArgb(0, color1.R, color1.G, color1.B), color2, 10
            );

            //波形曲线
            var curveBrush = new SolidColorBrush(color1);
            _timeDomain.DrawCurve(
                AudioWavePanel.ActualWidth, AudioWavePanel.ActualHeight, AudioWavePath, curveBrush, 0
            );

            //长条形波动图
            _frequencyDomain.DrawGradientStrips(
                AudioStripPanel.ActualWidth, AudioStripPanel.ActualHeight, StripsPath, color1, color2, 0, 2
            );
        }

        /// <summary>
        /// 画圆环条
        /// </summary>
        /// <param name="wavePath"></param>
        /// <param name="inner"></param>
        /// <param name="outer"></param>
        /// <param name="spectrumData"></param>
        /// <param name="stripCount"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="radius">圆环半径</param>
        /// <param name="spacing">圆环上面小竖条间距</param>
        /// <param name="rotation">圆环旋转角度</param>
        /// <param name="scale">控制圆环上面小竖条高度</param>
        private void DrawCircleGradientStrips(
            Path wavePath, Color inner, Color outer, double[] spectrumData, int stripCount,
            double xOffset, double yOffset, double radius, double spacing, double rotation, double scale
        )
        {
            //旋转角度转弧度
            var rotationRadian = Math.PI / 180 * rotation;

            //等分圆周，每个（竖条+空白）对应的弧度
            var blockRadian = Math.PI * 2 / stripCount;

            //每个空隙对应的弧度
            var spacingRadian = Math.PI / 180 * spacing;

            //每个竖条对应的弧度
            var stripRadian = blockRadian - spacingRadian;

            var pointArray = new Point[stripCount];

            for (var i = 0; i < stripCount; i++)
            {
                var x = blockRadian * i + rotationRadian; // angle
                var y = spectrumData[i * spectrumData.Length / stripCount] * scale; // height
                pointArray[i] = new Point(x, y);
            }

            var geometry = new PathGeometry();
            foreach (var point in pointArray)
            {
                var sinStart = Math.Sin(point.X);
                var sinEnd = Math.Sin(point.X + stripRadian);
                var cosStart = Math.Cos(point.X);
                var cosEnd = Math.Cos(point.X + stripRadian);

                var polygon = new[]
                {
                    new Point(cosStart * radius + xOffset, sinStart * radius + yOffset),
                    new Point(cosEnd * radius + xOffset, sinEnd * radius + yOffset),
                    new Point(cosEnd * (radius + point.Y) + xOffset, sinEnd * (radius + point.Y) + yOffset),
                    new Point(cosStart * (radius + point.Y) + xOffset, sinStart * (radius + point.Y) + yOffset)
                };

                var figure = new PathFigure
                {
                    StartPoint = polygon[0],
                    IsFilled = true
                };

                figure.Segments.Add(new PolyLineSegment(polygon, false));
                geometry.Figures.Add(figure);
            }

            wavePath.Data = geometry;

            var maxHeight = pointArray.Max(point => point.Y);
            var brush = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0),
                    new GradientStop(inner, radius / (radius + maxHeight)),
                    new GradientStop(outer, 1)
                },
                new Point(xOffset, 0),
                new Point(xOffset, 1)
            );

            wavePath.Fill = brush;
        }
    }
}