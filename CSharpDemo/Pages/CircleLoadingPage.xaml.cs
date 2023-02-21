using System.Windows.Controls;
using CSharpDemo.Utils;
using GalaSoft.MvvmLight.Messaging;

namespace CSharpDemo.Pages
{
    public partial class CircleLoadingPage : Page
    {
        public CircleLoadingPage()
        {
            InitializeComponent();

            Messenger.Default.Send(this, MessengerToken.StartLoading);
        }
    }
}