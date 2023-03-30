using System.Windows.Controls;
using System.Windows.Navigation;
using CSharpDemo.Utils;

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
            _service.Navigate("CameraPage".CreateUri());
            FuncListBox.SelectedIndex = 0;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListBox listBox))
            {
                return;
            }

            switch (listBox.SelectedIndex)
            {
                case 0:
                    _service.Navigate("CameraPage".CreateUri());
                    break;
                case 1:
                    _service.Navigate("ScottPlotPage".CreateUri());
                    break;
                case 2:
                    _service.Navigate("TcpServerPage".CreateUri());
                    break;
                case 3:
                    _service.Navigate("CircleLoadingPage".CreateUri());
                    break;
                case 4:
                    _service.Navigate("TransmitValuePage".CreateUri());
                    break;
            }
        }
    }
}