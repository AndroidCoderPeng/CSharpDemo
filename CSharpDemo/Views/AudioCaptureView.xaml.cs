using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CSharpDemo.Model;
using CSharpDemo.Service;
using CSharpDemo.Utils;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace CSharpDemo.Views
{
    public partial class AudioCaptureView : UserControl
    {
        private const int SampleRate = 44100;
        private readonly Color[] _allColors;
        private readonly AudioVisualizer _visualizer; // 可视化
        private Color _color1;
        private Color _color2;
        private double _rotation; // 旋转角度
        private double _bassScale;
        private WasapiLoopbackCapture _capture;
        private bool _isCapturing;

        public AudioCaptureView(IAppDataService dataService)
        {
            InitializeComponent();

            _allColors = dataService.GetHsvColors(); // 获取所有的渐变颜色 (HSV 颜色)
            _visualizer = new AudioVisualizer(SampleRate, false);

            InitializeAudioCapture();

            CaptureButton.Click += (sender, e) =>
            {
                if (_isCapturing)
                {
                    StopCapture();
                    _isCapturing = false;
                    CaptureButton.Content = "开始捕获";
                }
                else
                {
                    StartCapture();
                    _isCapturing = true;
                    CaptureButton.Content = "停止捕获";
                }
            };
        }

        private void InitializeAudioCapture()
        {
            try
            {
                _capture = new WasapiLoopbackCapture();

                _capture.DataAvailable += OnDataAvailable;
                _capture.RecordingStopped += OnRecordingStopped;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化音频捕获失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var sampleCount = e.BytesRecorded / 4;
            var samples = new float[sampleCount];
            Buffer.BlockCopy(e.Buffer, 0, samples, 0, e.BytesRecorded);

            _visualizer.PushAudioData(samples);
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                Console.WriteLine($@"音频捕获停止: {e.Exception.Message}");
            }
        }

        private void StartCapture()
        {
            if (_capture != null && _capture.CaptureState != CaptureState.Capturing)
            {
                try
                {
                    if (_visualizer != null)
                    {
                        _visualizer.TimeDomainEvent += Handle_TimeDomainEvent;
                        _visualizer.FrequencyDomainEvent += Handle_FrequencyDomainEvent;
                        _visualizer.RenderCountEvent += Handle_RenderCountEvent;
                    }

                    _capture.StartRecording();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"开始捕获失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StopCapture()
        {
            if (_capture != null && _capture.CaptureState == CaptureState.Capturing)
            {
                try
                {
                    _capture.StopRecording();
                    if (_visualizer != null)
                    {
                        _visualizer.TimeDomainEvent -= Handle_TimeDomainEvent;
                        _visualizer.FrequencyDomainEvent -= Handle_FrequencyDomainEvent;
                        _visualizer.RenderCountEvent -= Handle_RenderCountEvent;
                    }

                    if (_capture != null)
                    {
                        _capture.DataAvailable -= OnDataAvailable;
                        _capture.RecordingStopped -= OnRecordingStopped;
                        _capture.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"停止捕获失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
    }
}