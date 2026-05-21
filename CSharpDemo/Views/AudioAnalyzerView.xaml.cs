using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CSharpDemo.Views
{
    public partial class AudioAnalyzerView : UserControl
    {
        private const int SampleRate = 7500;
        private WaveOutEvent _wavePlayer;
        private TimeSpan _duration;
        private WaveStream _waveStream;
        private double[] _waveformData;
        private readonly DispatcherTimer _progressTimer;

        public AudioAnalyzerView()
        {
            InitializeComponent();

            SelectAudioButton.Click += (sender, e) =>
            {
                var fileDialog = new OpenFileDialog
                {
                    // 设置默认格式
                    DefaultExt = ".wav",
                    Filter = "WAV 文件 (*.wav)|*.wav"
                };
                var result = fileDialog.ShowDialog();
                if (result != true) return;

                var audioFilePath = fileDialog.FileName;
                AudioFilePathTextBox.Text = audioFilePath;

                ReadAndPlayAudioFile(audioFilePath);
            };

            // 创建进度更新定时器
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _progressTimer.Tick += OnProgressTimerTick;
        }

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        private void ReadAndPlayAudioFile(string path)
        {
            // 清理旧的资源
            StopAndCleanup();

            _waveStream = new WaveFileReader(path);
            _duration = _waveStream.TotalTime;

            // 加载完整的音频数据用于波形显示
            LoadFullWaveformData();

            // 设置波形控件
            WaveformRender.WaveformData = _waveformData;
            WaveformRender.TotalDuration = _duration;

            // 设置频谱控件
            SpectrumRender.WaveformData = _waveformData;
            SpectrumRender.SampleRate = SampleRate;

            // 准备播放
            PrepareForPlayback();

            // 开始跟踪播放进度
            WaveformRender.StartPlaybackTracking();
            _progressTimer.Start();
        }

        private void LoadFullWaveformData()
        {
            var sampleProvider = _waveStream.ToSampleProvider();

            if (_waveStream.WaveFormat.Channels > 1)
            {
                sampleProvider = sampleProvider.ToMono();
            }

            var resampledProvider = new WdlResamplingSampleProvider(sampleProvider, SampleRate);

            var buffer = new float[16384];
            var allSamples = new List<double>();

            int samplesRead;
            while ((samplesRead = resampledProvider.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (var i = 0; i < samplesRead; i++)
                {
                    allSamples.Add(buffer[i]);
                }
            }

            _waveformData = allSamples.ToArray();
        }

        private void PrepareForPlayback()
        {
            _waveStream.Position = 0;
            var sampleProvider = _waveStream.ToSampleProvider();

            if (_waveStream.WaveFormat.Channels > 1)
            {
                sampleProvider = sampleProvider.ToMono();
            }

            var resampledProvider = new WdlResamplingSampleProvider(sampleProvider, SampleRate);

            _wavePlayer = new WaveOutEvent();
            _wavePlayer.Init(resampledProvider.ToWaveProvider16());
            _wavePlayer.Play();
        }

        private void OnProgressTimerTick(object sender, EventArgs e)
        {
            if (_wavePlayer != null && _waveStream != null)
            {
                var currentPosition = _waveStream.CurrentTime;
                WaveformRender.UpdatePlaybackPosition(currentPosition);
            }
        }

        private void WaveformRender_OnPositionChanged(object sender, TimeSpan e)
        {
            // 跳转到指定位置播放
            if (_wavePlayer != null && _waveStream != null)
            {
                var ratio = e.TotalSeconds / _duration.TotalSeconds;
                _waveStream.Position = (long)(ratio * _waveStream.Length);
                _wavePlayer.Play();
            }
        }

        private void AudioAnalyzerView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopAndCleanup();
        }

        /// <summary>
        /// 停止播放并清理资源
        /// </summary>
        private void StopAndCleanup()
        {
            _progressTimer?.Stop();
            WaveformRender?.StopPlaybackTracking();

            if (_wavePlayer != null)
            {
                _wavePlayer.Stop();
                _wavePlayer.Dispose();
                _wavePlayer = null;
            }

            if (_waveStream != null)
            {
                _waveStream.Dispose();
                _waveStream = null;
            }
        }
    }
}