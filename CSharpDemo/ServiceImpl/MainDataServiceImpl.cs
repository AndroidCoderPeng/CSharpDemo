using System.Collections.ObjectModel;
using CSharpDemo.Service;

namespace CSharpDemo.ServiceImpl
{
    public class MainDataServiceImpl : IMainDataService
    {
        private readonly string[] _functions = { "摄像头", "波形图", "TCP服务端", "圆形进度条", "委托传值" };

        public ObservableCollection<string> GetFunctionModels()
        {
            var functionModels = new ObservableCollection<string>();

            foreach (var function in _functions)
            {
                functionModels.Add(function);
            }

            return functionModels;
        }
    }
}