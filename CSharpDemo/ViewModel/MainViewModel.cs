using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace CSharpDemo.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly string[] _functions = { "打开摄像头", "折线图", "波形图", "TCP服务端", "频率筛选", "圆形进度条" };

        public ObservableCollection<string> FunctionModels { get; } = new ObservableCollection<string>();

        public MainViewModel()
        {
            foreach (var function in _functions)
            {
                FunctionModels.Add(function);
            }
        }
    }
}