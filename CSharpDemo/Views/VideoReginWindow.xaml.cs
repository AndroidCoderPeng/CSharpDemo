using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using CSharpDemo.Model;
using Newtonsoft.Json;

namespace CSharpDemo.Views
{
    public partial class VideoReginWindow : Window
    {
        public VideoReginWindow(string data)
        {
            InitializeComponent();

            var width = SystemParameters.PrimaryScreenWidth;
            var height = SystemParameters.PrimaryScreenHeight;
            SizeTextBlock.Text = $"屏幕尺寸：[{width},{height}]";

            var regin = JsonConvert.DeserializeObject<ReginModel>(data);
            Brush colorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(regin.color));
            ModeTextBlock.Foreground = colorBrush;
            ModeTextBlock.Text = $"算法模板：{regin.code}";

            var position = regin.position;
            var leftTop = new Point(position[0].y * width, position[0].x * height);
            var rightTop = new Point(position[1].y * width, position[1].x * height);
            var leftBottom = new Point(position[2].y * width, position[2].x * height);
            var rightBottom = new Point(position[3].y * width, position[3].x * height);

            var rectangle = new Polygon();
            var points = new[] { leftTop, rightTop, rightBottom, leftBottom };
            rectangle.Points = new PointCollection(points);
            rectangle.Stroke = colorBrush;
            rectangle.StrokeThickness = 3;
            RectangleCanvas.Children.Add(rectangle);
        }
    }
}