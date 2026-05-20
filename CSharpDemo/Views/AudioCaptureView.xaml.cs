using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CSharpDemo.Model;
using CSharpDemo.Service;
using CSharpDemo.Utils;
using NAudio.Wave;

namespace CSharpDemo.Views
{
    public partial class AudioCaptureView : UserControl
    {
        private const int SampleRate = 7500;
        private readonly Color[] _allColors;
        private readonly AudioVisualizer _visualizer; // 可视化
        private Color _color1;
        private Color _color2;
        private double _rotation; // 旋转角度
        private double _bassScale;

        public AudioCaptureView(IAppDataService dataService)
        {
            InitializeComponent();

            _allColors = dataService.GetHsvColors(); // 获取所有的渐变颜色 (HSV 颜色)
            _visualizer = new AudioVisualizer(SampleRate, false);
            _visualizer.TimeDomainEvent += Handle_TimeDomainEvent;
            _visualizer.FrequencyDomainEvent += Handle_FrequencyDomainEvent;
            _visualizer.RenderCountEvent += Handle_RenderCountEvent;
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