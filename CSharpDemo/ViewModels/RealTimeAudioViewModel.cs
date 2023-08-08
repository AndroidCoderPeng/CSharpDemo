using System.Diagnostics;
using System.Linq;
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
            RedSensorMuteCommand = new DelegateCommand(delegate { });

            ListenBlueSensorCommand = new DelegateCommand(delegate { });
            BlueSensorMuteCommand = new DelegateCommand(delegate { });
        }

        private int GetCurrentMicVolume()
        {
            var volume = 0;
            var enumerator = new MMDeviceEnumerator();

            //获取音频输入设备
            var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var device in captureDevices)
            {
                Debug.WriteLine($"RealTimeAudioViewModel => {device.DeviceFriendlyName}");
            }

            if (captureDevices.Any())
            {
                var mMDevice = captureDevices.ToList()[1];
                volume = (int)(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }

            return volume;
        }

        private void SetCurrentMicVolume(bool isMute)
        {
            var enumerator = new MMDeviceEnumerator();
            var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            if (captureDevices.Any())
            {
                var mMDevice = captureDevices.ToList()[1];
                //false是静音，true是关闭静音
                mMDevice.AudioEndpointVolume.Mute = isMute;
            }
        }
    }
}