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
        private FrequencyDomainData _spectrumData; // 频谱数据
        private int _colorIndex;
        private double _rotation;

        public AudioVisualizerView(IAppDataService dataService)
        {
            InitializeComponent();

            _allColors = dataService.GetHsvColors(); // 获取所有的渐变颜色 (HSV 颜色)
            _visualizer = new AudioVisualizer(SampleRate, 512);

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
            var freqData = _visualizer.GetFrequencyDomain();

            // 平滑频谱数据
            var newSpectrumData = AudioVisualizer.MakeSmooth(freqData, 2); // 平滑频谱数据

            // 转换为分贝显示（更符合人耳感知）
            newSpectrumData.Magnitudes = newSpectrumData.Magnitudes.ToDecibels();

            if (_spectrumData == null)
            {
                // 如果已经存储的频谱数据为空, 则把新的频谱数据直接赋值上去
                _spectrumData = newSpectrumData;
                return;
            }

            for (var i = 0; i < newSpectrumData.Magnitudes.Length; i++)
            {
                var oldData = _spectrumData.Magnitudes[i];
                var newData = newSpectrumData.Magnitudes[i];

                // 计算旧频谱数据和新频谱数据之间的 "中间值"，每次向目标值移动 20%
                var deltaData = oldData + (newData - oldData) * 0.2;
                _spectrumData.Magnitudes[i] = deltaData;
            }
        }

        private void DrawingTimer_Tick(object sender, EventArgs e)
        {
            if (_spectrumData == null)
            {
                return;
            }

            _rotation += .1;
            _colorIndex++;

            var color1 = _allColors[_colorIndex % _allColors.Length];
            var color2 = _allColors[(_colorIndex + 200) % _allColors.Length];

            //圆形波动图
            // var bassArea = AudioVisualizer.TakeSpectrumOfFrequency(
            //     _spectrumData, _audioCapture.WaveFormat.SampleRate, 250
            // );
            // var bassScale = bassArea.Average() * 100; //低音区
            // var extraScale = Math.Min(CirclePath.ActualWidth, CirclePath.ActualHeight) / 6; //高音区
            // DrawCircleGradientStrips(
            //     CirclePath, color1, color2,
            //     _spectrumData, _spectrumData.Length,
            //     CirclePath.ActualWidth / 2, CirclePath.ActualHeight / 2,
            //     Math.Min(CirclePath.ActualWidth, CirclePath.ActualHeight) / 3 + extraScale * bassScale,
            //     1, _rotation, CirclePath.ActualHeight * 3
            // );

            //波形曲线
            // var curveBrush = new SolidColorBrush(color1);
            // DrawCurve(
            //     SampleWavePath, curveBrush,
            //     _visualizer.FrameBuffer, _visualizer.FrameBuffer.Length,
            //     SampleWavePanel.ActualWidth, 0, SampleWavePanel.ActualHeight / 2,
            //     Math.Min(SampleWavePanel.ActualHeight / 2, 50)
            // );

            //四周边框
            // DrawGradientBorder(
            //     TopBorder, BottomBorder, LeftBorder, RightBorder,
            //     Color.FromArgb(0, color1.R, color1.G, color1.B), color2,
            //     SampleWavePanel.ActualWidth / 3, bassScale
            // );

            //长条形波动图
            _spectrumData.DrawGradientStrips(
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

        /// <summary>
        /// 画曲线
        /// </summary>
        /// <param name="wavePath"></param>
        /// <param name="brush"></param>
        /// <param name="spectrumData"></param>
        /// <param name="pointCount"></param>
        /// <param name="drawingWidth"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="scale">控制波形图波峰高度和波谷深度</param>
        private void DrawCurve(
            Path wavePath, Brush brush, double[] spectrumData, int pointCount, double drawingWidth,
            double xOffset, double yOffset, double scale
        )
        {
            var pointArray = new Point[pointCount];
            for (var i = 0; i < pointCount; i++)
            {
                var x = i * drawingWidth / pointCount + xOffset;
                var y = spectrumData[i * spectrumData.Length / pointCount] * scale + yOffset;
                pointArray[i] = new Point(x, y);
            }

            var figure = new PathFigure
            {
                StartPoint = pointArray[0]
            };
            figure.Segments.Add(new PolyLineSegment(pointArray, true));

            wavePath.Data = new PathGeometry { Figures = { figure } };
            wavePath.StrokeThickness = 2;
            wavePath.Stroke = brush;
        }

        /// <summary>
        /// 画四周渐变边框
        /// </summary>
        /// <param name="topBorder"></param>
        /// <param name="bottomBorder"></param>
        /// <param name="leftBorder"></param>
        /// <param name="rightBorder"></param>
        /// <param name="inner"></param>
        /// <param name="outer"></param>
        /// <param name="width">画图宽度</param>
        /// <param name="bassScale">高低音转化比例</param>
        private void DrawGradientBorder(
            Rectangle topBorder, Rectangle bottomBorder, Rectangle leftBorder,
            Rectangle rightBorder, Color inner, Color outer, double width, double bassScale
        )
        {
            //边框粗细根据音频高低音变化
            var thickness = (int)(width * bassScale);

            topBorder.Height = thickness;
            bottomBorder.Height = thickness;
            leftBorder.Width = thickness;
            rightBorder.Width = thickness;

            topBorder.Fill = new LinearGradientBrush(outer, inner, 90);
            bottomBorder.Fill = new LinearGradientBrush(inner, outer, 90);
            leftBorder.Fill = new LinearGradientBrush(outer, inner, 0);
            rightBorder.Fill = new LinearGradientBrush(inner, outer, 0);
        }
    }
}