using System.Windows.Navigation;
using CSharpDemo.Utils;

namespace CSharpDemo.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static NavigationService Service;

        public MainWindow()
        {
            InitializeComponent();

            Service = ContentFrame.NavigationService;
            Service.Navigate("CameraPage".CreateUri());
        }
    }
}