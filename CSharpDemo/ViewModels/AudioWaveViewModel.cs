using System.Windows.Media;
using CSharpDemo.Views;
using Prism.Commands;
using Prism.Mvvm;

namespace CSharpDemo.ViewModels
{
    public class AudioWaveViewModel : BindableBase
    {
        #region VM

        private LinearGradientBrush _stripsGradientBrush;

        public LinearGradientBrush StripsGradientBrush
        {
            get => _stripsGradientBrush;
            set
            {
                _stripsGradientBrush = value;
                RaisePropertyChanged();
            }
        }

        private GeometryGroup _stripsGeometry;

        public GeometryGroup StripsGeometry
        {
            get => _stripsGeometry;
            set
            {
                _stripsGeometry = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region DelegateCommand

        public DelegateCommand<AudioWaveView> ViewLoadedCommand { get; }
        public DelegateCommand ViewUnLoadedCommand { get; }

        #endregion

        private AudioWaveView _view;

        public AudioWaveViewModel()
        {
            ViewLoadedCommand = new DelegateCommand<AudioWaveView>(delegate(AudioWaveView view) { _view = view; });
            ViewUnLoadedCommand = new DelegateCommand(delegate { });
        }
    }
}