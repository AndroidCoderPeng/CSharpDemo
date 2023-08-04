using System.Windows;

namespace CSharpDemo.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            CancelButton.Click += delegate { DialogResult = false; };
            ConfirmButton.Click += delegate { DialogResult = true; };
        }

        private void LoginWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            MouseDown += delegate { DragMove(); };
        }
    }
}