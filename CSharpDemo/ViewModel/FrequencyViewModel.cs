using CSharpDemo.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace CSharpDemo.ViewModel
{
    public class FrequencyViewModel : ViewModelBase
    {
        private int _minCurrentValue = 20;

        public int MinCurrentValue
        {
            get => _minCurrentValue;
            set
            {
                _minCurrentValue = value;
                RaisePropertyChanged(() => MinCurrentValue);
            }
        }

        private int _maxCurrentValue = 20;

        public int MaxCurrentValue
        {
            get => _maxCurrentValue;
            set
            {
                _maxCurrentValue = value;
                RaisePropertyChanged(() => MaxCurrentValue);
            }
        }

        public RelayCommand GoBackCommand { get; set; }
        public RelayCommand StartCollectDataCommand { get; set; }

        public FrequencyViewModel()
        {
            GoBackCommand = new RelayCommand(() =>
            {
                Messenger.Default.Send("", MessengerToken.CloseFrequencyWindow);
            });
            StartCollectDataCommand = new RelayCommand(StartCollectData);
        }

        private void StartCollectData()
        {
        }
    }
}