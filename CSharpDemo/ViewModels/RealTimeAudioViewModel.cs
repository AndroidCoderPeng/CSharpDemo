using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CSharpDemo.Events;
using CSharpDemo.Model;
using CSharpDemo.Utils;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace CSharpDemo.ViewModels
{
    public class RealTimeAudioViewModel : BindableBase
    {
        #region DelegateCommand

        public DelegateCommand ListenRedSensorCommand { get; }
        public DelegateCommand RedSensorMuteCommand { get; }
        public DelegateCommand ListenBlueSensorCommand { get; }
        public DelegateCommand BlueSensorMuteCommand { get; }

        #endregion

        #region VM

        private int _currentVolume;

        private static readonly Lazy<WaveIn> LazyWaveIn =
            new Lazy<WaveIn>(() => new WaveIn { WaveFormat = new WaveFormat(7500, 1) });

        public int CurrentVolume
        {
            get => _currentVolume;
            set
            {
                _currentVolume = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        private bool _isStartRecording;
        private bool _isRedSensor;

        private WaveFileWriter _waveFileWriter;

        public RealTimeAudioViewModel(IEventAggregator aggregator)
        {
            CurrentVolume = GetCurrentMicVolume();

            ListenRedSensorCommand = new DelegateCommand(delegate
            {
                _isRedSensor = true;
                RecordAudio();
            });
            RedSensorMuteCommand = new DelegateCommand(SetCurrentMicVolume);

            ListenBlueSensorCommand = new DelegateCommand(delegate
            {
                _isRedSensor = false;
                RecordAudio();
            });
            BlueSensorMuteCommand = new DelegateCommand(SetCurrentMicVolume);

            //音频监听
            LazyWaveIn.Value.DataAvailable += delegate(object sender, WaveInEventArgs args)
            {
                _isStartRecording = true;

                var buffer = args.Buffer;
                var bytesRecorded = args.BytesRecorded;
                //写入wav文件
                _waveFileWriter.Write(buffer, 0, bytesRecorded);

                var sts = new float[buffer.Length / 2];
                var outIndex = 0;
                for (var i = 0; i < buffer.Length; i += 2)
                {
                    sts[outIndex++] = BitConverter.ToInt16(buffer, i) / 32768f;
                }

                var audioWaveModel = new AudioWaveModel
                {
                    IsRedSensor = _isRedSensor,
                    WavePoints = sts
                };

                aggregator.GetEvent<AudioWavePointEvent>().Publish(audioWaveModel);
            };

            LazyWaveIn.Value.RecordingStopped += delegate
            {
                if (_waveFileWriter != null)
                {
                    _waveFileWriter.Close();
                    _waveFileWriter = null;
                }

                _isStartRecording = false;
            };
        }

        /// <summary>
        /// 录音
        /// </summary>
        private void RecordAudio()
        {
            if (_isStartRecording)
            {
                LazyWaveIn.Value.StopRecording();
            }
            else
            {
                _waveFileWriter = new WaveFileWriter(
                    $"{DirectoryManager.GetAudioDir()}/Audio_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.wav",
                    LazyWaveIn.Value.WaveFormat
                );

                LazyWaveIn.Value.StartRecording();
            }
        }

        private int GetCurrentMicVolume()
        {
            var enumerator = new MMDeviceEnumerator();

            //获取音频输入设备
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            Debug.WriteLine($"RealTimeAudioViewModel => {device.FriendlyName}");
            return (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        }

        #region 静音

        //函数名不能改，否则会报找不到函数错误，dll里面定好了的
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern byte MapVirtualKey(uint uCode, uint uMapType);

        private void SetCurrentMicVolume()
        {
            if (_currentVolume > 0)
            {
                //静音
                keybd_event(0xAD, MapVirtualKey(0xAD, 0), 0x0001, 0);
                keybd_event(0xAD, MapVirtualKey(0xAD, 0), 0x0001 | 0x0002, 0);

                CurrentVolume = 0;
            }
            else
            {
                //非静音
                keybd_event(0xAD, MapVirtualKey(0xAD, 0), 0x0001, 0);
                keybd_event(0xAD, MapVirtualKey(0xAD, 0), 0x0001 | 0x0002, 0);

                CurrentVolume = GetCurrentMicVolume();
            }
        }

        #endregion
    }
}