using CSharpDemo.Service;
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

        private string _highFrequencyValue;

        public string HighFrequencyValue
        {
            get => _highFrequencyValue;
            set
            {
                _highFrequencyValue = value;
                RaisePropertyChanged(() => HighFrequencyValue);
            }
        }

        public RelayCommand StartCollectDataCommand { get; set; }

        private readonly IFrequencyDataService _dataService;

        public FrequencyViewModel(IFrequencyDataService dataService)
        {
            _dataService = dataService;
            StartCollectDataCommand = new RelayCommand(StartCollectData);
        }

        private void StartCollectData()
        {
            HighFrequencyValue = _dataService.GetFrequency();
        }
    }
}