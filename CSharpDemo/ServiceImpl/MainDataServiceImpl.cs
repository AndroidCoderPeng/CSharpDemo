using System.Collections.ObjectModel;
using CSharpDemo.Service;

namespace CSharpDemo.ServiceImpl
{
    public class MainDataServiceImpl : IMainDataService
    {
        private readonly string[] _itemTitles = { "摄像头", "界面传值", "串口通信", "水听器数据解析", "实时音频", "海康摄像头" };

        public ObservableCollection<string> GetItemModels()
        {
            var itemModels = new ObservableCollection<string>();

            foreach (var function in _itemTitles)
            {
                itemModels.Add(function);
            }

            return itemModels;
        }
    }
}