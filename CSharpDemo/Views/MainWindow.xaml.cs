using System.Collections.Generic;
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
        private readonly List<Page> _pages = new List<Page>();
        private readonly NavigationService _service;

        public MainWindow()
        {
            InitializeComponent();

            _pages.Add(new CameraPage());
            _pages.Add(new LiveChartsPage());
            _pages.Add(new ScottPlotPage());
            _pages.Add(new TcpServerPage());
            _pages.Add(new FrequencyPage());
            _pages.Add(new CircleLoadingPage());

            _service = ContentFrame.NavigationService;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedIndex == -1) return;

            switch (listBox.SelectedIndex)
            {
                case 0:
                    _service.Navigate(_pages[0]);
                    break;
                case 1:
                    _service.Navigate(_pages[1]);
                    break;
                case 2:
                    _service.Navigate(_pages[2]);
                    break;
                case 3:
                    _service.Navigate(_pages[3]);
                    break;
                case 4:
                    _service.Navigate(_pages[4]);
                    break;
                case 5:
                    _service.Navigate(_pages[5]);
                    break;
            }
        }
    }
}