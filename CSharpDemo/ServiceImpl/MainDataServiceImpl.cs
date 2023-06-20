using System.Collections.ObjectModel;
using CSharpDemo.Service;

namespace CSharpDemo.ServiceImpl
{
    public class MainDataServiceImpl : IMainDataService
    {
        private readonly string[] _functions = { "摄像头", "波形图", "UDP服务端", "界面传值", "串口通信", "海康摄像头", "点数对话框" };

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