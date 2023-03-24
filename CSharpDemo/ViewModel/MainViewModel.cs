using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace CSharpDemo.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly string[] _functions = { "摄像头", "折线图", "波形图", "TCP服务端", "频率筛选", "圆形进度条" };

        #region VM

        public ObservableCollection<string> FunctionModels { get; } = new ObservableCollection<string>();

        #endregion

        public MainViewModel()
        {
            foreach (var function in _functions)
            {
                FunctionModels.Add(function);
            }
        }
    }
}