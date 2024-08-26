using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Mvvm;

namespace CSharpDemo.ViewModels
{
    public class CameraViewModel : BindableBase
    {
        #region VM

        private BitmapSource _captureImageSource;

        public BitmapSource CaptureImageSource
        {
            get => _captureImageSource;
            set
            {
                _captureImageSource = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource _imagePreviewSource;

        public BitmapSource ImagePreviewSource
        {
            get => _imagePreviewSource;
            set
            {
                _imagePreviewSource = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region DelegateCommand

        public DelegateCommand OpenCameraCommand { set; get; }
        public DelegateCommand CaptureImageCommand { set; get; }
        public DelegateCommand CloseCameraCommand { set; get; }

        #endregion

        private VideoCapture _capCamera;
        private readonly Mat _imageMat = new Mat();
        private Thread _cameraPreviewThread;

        public CameraViewModel()
        {
            OpenCameraCommand = new DelegateCommand(OpenCamera);
            CaptureImageCommand = new DelegateCommand(CaptureImage);
            CloseCameraCommand = new DelegateCommand(CloseCamera);
        }

        private void OpenCamera()
        {
            if (_capCamera != null && _cameraPreviewThread != null)
            {
                _cameraPreviewThread.Abort();
                DisposeCamera();
            }

            //默认打开第一个摄像头
            _capCamera = new VideoCapture(0);
            _capCamera.Fps = 30;

            //启动相机预览线程
            _cameraPreviewThread = new Thread(PlayCamera);
            _cameraPreviewThread.Start();
        }

        private void CaptureImage()
        {
            //显示某一帧画面
            CaptureImageSource = _imageMat.ToBitmapSource();
        }

        private void CloseCamera()
        {
            DisposeCamera();
        }

        private void PlayCamera()
        {
            while (_capCamera != null && !_capCamera.IsDisposed)
            {
                _capCamera.Read(_imageMat);
                if (_imageMat.Empty()) break;
                Application.Current.Dispatcher.Invoke(delegate { ImagePreviewSource = _imageMat.ToBitmapSource(); });
            }
        }

        private void DisposeCamera()
        {
            if (_capCamera != null && _capCamera.IsOpened())
            {
                _capCamera.Dispose();
                _capCamera = null;
            }
        }
    }
}