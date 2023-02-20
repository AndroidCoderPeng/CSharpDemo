using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using CSharpDemo.Pages;

namespace CSharpDemo.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly NavigationService _service;

        public MainWindow()
        {
            InitializeComponent();

            _service = ContentFrame.NavigationService;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedIndex == -1) return;

            switch (listBox.SelectedIndex)
            {
                case 0:
                    _service.Navigate(new CameraPage());
                    break;
                case 1:
                    _service.Navigate(new LiveChartsPage());
                    break;
                case 2:
                    _service.Navigate(new ScottPlotPage());
                    break;
                case 3:
                    _service.Navigate(new TcpServerPage());
                    break;
                case 4:
                    _service.Navigate(new FrequencyPage());
                    break;
                case 5:
                    _service.Navigate(new CircleLoadingPage());
                    break;
            }
        }

        private void OpenCamera(object sender, RoutedEventArgs e)
        {
            // if (_currentDevice != null)
            // {
            //     if (CameraPreviewPlayer.IsRunning)
            //     {
            //         CameraPreviewPlayer.SignalToStop();
            //         OperateCameraButton.Content = "打开相机";
            //     }
            //     else
            //     {
            //         var videoSource = new VideoCaptureDevice(_currentDevice.MonikerString);
            //         CameraPreviewPlayer.VideoSource = videoSource;
            //         CameraPreviewPlayer.Start();
            //         OperateCameraButton.Content = "关闭相机";
            //     }
            // }
        }
    }
}