﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CSharpDemo.Events;
using CSharpDemo.Utils;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Prism.Events;

namespace CSharpDemo.Views
{
    public partial class AudioFileToWaveView : UserControl
    {
        public AudioFileToWaveView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            //去掉四周坐标轴
            var scottPlot = ScottplotView.Plot;
            //去掉网格线
            scottPlot.Grid(false);
            //去掉四周坐标轴
            scottPlot.Frameless();

            eventAggregator.GetEvent<WavePointEvent>().Subscribe(delegate(List<double> doubles)
            {
                ScottplotView.Plot.AddSignal(
                    doubles.ToArray(), color: System.Drawing.Color.FromArgb(255, 49, 151, 36)
                );
                ScottplotView.Refresh();
            });

            InitializeAudioVisualizer();
        }

        private void InitializeAudioVisualizer()
        {
            _capture = new WasapiLoopbackCapture(); // 捕获电脑发出的声音
            _visualizer = new AudioVisualizer(256); // 新建一个可视化器, 并使用 256 个采样进行傅里叶变换

            _dataTimer.Tick += DataTimer_Tick;
            _drawingTimer.Tick += DrawingTimer_Tick;

            _allColors = GetAllHsvColors(); // 获取所有的渐变颜色 (HSV 颜色)

            _capture.WaveFormat =
                WaveFormat.CreateIeeeFloatWaveFormat(8192, 1); // 指定捕获的格式, 单声道, 32位深度, IeeeFloat 编码, 8192采样率
            _capture.DataAvailable += Capture_DataAvailable;

            _capture.StartRecording();

            _dataTimer.Start();
            _drawingTimer.Start();
        }

        /// <summary>
        /// 用来刷新频谱数据以及实现频谱数据缓动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTimer_Tick(object sender, EventArgs e)
        {
            var newSpectrumData = _visualizer.GetSpectrumData(); // 从可视化器中获取频谱数据
            newSpectrumData = AudioVisualizer.MakeSmooth(newSpectrumData, 2); // 平滑频谱数据

            if (_spectrumData == null) // 如果已经存储的频谱数据为空, 则把新的频谱数据直接赋值上去
            {
                _spectrumData = newSpectrumData;
                return;
            }

            for (var i = 0; i < newSpectrumData.Length; i++) // 计算旧频谱数据和新频谱数据之间的 "中间值"
            {
                var oldData = _spectrumData[i];
                var newData = newSpectrumData[i];
                // 每一次执行, 频谱值会向目标值移动 20% (如果太大, 缓动效果不明显, 如果太小, 频谱会有延迟的感觉)
                var lerpData = oldData + (newData - oldData) * .2f;
                _spectrumData[i] = lerpData;
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

            var bassArea = AudioVisualizer.TakeSpectrumOfFrequency(
                _spectrumData, _capture.WaveFormat.SampleRate, 250
            );
            var bassScale = bassArea.Average() * 100;
            var extraScale = Math.Min(DrawingPanel.ActualHeight, DrawingPanel.ActualHeight) / 6;

            // - 波形波动矩形
            DrawGradientStrips(
                StripsPath, color1, color2,
                _spectrumData, _spectrumData.Length,
                StripsPath.ActualWidth, 0, StripsPath.ActualHeight,
                3, -StripsPath.ActualHeight * 10
            );

            DrawCircleGradientStrips(CirclePath, color1, color2, _spectrumData, _spectrumData.Length,
                DrawingPanel.ActualHeight / 2, DrawingPanel.ActualHeight / 2,
                Math.Min(DrawingPanel.ActualHeight, DrawingPanel.ActualHeight) / 4 + extraScale * bassScale, 1,
                _rotation, DrawingPanel.ActualHeight / 6 * 10);

            //Done - 波形曲线
            var curveBrush = new SolidColorBrush(color1);
            DrawCurve(
                SampleWavePath, curveBrush,
                _visualizer.SampleData, _visualizer.SampleData.Length,
                DrawingPanel.ActualWidth, 0, DrawingPanel.ActualHeight / 2,
                Math.Min(DrawingPanel.ActualHeight / 10, 100)
            );
        }

        /// <summary>
        /// 获取 HSV 中所有的基础颜色 (饱和度和明度均为最大值)
        /// </summary>
        /// <returns>所有的 HSV 基础颜色(共 256 * 6 个, 并且随着索引增加, 颜色也会渐变)</returns>
        private Color[] GetAllHsvColors()
        {
            var result = new Color[256 * 6];

            for (var i = 0; i <= 255; i++)
            {
                result[i] = Color.FromArgb(255, 255, (byte)i, 0);
            }

            for (var i = 0; i <= 255; i++)
            {
                result[256 + i] = Color.FromArgb(255, (byte)(255 - i), 255, 0);
            }

            for (var i = 0; i <= 255; i++)
            {
                result[512 + i] = Color.FromArgb(255, 0, 255, (byte)i);
            }

            for (var i = 0; i <= 255; i++)
            {
                result[768 + i] = Color.FromArgb(255, 0, (byte)(255 - i), 255);
            }

            for (var i = 0; i <= 255; i++)
            {
                result[1024 + i] = Color.FromArgb(255, (byte)i, 0, 255);
            }

            for (var i = 0; i <= 255; i++)
            {
                result[1280 + i] = Color.FromArgb(255, 255, 0, (byte)(255 - i));
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            var length = e.BytesRecorded / 4; // 采样的数量 (每一个采样是 4 字节)
            var result = new double[length]; // 声明结果

            for (var i = 0; i < length; i++)
            {
                result[i] = BitConverter.ToSingle(e.Buffer, i * 4); // 取出采样值
            }

            _visualizer.PushSampleData(result); // 将新的采样存储到 可视化器 中
        }

        /// <summary>
        /// 绘制渐变的条形
        /// </summary>
        /// <param name="stripsPath">绘图目标</param>
        /// <param name="down">下方颜色</param>
        /// <param name="up">上方颜色</param>
        /// <param name="spectrumData">频谱数据</param>
        /// <param name="stripCount">条形的数量</param>
        /// <param name="drawingWidth">绘图的宽度</param>
        /// <param name="xOffset">绘图的起始 X 坐标</param>
        /// <param name="yOffset">绘图的起始 Y 坐标</param>
        /// <param name="spacing">条形与条形之间的间隔(像素)</param>
        /// <param name="scale"></param>
        private void DrawGradientStrips(Path stripsPath, Color down, Color up, double[] spectrumData, int stripCount,
            double drawingWidth, double xOffset, double yOffset, double spacing, double scale)
        {
            var stripWidth = (drawingWidth - spacing * stripCount) / stripCount;
            var points = new Point[stripCount];

            for (var i = 0; i < stripCount; i++)
            {
                var x = stripWidth * i + spacing * i + xOffset;
                var y = spectrumData[i * spectrumData.Length / stripCount] * scale; // height
                points[i] = new Point(x, y);
            }

            var upP = points.Min(v => v.Y < 0 ? yOffset + v.Y : yOffset);
            var downP = points.Max(v => v.Y < 0 ? yOffset : yOffset + v.Y);

            if (downP < yOffset)
            {
                downP = yOffset;
            }

            var geometry = new GeometryGroup();
            var brush = new LinearGradientBrush(down, up, new Point(0, downP), new Point(0, upP));

            for (var i = 0; i < stripCount; i++)
            {
                var p = points[i];
                var height = p.Y;

                if (height < 0)
                {
                    height = -height;
                }

                var endPoints = new[]
                {
                    new Point(p.X, p.Y),
                    new Point(p.X, p.Y + height),
                    new Point(p.X + stripWidth, p.Y + height),
                    new Point(p.X + stripWidth, p.Y)
                };

                var figure = new PathFigure
                {
                    StartPoint = endPoints[0]
                };

                figure.Segments.Add(new PolyLineSegment(endPoints, false));
                geometry.Children.Add(new PathGeometry { Figures = { figure } });
            }

            stripsPath.Data = geometry;
            stripsPath.Fill = brush;
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
        /// <param name="radius"></param>
        /// <param name="spacing"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        private void DrawCircleGradientStrips(
            Path wavePath, Color inner, Color outer, double[] spectrumData, int stripCount,
            double xOffset, double yOffset, double radius, double spacing, double rotation, double scale
        )
        {
            var rotationAngle = Math.PI / 180 * rotation;
            var blockWidth = Math.PI * 2 / stripCount; // angle
            var stripWidth = blockWidth - Math.PI / 180 * spacing; // angle
            var points = new Point[stripCount];

            for (var i = 0; i < stripCount; i++)
            {
                var x = blockWidth * i + rotationAngle; // angle
                var y = spectrumData[i * spectrumData.Length / stripCount] * scale; // height
                points[i] = new Point(x, y);
            }

            var maxHeight = points.Max(v => v.Y);
            var outerRadius = radius + maxHeight;

            var geo = new PathGeometry();

            for (var i = 0; i < stripCount; i++)
            {
                var p = points[i];
                var sinStart = Math.Sin(p.X);
                var sinEnd = Math.Sin(p.X + stripWidth);
                var cosStart = Math.Cos(p.X);
                var cosEnd = Math.Cos(p.X + stripWidth);

                var polygon = new[]
                {
                    new Point((cosStart * radius + xOffset), (sinStart * radius + yOffset)),
                    new Point((cosEnd * radius + xOffset), (sinEnd * radius + yOffset)),
                    new Point((cosEnd * (radius + p.Y) + xOffset), (sinEnd * (radius + p.Y) + yOffset)),
                    new Point((cosStart * (radius + p.Y) + xOffset), (sinStart * (radius + p.Y) + yOffset))
                };

                var fig = new PathFigure
                {
                    IsFilled = true
                };

                fig.Segments.Add(new PolyLineSegment(polygon, false));
                geo.Figures.Add(fig);
            }

            var brush = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0),
                    new GradientStop(inner, radius / (radius + maxHeight)),
                    new GradientStop(outer, 1)
                },
                new Point(xOffset, yOffset),
                new Point(xOffset, yOffset + radius + maxHeight));
            wavePath.Data = geo;
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
        /// <param name="scale"></param>
        private void DrawCurve(Path wavePath, Brush brush, double[] spectrumData, int pointCount, double drawingWidth,
            double xOffset, double yOffset, double scale)
        {
            var points = new Point[pointCount];
            for (var i = 0; i < pointCount; i++)
            {
                var x = i * drawingWidth / pointCount + xOffset;
                var y = spectrumData[i * spectrumData.Length / pointCount] * scale + yOffset;
                points[i] = new Point(x, y);
            }

            var figure = new PathFigure();
            figure.Segments.Add(new PolyLineSegment(points, true));

            wavePath.Data = new PathGeometry { Figures = { figure } };
            wavePath.Stroke = brush;
        }

        private AudioVisualizer _visualizer; // 可视化
        private WasapiCapture _capture; // 音频捕获
        private double[] _spectrumData; // 频谱数据
        private int _colorIndex;
        private double _rotation;
        private Color[] _allColors;

        private readonly DispatcherTimer _dataTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 30)
        };

        private readonly DispatcherTimer _drawingTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 30)
        };
    }
}