using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CSharpDemo.Service;
using CSharpDemo.Utils;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace CSharpDemo.Views
{
    public partial class AudioWaveView : UserControl
    {
        private readonly AudioVisualizer _visualizer; // 可视化
        private readonly WasapiCapture _capture; // 音频捕获
        private double[] _spectrumData; // 频谱数据
        private int _colorIndex;
        private double _rotation;
        private readonly Color[] _allColors;

        private readonly DispatcherTimer _dataTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 30)
        };

        private readonly DispatcherTimer _drawingTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 30)
        };

        public AudioWaveView(IMainDataService dataService)
        {
            InitializeComponent();

            _capture = new WasapiLoopbackCapture(); // 捕获电脑发出的声音
            _visualizer = new AudioVisualizer(128);

            _allColors = dataService.GetAllHsvColors(); // 获取所有的渐变颜色 (HSV 颜色)

            _capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(7500, 1);
            _capture.DataAvailable += delegate(object o, WaveInEventArgs args)
            {
                var length = args.BytesRecorded / 4; // 采样的数量 (每一个采样是 4 字节)
                var result = new double[length]; // 声明结果

                for (var i = 0; i < length; i++)
                {
                    result[i] = BitConverter.ToSingle(args.Buffer, i * 4); // 取出采样值
                }

                _visualizer.PushSampleData(result); // 将新的采样存储到 可视化器 中
            };

            _dataTimer.Tick += DataTimer_Tick;
            _drawingTimer.Tick += DrawingTimer_Tick;
        }

        private void AudioWaveView_OnLoaded(object sender, RoutedEventArgs e)
        {
            _capture.StartRecording();
            _dataTimer.Start();
            _drawingTimer.Start();
        }

        private void AudioWaveView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _drawingTimer.Stop();
            _dataTimer.Stop();
            _capture.StopRecording();
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

            //长条形波动图
            DrawGradientStrips(
                StripsPath, color1, color2,
                _spectrumData, _spectrumData.Length,
                StripsPath.ActualWidth, 0, StripsPath.ActualHeight,
                2, -StripsPath.ActualHeight * 25
            );

            //圆形波动图
            var bassArea = AudioVisualizer.TakeSpectrumOfFrequency(
                _spectrumData, _capture.WaveFormat.SampleRate, 250
            );
            var bassScale = bassArea.Average() * 100; //低音区
            var extraScale = Math.Min(CirclePath.ActualWidth, CirclePath.ActualHeight) / 6; //高音区
            DrawCircleGradientStrips(
                CirclePath, color1, color2,
                _spectrumData, _spectrumData.Length,
                CirclePath.ActualWidth / 2, CirclePath.ActualHeight / 2,
                Math.Min(CirclePath.ActualWidth, CirclePath.ActualHeight) / 3 + extraScale * bassScale,
                1, _rotation, CirclePath.ActualHeight * 3);

            //波形曲线
            var curveBrush = new SolidColorBrush(color1);
            DrawCurve(
                SampleWavePath, curveBrush,
                _visualizer.SampleData, _visualizer.SampleData.Length,
                SampleWavePanel.ActualWidth, 0, SampleWavePanel.ActualHeight / 2,
                Math.Min(SampleWavePanel.ActualHeight / 2, 50)
            );

            //四周边框
            DrawGradientBorder(TopBorder, BottomBorder, LeftBorder, RightBorder,
                Color.FromArgb(0, color1.R, color1.G, color1.B), color2,
                SampleWavePanel.ActualWidth / 3, bassScale);
        }

        /// <summary>
        /// 绘制渐变的条形
        /// </summary>
        /// <param name="stripsPath">绘图目标</param>
        /// <param name="bottomColor">下方颜色</param>
        /// <param name="topColor">上方颜色</param>
        /// <param name="spectrumData">频谱数据</param>
        /// <param name="stripCount">条形的数量</param>
        /// <param name="drawingWidth">绘图的宽度</param>
        /// <param name="xOffset">绘图的起始 X 坐标</param>
        /// <param name="yOffset">绘图的起始 Y 坐标</param>
        /// <param name="spacing">条形与条形之间的间隔(像素)</param>
        /// <param name="scale">控制波形图波峰高度</param>
        private void DrawGradientStrips(Path stripsPath, Color bottomColor, Color topColor, double[] spectrumData,
            int stripCount, double drawingWidth, double xOffset, double yOffset, double spacing, double scale)
        {
            //竖条宽度
            var stripWidth = (drawingWidth - spacing * stripCount) / stripCount;
            var pointArray = new Point[stripCount];

            for (var i = 0; i < stripCount; i++)
            {
                var x = stripWidth * i + spacing * i + xOffset;
                var y = spectrumData[i * spectrumData.Length / stripCount] * scale; // height
                //给所有频谱位置赋值
                pointArray[i] = new Point(x, y);
            }

            //生成一系列频谱竖条
            var geometry = new GeometryGroup();
            for (var i = 0; i < stripCount; i++)
            {
                var p = pointArray[i];
                var height = p.Y;

                if (height < 0)
                {
                    height = -height;
                }

                //每根竖条的四个角坐标
                var endPoints = new[]
                {
                    new Point(p.X, p.Y + yOffset), //左下角
                    new Point(p.X, p.Y + height + yOffset), //左上角
                    new Point(p.X + stripWidth, p.Y + height + yOffset), //右上角
                    new Point(p.X + stripWidth, p.Y + yOffset) //右下角
                };

                var figure = new PathFigure
                {
                    StartPoint = endPoints[0]
                };

                figure.Segments.Add(new PolyLineSegment(endPoints, false));
                geometry.Children.Add(new PathGeometry { Figures = { figure } });
            }

            stripsPath.Data = geometry;

            //设置频谱竖条的渐变色
            var linearGradientBrush = new LinearGradientBrush(
                bottomColor, topColor, new Point(0, 0), new Point(0, 1)
            );
            stripsPath.Fill = linearGradientBrush;
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
        private void DrawCurve(Path wavePath, Brush brush, double[] spectrumData, int pointCount, double drawingWidth,
            double xOffset, double yOffset, double scale)
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
        private void DrawGradientBorder(Rectangle topBorder, Rectangle bottomBorder, Rectangle leftBorder,
            Rectangle rightBorder, Color inner, Color outer, double width, double bassScale)
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