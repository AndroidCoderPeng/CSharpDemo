using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace CSharpDemo.Service
{
    public class AppDataServiceImpl : IAppDataService
    {
        private readonly string[] _itemTitles = { "摄像头", "界面传值", "串口通信", "水听器数据解析", "音频转波形图", "海康摄像头" };

        public List<string> GetItemModels()
        {
            return _itemTitles.ToList();
        }

        public Color[] GetAllHsvColors()
        {
            var result = new Color[256 * 6];

            for (var i = 0; i <= 255; i++)
            {
                result[i] = Color.FromArgb(255, 255, (byte)i, 0);
            }

            for (var i = 0; i <= 255; i++)
            {
                result[256 + i] = Color.FromArgb(255, (byte)(255 - i), 255, 0);
            }

            for (var i = 0; i <= 255; i++)
            {
                result[512 + i] = Color.FromArgb(255, 0, 255, (byte)i);
            }

            for (var i = 0; i <= 255; i++)
            {
                result[768 + i] = Color.FromArgb(255, 0, (byte)(255 - i), 255);
            }

            for (var i = 0; i <= 255; i++)
            {
                result[1024 + i] = Color.FromArgb(255, (byte)i, 0, 255);
            }

            for (var i = 0; i <= 255; i++)
            {
                result[1280 + i] = Color.FromArgb(255, 255, 0, (byte)(255 - i));
            }

            return result;
        }
    }
}