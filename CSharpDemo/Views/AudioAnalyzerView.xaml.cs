using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using NAudio.Wave;

namespace CSharpDemo.Views
{
    public partial class AudioAnalyzerView : UserControl
    {
        private WaveOutEvent _wavePlayer;
        private TimeSpan _duration;
        private readonly DispatcherTimer _positionTimer;
        
        public AudioAnalyzerView()
        {
            InitializeComponent();
            
            SelectAudioButton.Click += SelectAudioButton_Click;
            
            _positionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30) // ~33 FPS
            };

            _positionTimer.Tick += PositionTimer_Tick;
        }
        
        private void SelectAudioButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                // 设置默认格式
                DefaultExt = ".wav",
                Filter = "WAV 文件 (*.wav)|*.wav|MP3 文件 (*.mp3)|*.mp3"
            };
            var result = fileDialog.ShowDialog();
            if (result != true) return;

            var audioFilePath = fileDialog.FileName;
            AudioFilePathTextBox.Text = audioFilePath;

            ReadAndPlayAudioFile(audioFilePath);
        }
        
        /// <summary>
        /// WAV - WaveFileReader
        /// MP3 - Mp3FileReader
        /// </summary>
        /// <param name="path"></param>
        private void ReadAndPlayAudioFile(string path)
        {
            
        }
        
        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// 停止播放并清理资源
        /// </summary>
        private void StopAndCleanup()
        {
            _positionTimer?.Stop();

            if (_wavePlayer != null)
            {
                _wavePlayer.Stop();
                _wavePlayer.Dispose();
                _wavePlayer = null;
            }
        }
    }
}