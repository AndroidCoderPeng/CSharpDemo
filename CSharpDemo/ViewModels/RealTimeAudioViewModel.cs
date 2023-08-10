using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using CSharpDemo.Utils;
using CSharpDemo.Views;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Prism.Commands;
using Prism.Mvvm;

namespace CSharpDemo.ViewModels
{
    public class RealTimeAudioViewModel : BindableBase
    {
        #region DelegateCommand

        public DelegateCommand<RealTimeAudioView> WindowLoadedCommand { get; }
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

        private RealTimeAudioView _view;
        private bool _isStartRecording;

        private WaveFileWriter _waveFileWriter;

        //波形数据集
        private List<float> _waveDataArray = new List<float>();

        public RealTimeAudioViewModel()
        {
            CurrentVolume = GetCurrentMicVolume();

            WindowLoadedCommand = new DelegateCommand<RealTimeAudioView>(delegate(RealTimeAudioView view)
            {
                _view = view;
            });

            ListenRedSensorCommand = new DelegateCommand(RecordAudio);
            RedSensorMuteCommand = new DelegateCommand(SetCurrentMicVolume);

            ListenBlueSensorCommand = new DelegateCommand(RecordAudio);
            BlueSensorMuteCommand = new DelegateCommand(SetCurrentMicVolume);

            //音频监听
            LazyWaveIn.Value.DataAvailable += delegate(object sender, WaveInEventArgs args)
            {
                _isStartRecording = true;

                var buffer = args.Buffer;
                var bytesRecorded = args.BytesRecorded;
                //写入wav文件
                _waveFileWriter.Write(buffer, 0, bytesRecorded);

                // for (var index = 0; index < bytesRecorded; index += 2)
                // {
                //     var sample = (short)((buffer[index + 1] << 8) | buffer[index + 0]);
                //     var sample32 = sample / 32768f;
                //     _waveDataArray.Add(sample32);
                // }

                // new Label { BackColor = Color.Pink, Width = 5, Height = (int)h, Margin = new Padding(0, baseMidHeight-(int)h, 0, 0) }
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
            var volume = 0;
            var enumerator = new MMDeviceEnumerator();

            //获取音频输入设备
            var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var device in captureDevices)
            {
                Debug.WriteLine($"RealTimeAudioViewModel => {device.FriendlyName}");
            }

            if (captureDevices.Any())
            {
                foreach (var device in captureDevices)
                {
                    if (device.FriendlyName.Contains("Realtek High Definition Audio"))
                    {
                        volume = (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    }
                }
            }

            return volume;
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