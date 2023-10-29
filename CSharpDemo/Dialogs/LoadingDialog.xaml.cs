using System.Windows;

namespace CSharpDemo.Dialogs
{
    public partial class LoadingDialog : Window
    {
        public LoadingDialog(string message)
        {
            InitializeComponent();

            LoadingView.LoadingText = message;
        }
    }
}