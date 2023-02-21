using CSharpDemo.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace CSharpDemo.ViewModel
{
    public class CircleLoadingViewModel : ViewModelBase
    {
        private double _processValue;

        public double ProcessValue
        {
            get => _processValue;
            set
            {
                _processValue = value;
                RaisePropertyChanged(() => ProcessValue);
            }
        }

        public CircleLoadingViewModel()
        {
            Messenger.Default.Register<double>(this, MessengerToken.StartLoading, it => { ProcessValue = it; });
        }
    }
}