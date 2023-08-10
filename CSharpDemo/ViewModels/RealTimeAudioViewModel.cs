using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using Prism.Commands;
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

        public RealTimeAudioViewModel()
        {
            CurrentVolume = GetCurrentMicVolume();

            ListenRedSensorCommand = new DelegateCommand(delegate { });
            RedSensorMuteCommand = new DelegateCommand(delegate { SetCurrentMicVolume(true); });

            ListenBlueSensorCommand = new DelegateCommand(delegate { });
            BlueSensorMuteCommand = new DelegateCommand(delegate { SetCurrentMicVolume(false); });
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

        private void SetCurrentMicVolume(bool isMute)
        {
            if (isMute)
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