using System.Windows;

namespace CSharpDemo.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // private readonly NavigationService _service;

        public MainWindow()
        {
            InitializeComponent();

            // _service = ContentFrame.NavigationService;
            // _service.Navigate("CameraPage".CreateUri());
            // FuncListBox.SelectedIndex = 0;
        }

        // private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        // {
        //     if (!(sender is ListBox listBox))
        //     {
        //         return;
        //     }
        //
        //     switch (listBox.SelectedIndex)
        //     {
        //         case 0:
        //             _service.Navigate("CameraPage".CreateUri());
        //             break;
        //         case 1:
        //             _service.Navigate("ScottPlotPage".CreateUri());
        //             break;
        //         case 2:
        //             _service.Navigate("UdpServerPage".CreateUri());
        //             break;
        //         case 3:
        //             _service.Navigate("TransmitValuePage".CreateUri());
        //             break;
        //         case 4:
        //             _service.Navigate("SerialPortPage".CreateUri());
        //             break;
        //         case 5:
        //             //初始化海康网络摄像头
        //             if (InitHikVisionSdk())
        //             {
        //                 new HikVisionLoginDialog(GetLoginParam) { Owner = this }.ShowDialog();
        //             }
        //             else
        //             {
        //                 MessageBox.Show("NET_DVR_Init error!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //             }
        //             break;
        //         case 6:
        //             _service.Navigate("DataAnalysisPage".CreateUri());
        //             break;
        //     }
        // }
    }
}