using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CSharpDemo.Controls
{
    public class AudioWaveformRender : Canvas
    {
        public static readonly DependencyProperty WaveformDataProperty = DependencyProperty.Register(
            nameof(WaveformData), typeof(double[]),
            typeof(AudioWaveformRender), new PropertyMetadata(null, OnWaveformDataChanged)
        );

        public static readonly DependencyProperty TotalDurationProperty = DependencyProperty.Register(
            nameof(TotalDuration), typeof(TimeSpan),
            typeof(AudioWaveformRender), new PropertyMetadata(TimeSpan.Zero, OnTotalDurationChanged)
        );

        public static readonly DependencyProperty CurrentPositionProperty = DependencyProperty.Register(
            nameof(CurrentPosition), typeof(TimeSpan),
            typeof(AudioWaveformRender), new PropertyMetadata(TimeSpan.Zero, OnCurrentPositionChanged)
        );

        public static readonly DependencyProperty WaveMarginProperty = DependencyProperty.Register(
            nameof(WaveMargin), typeof(Thickness),
            typeof(AudioWaveformRender), new PropertyMetadata(new Thickness(10, 0, 15, 15))
        );

        public static readonly DependencyProperty PositionChangedCommandProperty = DependencyProperty.Register(
            nameof(PositionChangedCommand), typeof(ICommand),
            typeof(AudioWaveformRender), new PropertyMetadata(null)
        );

        private double[] _waveformData;
        private double _maxAmplitude = 1.0;
        private readonly Polyline _waveformLine;
        private readonly Rectangle _positionIndicator;
        private readonly Line _zeroLine;
        private bool _isDragging;

        public double[] WaveformData
        {
            get => (double[])GetValue(WaveformDataProperty);
            set => SetValue(WaveformDataProperty, value);
        }

        public TimeSpan TotalDuration
        {
            get => (TimeSpan)GetValue(TotalDurationProperty);
            set => SetValue(TotalDurationProperty, value);
        }

        public TimeSpan CurrentPosition
        {
            get => (TimeSpan)GetValue(CurrentPositionProperty);
            set => SetValue(CurrentPositionProperty, value);
        }

        public Thickness WaveMargin
        {
            get => (Thickness)GetValue(WaveMarginProperty);
            set => SetValue(WaveMarginProperty, value);
        }

        public ICommand PositionChangedCommand
        {
            get => (ICommand)GetValue(PositionChangedCommandProperty);
            set => SetValue(PositionChangedCommandProperty, value);
        }

        public AudioWaveformRender()
        {
            Background = Brushes.Transparent;

            // 零线（水平中心线）
            _zeroLine = new Line
            {
                Stroke = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 5, 5 }
            };
            Children.Add(_zeroLine);

            // 波形线
            _waveformLine = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };
            Children.Add(_waveformLine);

            // 播放位置指示器（竖线）
            _positionIndicator = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                Width = 1,
                IsHitTestVisible = false
            };
            Children.Add(_positionIndicator);

            // 鼠标事件
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderWaveform();
            UpdatePositionIndicator();
            RenderXAxis();
        }

        private static void OnWaveformDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioWaveformRender render)
            {
                render._waveformData = (double[])e.NewValue;
                render.CalculateMaxAmplitude();
                render.RenderWaveform();
            }
        }

        private static void OnTotalDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioWaveformRender render)
            {
                render.RenderXAxis();
            }
        }

        private static void OnCurrentPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioWaveformRender render)
            {
                render.UpdatePositionIndicator();
            }
        }

        private void CalculateMaxAmplitude()
        {
            if (_waveformData == null || _waveformData.Length == 0)
            {
                return;
            }

            _maxAmplitude = 0;
            foreach (var sample in _waveformData)
            {
                var abs = Math.Abs(sample);
                if (abs > _maxAmplitude)
                {
                    _maxAmplitude = abs;
                }
            }

            if (_maxAmplitude < 0.000001)
            {
                // 接近静音的音频，给予一个极小的基准值避免除零
                _maxAmplitude = 0.000001;
            }
        }

        public double GetMaxAmplitude()
        {
            return _maxAmplitude;
        }

        /// <summary>
        /// 将振幅转换为分贝值（dB）
        /// </summary>
        public double GetAmplitudeInDecibels()
        {
            if (_maxAmplitude <= 0)
                return -double.MaxValue;

            // dB = 20 * log10(amplitude)
            return 20 * Math.Log10(_maxAmplitude);
        }

        private void RenderWaveform()
        {
            if (_waveformData == null || _waveformData.Length == 0)
            {
                return;
            }

            var width = ActualWidth;
            var height = ActualHeight;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            // 计算有效绘图区域（减去边距）
            var effectiveWidth = width - WaveMargin.Left - WaveMargin.Right;
            var effectiveHeight = height - WaveMargin.Top - WaveMargin.Bottom;

            // 绘制波形
            var points = new PointCollection();
            var samplesPerPixel = _waveformData.Length / effectiveWidth;

            for (var x = 0; x < width; x++)
            {
                var sampleIndex = (int)(x * samplesPerPixel);
                if (sampleIndex >= _waveformData.Length)
                {
                    break;
                }

                var sample = _waveformData[sampleIndex];
                // sample / _maxAmplitude：归一化采样值（未归一化的波形只在中心附近一小块区域）
                // (sample / _maxAmplitude) * (height / 2)：缩放到屏幕高度
                // height / 2 - ...：转换为屏幕坐标
                // * 0.9：留出顶部和底部边距
                var centerY = WaveMargin.Top + effectiveHeight / 2;
                var y = centerY - (sample / _maxAmplitude) * (effectiveHeight / 2) * 0.9;
                // var y = centerY - sample * (effectiveHeight / 2) * 0.9; // 直接使用采样值（未归一化）

                // 添加左边距偏移
                points.Add(new Point(x + WaveMargin.Left, y));
            }

            _waveformLine.Points = points;

            // 绘制零线（也添加边距）
            _zeroLine.X1 = WaveMargin.Left;
            _zeroLine.Y1 = WaveMargin.Top + effectiveHeight / 2;
            _zeroLine.X2 = width - WaveMargin.Right;
            _zeroLine.Y2 = WaveMargin.Top + effectiveHeight / 2;
        }

        private void RenderXAxis()
        {
            if (TotalDuration.TotalSeconds <= 0)
            {
                return;
            }

            // 清除旧的X轴标签
            var toRemove = new List<UIElement>();
            foreach (var child in Children)
            {
                if (child is TextBlock tb && tb.Tag?.ToString() == "XAxis")
                {
                    toRemove.Add(tb);
                }
            }

            foreach (var item in toRemove)
            {
                Children.Remove(item);
            }

            var width = ActualWidth;
            if (width <= 0)
            {
                return;
            }

            var totalSeconds = TotalDuration.TotalSeconds;
            var effectiveWidth = width - WaveMargin.Left - WaveMargin.Right;

            if (totalSeconds <= 10)
            {
                // 时长 ≤ 10秒：每秒1个刻度
                var tickCount = (int)totalSeconds + 1;
                for (var i = 0; i < tickCount; i++)
                {
                    var timeValue = (double)i;
                    var x = WaveMargin.Left + (timeValue / totalSeconds) * effectiveWidth;
                    AddXAxisLabel(timeValue, x);
                }
            }
            else
            {
                // 时长 > 10秒：固定10个刻度，均匀分布
                const int tickCount = 10;
                var timeStep = totalSeconds / tickCount;

                for (var i = 0; i <= tickCount; i++)
                {
                    var timeValue = i * timeStep;
                    var x = WaveMargin.Left + (i * effectiveWidth / tickCount);
                    AddXAxisLabel(timeValue, x);
                }
            }
        }

        private void AddXAxisLabel(double seconds, double x)
        {
            var textBlock = new TextBlock
            {
                Text = $"{seconds}s",
                Foreground = Brushes.White,
                FontSize = 10,
                Tag = "XAxis"
            };

            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var textWidth = textBlock.DesiredSize.Width;

            Children.Add(textBlock);
            SetLeft(textBlock, x - textWidth / 2);
            SetBottom(textBlock, 2);
        }

        private void UpdatePositionIndicator()
        {
            if (TotalDuration.TotalSeconds <= 0)
            {
                return;
            }

            var width = ActualWidth;
            var effectiveWidth = width - WaveMargin.Left - WaveMargin.Right;
            var ratio = CurrentPosition.TotalSeconds / TotalDuration.TotalSeconds;
            var x = WaveMargin.Left + ratio * effectiveWidth;

            SetLeft(_positionIndicator, x - 1);
            SetTop(_positionIndicator, WaveMargin.Top);
            _positionIndicator.Height = ActualHeight - WaveMargin.Top - WaveMargin.Bottom;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            UpdatePositionFromMouse(e);
            CaptureMouse();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                UpdatePositionFromMouse(e);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                UpdatePositionFromMouse(e);
            }
        }

        private void UpdatePositionFromMouse(MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            var width = ActualWidth;
            var effectiveWidth = width - WaveMargin.Left - WaveMargin.Right;

            // 计算在有效区域内的相对位置
            var relativeX = pos.X - WaveMargin.Left;
            var ratio = relativeX / effectiveWidth;
            ratio = Math.Max(0, Math.Min(1, ratio));

            var newPosition = TimeSpan.FromSeconds(ratio * TotalDuration.TotalSeconds);
            CurrentPosition = newPosition;
            PositionChangedCommand?.Execute(newPosition);
        }

        public void AddGridLines(int hCount, int vCount)
        {
            var width = ActualWidth;
            var height = ActualHeight;
            var effectiveWidth = width - WaveMargin.Left - WaveMargin.Right;
            var effectiveHeight = height - WaveMargin.Top - WaveMargin.Bottom;

            for (var i = 1; i < vCount; i++)
            {
                var x = WaveMargin.Left + i * effectiveWidth / vCount;
                var line = new Line
                {
                    X1 = x,
                    Y1 = WaveMargin.Top,
                    X2 = x,
                    Y2 = height - WaveMargin.Bottom,
                    Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                    StrokeThickness = 1
                };
                Children.Add(line);
            }

            for (var i = 1; i < hCount; i++)
            {
                var y = WaveMargin.Top + i * effectiveHeight / hCount;
                var line = new Line
                {
                    X1 = WaveMargin.Left,
                    Y1 = y,
                    X2 = width - WaveMargin.Right,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                    StrokeThickness = 1
                };
                Children.Add(line);
            }
        }
    }
}