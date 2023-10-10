using System.Collections.ObjectModel;
using System.Windows.Media;

namespace CSharpDemo.Service
{
    public interface IMainDataService
    {
        ObservableCollection<string> GetItemModels();

        /// <summary>
        /// 获取 HSV 中所有的基础颜色 (饱和度和明度均为最大值)
        /// </summary>
        /// <returns>所有的 HSV 基础颜色(共 256 * 6 个, 并且随着索引增加, 颜色也会渐变)</returns>
        Color[] GetAllHsvColors();
    }
}