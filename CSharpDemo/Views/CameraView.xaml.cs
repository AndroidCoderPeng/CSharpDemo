using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPFMediaKit.DirectShow.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CSharpDemo.Views
{
    public partial class CameraView : UserControl
    {
        private readonly DispatcherTimer _faceImageTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        private byte[] _faceBytes;

        public CameraView()
        {
            InitializeComponent();

            //人脸画面Timer
            _faceImageTimer.Tick += delegate
            {
                var bmp = new RenderTargetBitmap((int)DeviceCamera.ActualWidth, (int)DeviceCamera.ActualHeight,
                    96, 96, PixelFormats.Default);
                DeviceCamera.Measure(DeviceCamera.RenderSize);
                DeviceCamera.Arrange(new Rect(DeviceCamera.RenderSize));
                bmp.Render(DeviceCamera);

                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    _faceBytes = stream.ToArray();
                }
            };
        }

        private void OpenCameraButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_faceImageTimer.IsEnabled && !DeviceCamera.IsPlaying)
            {
                DeviceCamera.Visibility = Visibility.Visible;
                _faceImageTimer.Start();
                DeviceCamera.Play();
            }

            var videoInputNames = MultimediaUtil.VideoInputNames;
            if (videoInputNames.Length > 0)
            {
                //默认选择第一个
                var currentDevice = videoInputNames[0];
                DeviceCamera.VideoCaptureSource = currentDevice;
                _faceImageTimer.Start();
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseCameraButton_OnClick(object sender, RoutedEventArgs e)
        {
            _faceImageTimer.Stop();
            DeviceCamera.Stop();
            DeviceCamera.Visibility = Visibility.Collapsed;
        }

        private void CaptureImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_faceBytes == null) return;
            var ms = new MemoryStream(_faceBytes);
            var bitmap = new Bitmap(ms);
            var bs = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            CaptureImageView.Source = bs;
        }
    }
}