using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Accord.Math;

namespace CSharpDemo.Controls
{
    public class AudioSpectrumRender : Canvas
    {
        public static readonly DependencyProperty WaveformDataProperty = DependencyProperty.Register(
            nameof(WaveformData), typeof(double[]),
            typeof(AudioSpectrumRender), new PropertyMetadata(null, OnWaveformDataChanged)
        );

        public static readonly DependencyProperty SampleRateProperty = DependencyProperty.Register(
            nameof(SampleRate), typeof(int),
            typeof(AudioSpectrumRender), new PropertyMetadata(7500, OnSampleRateChanged)
        );

        private double[] _waveformData;
        private int _sampleRate = 7500;
        private readonly Image _spectrumImage;
        private WriteableBitmap _spectrumBitmap;
        private double[] _spectrumData;

        private const double LeftMargin = 10;
        private const double RightMargin = 15;
        private const double TopMargin = 5;
        private const double BottomMargin = 25;
        private const int FftSize = 4096; // 数据样本3750个点，FFT size一般是大于样本数的最小2次幂，如果小于样本数，会导致样本FFT计算出现遗漏

        private double _zoomLevel = 1.0;
        private double _panOffset;
        private const double MinZoom = 1.0;
        private const double MaxZoom = 20.0;
        private bool _isPanning;
        private double _lastPanX;
        private double _lastPinchDistance;
        private bool _isPinching;

        
        public double[] WaveformData
        {
            get => (double[])GetValue(WaveformDataProperty);
            set => SetValue(WaveformDataProperty, value);
        }

        public int SampleRate
        {
            get => (int)GetValue(SampleRateProperty);
            set => SetValue(SampleRateProperty, value);
        }

        public AudioSpectrumRender()
        {
            Background = Brushes.Transparent;

            _spectrumImage = new Image
            {
                Stretch = Stretch.Fill
            };
            Children.Add(_spectrumImage);

            SizeChanged += OnSizeChanged;
            
            // 添加鼠标事件处理程序
            MouseWheel += OnMouseWheel;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
            
            // 添加触摸事件处理程序
            ManipulationStarting += OnManipulationStarting;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
            IsManipulationEnabled = true;
        }

        private static void OnWaveformDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioSpectrumRender render)
            {
                render._waveformData = (double[])e.NewValue;
                render.ComputeSpectrum();
                render.RenderSpectrum();
            }
        }

        private static void OnSampleRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioSpectrumRender render)
            {
                render._sampleRate = (int)e.NewValue;
                render.ComputeSpectrum();
                render.RenderSpectrum();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderSpectrum();
            RenderAxes();
        }

         private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var mousePos = e.GetPosition(this);
            var plotWidth = ActualWidth - LeftMargin - RightMargin;
            
            if (plotWidth <= 0) return;

            var oldZoom = _zoomLevel;

            if (e.Delta > 0)
            {
                _zoomLevel = Math.Min(MaxZoom, _zoomLevel * 1.2);
            }
            else
            {
                _zoomLevel = Math.Max(MinZoom, _zoomLevel / 1.2);
            }

            var mouseRatio = (mousePos.X - LeftMargin) / plotWidth;
            var visibleRange = _spectrumData.Length / oldZoom;
            var oldStart = _panOffset;
            
            var newVisibleRange = _spectrumData.Length / _zoomLevel;
            _panOffset = oldStart + (mouseRatio * visibleRange) - (mouseRatio * newVisibleRange);
            _panOffset = Math.Max(0, Math.Min(_spectrumData.Length - newVisibleRange, _panOffset));

            RenderSpectrum();
            RenderXAxis();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isPanning = true;
            _lastPanX = e.GetPosition(this).X;
            CaptureMouse();
            Cursor = Cursors.Hand;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                ReleaseMouseCapture();
                Cursor = Cursors.Arrow;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPanning || _spectrumData == null || _spectrumData.Length == 0) return;

            var currentX = e.GetPosition(this).X;
            var deltaX = currentX - _lastPanX;
            _lastPanX = currentX;

            var plotWidth = ActualWidth - LeftMargin - RightMargin;
            if (plotWidth <= 0) return;

            var visibleRange = _spectrumData.Length / _zoomLevel;
            var binShift = (deltaX / plotWidth) * visibleRange;

            _panOffset -= binShift;
            _panOffset = Math.Max(0, Math.Min(_spectrumData.Length - visibleRange, _panOffset));

            RenderSpectrum();
            RenderXAxis();
        }
        
        private void OnManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.Mode = ManipulationModes.Scale | ManipulationModes.TranslateX;
            e.ManipulationContainer = this;
            e.Handled = true;
        }
        
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (_spectrumData == null || _spectrumData.Length == 0) return;

            var manipulationDelta = e.DeltaManipulation;
            
            var plotWidth = ActualWidth - LeftMargin - RightMargin;
            if (plotWidth <= 0) return;

            var centerPoint = e.ManipulationOrigin.X;
            var centerRatio = (centerPoint - LeftMargin) / plotWidth;

            var oldZoom = _zoomLevel;
            var scale = manipulationDelta.Scale.X;
            _zoomLevel = Math.Max(MinZoom, Math.Min(MaxZoom, _zoomLevel * scale));

            var oldVisibleRange = _spectrumData.Length / oldZoom;
            var newVisibleRange = _spectrumData.Length / _zoomLevel;
            _panOffset = _panOffset + (centerRatio * oldVisibleRange) - (centerRatio * newVisibleRange);
            _panOffset = Math.Max(0, Math.Min(_spectrumData.Length - newVisibleRange, _panOffset));

            var translation = manipulationDelta.Translation.X;
            var binShift = (translation / plotWidth) * newVisibleRange;
            _panOffset -= binShift;
            _panOffset = Math.Max(0, Math.Min(_spectrumData.Length - newVisibleRange, _panOffset));

            RenderSpectrum();
            RenderXAxis();

            e.Handled = true;
        }
        
        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
        }
        
        private void ComputeSpectrum()
        {
            if (_waveformData == null || _waveformData.Length == 0)
            {
                return;
            }

            var fftBuffer = new double[FftSize];
            var copyLength = Math.Min(FftSize, _waveformData.Length);
            Array.Copy(_waveformData, 0, fftBuffer, 0, copyLength);

            var complex = new Complex[FftSize];
            for (var i = 0; i < FftSize; i++)
            {
                complex[i] = new Complex(fftBuffer[i], 0);
            }

            FourierTransform.FFT(complex, FourierTransform.Direction.Forward);

            _spectrumData = new double[FftSize / 2];
            for (var i = 0; i < FftSize / 2; i++)
            {
                _spectrumData[i] = complex[i].Magnitude;
            }

            var maxMagnitude = 0.0;
            foreach (var value in _spectrumData)
            {
                if (value > maxMagnitude)
                {
                    maxMagnitude = value;
                }
            }

            if (maxMagnitude > 0)
            {
                for (var i = 0; i < _spectrumData.Length; i++)
                {
                    _spectrumData[i] /= maxMagnitude;
                }
            }
        }

        private void RenderSpectrum()
        {
            if (_spectrumData == null || _spectrumData.Length == 0)
            {
                return;
            }

            var width = ActualWidth;
            var height = ActualHeight;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            var plotWidth = (int)(width - LeftMargin - RightMargin);
            var plotHeight = (int)(height - TopMargin - BottomMargin);

            if (plotWidth <= 0 || plotHeight <= 0)
            {
                return;
            }

            _spectrumBitmap = new WriteableBitmap(
                plotWidth, plotHeight, 96, 96, PixelFormats.Pbgra32, null
            );

            var pixels = new int[plotWidth * plotHeight];
            var visibleRange = _spectrumData.Length / _zoomLevel;
            var startBin = (int)_panOffset;
            var binsPerPixel = visibleRange / plotWidth;

            for (var x = 0; x < plotWidth; x++)
            {
                var binIndex = (int)(startBin + x * binsPerPixel);
                if (binIndex >= _spectrumData.Length)
                {
                    break;
                }
                if (binIndex < 0)
                {
                    continue;
                }

                var magnitude = _spectrumData[binIndex];
                var barHeight = (int)(magnitude * plotHeight);

                for (var y = 0; y < plotHeight; y++)
                {
                    var pixelIndex = y * plotWidth + x;

                    if (y >= plotHeight - barHeight)
                    {
                        var color = GetSpectrumColor(magnitude);
                        pixels[pixelIndex] = color;
                    }
                    else
                    {
                        pixels[pixelIndex] = 0x00000000;
                    }
                }
            }

            var stride = plotWidth * 4;
            _spectrumBitmap.WritePixels(new Int32Rect(0, 0, plotWidth, plotHeight), pixels, stride, 0);
            
            _spectrumImage.Source = _spectrumBitmap;
            SetLeft(_spectrumImage, LeftMargin);
            SetTop(_spectrumImage, TopMargin);
            _spectrumImage.Width = plotWidth;
            _spectrumImage.Height = plotHeight;

            RenderXAxis();
        }

        private int GetSpectrumColor(double magnitude)
        {
            var intensity = Math.Max(0, Math.Min(1, magnitude));

            byte r, g, b;

            if (intensity < 0.33)
            {
                var t = intensity / 0.33;
                r = 0;
                g = (byte)(t * 255);
                b = 255;
            }
            else if (intensity < 0.66)
            {
                var t = (intensity - 0.33) / 0.33;
                r = (byte)(t * 255);
                g = 255;
                b = (byte)((1 - t) * 255);
            }
            else
            {
                var t = (intensity - 0.66) / 0.34;
                r = 255;
                g = (byte)((1 - t) * 255);
                b = 0;
            }

            return (255 << 24) | (r << 16) | (g << 8) | b;
        }

        private void RenderAxes()
        {
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

            RenderXAxis();
        }

        private void RenderXAxis()
        {
            if (_sampleRate <= 0)
            {
                return;
            }

            if (_spectrumData == null || _spectrumData.Length == 0)
            {
                return;
            }
            
            var width = ActualWidth;
            if (width <= 0)
            {
                return;
            }

            var plotWidth = width - LeftMargin - RightMargin;
            var visibleRange = _spectrumData.Length / _zoomLevel;
            var startBin = (int)_panOffset;
            var startFreq = startBin * (_sampleRate / 2.0 / (_spectrumData.Length - 1));
            var endBin = Math.Min(startBin + (int)visibleRange, _spectrumData.Length - 1);
            var endFreq = endBin * (_sampleRate / 2.0 / (_spectrumData.Length - 1));
            var freqRange = endFreq - startFreq;

            const int tickCount = 10;
            var frequencyStep = freqRange / tickCount;

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

            for (var i = 0; i <= tickCount; i++)
            {
                var frequency = startFreq + i * frequencyStep;
                var x = LeftMargin + (i * plotWidth / tickCount);
                AddXAxisLabel(frequency, x);
            }
        }

        private void AddXAxisLabel(double frequency, double x)
        {
            var textBlock = new TextBlock
            {
                Text = $"{frequency:F0}Hz",
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
    }
}