using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Prism.Commands;
using Prism.Mvvm;

namespace CSharpDemo.ViewModels
{
    public class AudioAnalyzerViewModel : BindableBase, IDisposable
    {
        #region VM

        private string _audioFilePath;

        public string AudioFilePath
        {
            get => _audioFilePath;
            set => SetProperty(ref _audioFilePath, value);
        }

        private double[] _waveformData;

        public double[] WaveformData
        {
            get => _waveformData;
            set => SetProperty(ref _waveformData, value);
        }

        private TimeSpan _totalDuration;

        public TimeSpan TotalDuration
        {
            get => _totalDuration;
            set => SetProperty(ref _totalDuration, value);
        }

        private TimeSpan _currentPosition;

        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            set => SetProperty(ref _currentPosition, value);
        }

        #endregion

        #region DelegateCommand

        public DelegateCommand SelectAudioCommand { get; set; }
        public DelegateCommand<TimeSpan?> PositionChangedCommand { get; set; }

        #endregion

        private const int SampleRate = 7500;
        private WaveOutEvent _wavePlayer;
        private WaveStream _waveStream;
        private readonly DispatcherTimer _progressTimer;

        public AudioAnalyzerViewModel()
        {
            SelectAudioCommand = new DelegateCommand(OnSelectAudio);
            PositionChangedCommand = new DelegateCommand<TimeSpan?>(OnPositionChanged);

            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _progressTimer.Tick += OnProgressTimerTick;
        }

        private void OnSelectAudio()
        {
            var fileDialog = new OpenFileDialog
            {
                DefaultExt = ".wav",
                Filter = "WAV 文件 (*.wav)|*.wav"
            };

            var result = fileDialog.ShowDialog();
            if (result != true) return;

            AudioFilePath = fileDialog.FileName;
            LoadAndPlayAudio(AudioFilePath);
        }

        private void LoadAndPlayAudio(string path)
        {
            StopAndCleanup();

            _waveStream = new WaveFileReader(path);
            TotalDuration = _waveStream.TotalTime;

            LoadFullWaveformData();

            StartPlayback();
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

            WaveformData = allSamples.ToArray();
        }

        private void StartPlayback()
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
                CurrentPosition = _waveStream.CurrentTime;
            }
        }

        private void OnPositionChanged(TimeSpan? position)
        {
            if (!position.HasValue || _wavePlayer == null || _waveStream == null)
            {
                return;
            }

            var ratio = position.Value.TotalSeconds / TotalDuration.TotalSeconds;
            _waveStream.Position = (long)(ratio * _waveStream.Length);
            _wavePlayer.Play();
        }

        private void StopAndCleanup()
        {
            _progressTimer?.Stop();

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

        public void Dispose()
        {
            StopAndCleanup();
            _progressTimer.Tick -= OnProgressTimerTick;
        }
    }
}