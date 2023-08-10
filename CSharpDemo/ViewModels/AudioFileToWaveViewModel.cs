﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using CSharpDemo.Events;
using CSharpDemo.Views;
using LiveCharts;
using Microsoft.Win32;
using NAudio.Wave;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace CSharpDemo.ViewModels
{
    public class AudioFileToWaveViewModel : BindableBase
    {
        #region VM

        private string _audioFilePath;

        public string AudioFilePath
        {
            get => _audioFilePath;
            set
            {
                _audioFilePath = value;
                RaisePropertyChanged();
            }
        }

        private int _progressBarValue;

        public int ProgressBarValue
        {
            get => _progressBarValue;
            set
            {
                _progressBarValue = value;
                RaisePropertyChanged();
            }
        }

        public ChartValues<double> WaveLineSeries { get; set; }

        #endregion

        #region DelegateCommand

        public DelegateCommand<AudioFileToWaveView> WindowLoadedCommand { get; }
        public DelegateCommand ImportAudioFileCommand { get; }

        #endregion

        private AudioFileToWaveView _view;
        private readonly IEventAggregator _eventAggregator;
        private readonly BackgroundWorker _backgroundWorker;
        private AudioFileReader _audioFileReader;
        private readonly List<Point> _lineData = new List<Point>();

        public AudioFileToWaveViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            WaveLineSeries = new ChartValues<double>();

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += Worker_OnDoWork;
            _backgroundWorker.ProgressChanged += Worker_OnProgressChanged;
            _backgroundWorker.RunWorkerCompleted += Worker_OnRunWorkerCompleted;

            WindowLoadedCommand = new DelegateCommand<AudioFileToWaveView>(delegate(AudioFileToWaveView view)
            {
                _view = view;
            });

            ImportAudioFileCommand = new DelegateCommand(delegate
            {
                var fileDialog = new OpenFileDialog
                {
                    // 设置默认格式
                    DefaultExt = ".wav",
                    Filter = "音频文件(*.wav)|*.wav"
                };
                if (fileDialog.ShowDialog() == true)
                {
                    AudioFilePath = fileDialog.FileName;

                    //开始处理数据
                    _backgroundWorker.RunWorkerAsync();
                }
            });
        }

        private void Worker_OnDoWork(object sender, DoWorkEventArgs e)
        {
            _audioFileReader = new AudioFileReader(_audioFilePath);
            var bytes = new byte[_audioFileReader.Length];
            _audioFileReader.Read(bytes, 0, bytes.Length);
            var waveData = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, waveData, 0, bytes.Length);

            var actualWidth = _view.LineChart.ActualWidth;
            var yScale = _view.LineChart.ActualHeight;
            var index = waveData.Length / (int)actualWidth;

            for (var i = 0; i < actualWidth; i++)
            {
                var y = yScale - waveData[i * index] * yScale;

                _lineData.Add(new Point(i, y));

                var percent = (i + 1) / (float)actualWidth;
                _backgroundWorker.ReportProgress((int)(percent * 100));
                Thread.Sleep(1);
            }
        }

        private void Worker_OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBarValue = e.ProgressPercentage;
        }

        private void Worker_OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _eventAggregator.GetEvent<WavePointEvent>().Publish(_lineData);

            foreach (var point in _lineData)
            {
                WaveLineSeries.Add(point.Y);
            }
        }
    }
}