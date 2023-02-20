using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

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

        public RelayCommand StartCollectDataCommand { get; set; }

        public FrequencyViewModel()
        {
            StartCollectDataCommand = new RelayCommand(StartCollectData);
        }

        private void StartCollectData()
        {
        }
    }
}