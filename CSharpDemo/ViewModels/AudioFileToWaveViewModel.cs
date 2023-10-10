using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace CSharpDemo.ViewModels
{
    public class AudioFileToWaveViewModel : BindableBase
    {
        #region VM

        private string _audioFilePath;

        public string AudioFilePath
        {
            get => _audioFilePath;
            set
            {
                _audioFilePath = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region DelegateCommand

        public DelegateCommand ImportAudioFileCommand { get; }

        #endregion

        public AudioFileToWaveViewModel(IEventAggregator eventAggregator)
        {
            ImportAudioFileCommand = new DelegateCommand(delegate
            {
                var fileDialog = new OpenFileDialog
                {
                    // 设置默认格式
                    DefaultExt = ".wav",
                    Filter = "音频文件(*.wav)|*.wav"
                };
                if (fileDialog.ShowDialog() == true)
                {
                    AudioFilePath = fileDialog.FileName;

                    //开始播放音频
                }
            });
        }
    }
}