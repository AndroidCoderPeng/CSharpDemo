using System.Windows;

namespace CSharpDemo.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            CancalButton.Click += delegate { Close(); };
            ConfirmButton.Click += delegate { };
        }

        private void LoginWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            MouseDown += delegate { DragMove(); };
        }
    }
}