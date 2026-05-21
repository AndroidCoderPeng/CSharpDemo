using System.Collections.Generic;
using System.Linq;

namespace CSharpDemo.Service
{
    public class AppDataServiceImpl : IAppDataService
    {
        private readonly string[] _itemTitles =
        {
            "相关仪算法测试", "音频可视化", "串口通信"
        };

        public List<string> GetItemModels()
        {
            return _itemTitles.ToList();
        }
    }
}