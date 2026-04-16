using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using CSharpDemo.Model;

namespace CSharpDemo.Utils
{
    public static class DrawPath
    {
        /// <summary>
        /// 绘制渐变的条形
        /// </summary>
        /// <param name="spectrumData">频谱数据</param>
        /// <param name="width">绘制频谱图的视图宽度</param>
        /// <param name="height">绘制频谱图的视图高度</param>
        /// <param name="target">绘图目标</param>
        /// <param name="bottomColor">下方颜色</param>
        /// <param name="topColor">上方颜色</param>
        /// <param name="xOffset">绘图的起始 X 坐标</param>
        /// <param name="yOffset">绘图的起始 Y 坐标</param>
        /// <param name="spacing">条形与条形之间的间隔(像素)</param>
        public static void DrawGradientStrips(
            this FrequencyDomainData spectrumData, double width, double height, Path target, Color bottomColor,
            Color topColor, double xOffset, double spacing
        )
        {
            // 后续如果需要更专业的频谱分布方式，再考虑引入 frequencies。
            var frequencies = spectrumData.Frequencies;
            var magnitudes = spectrumData.Magnitudes;
            var stripCount = magnitudes.Length;

            //竖条宽度
            var stripWidth = (width - spacing * stripCount) / stripCount;

            var maxMagnitude = 0.0;
            foreach (var magnitude in magnitudes)
            {
                var mag = Math.Abs(magnitude);
                if (mag > maxMagnitude) maxMagnitude = mag;
            }

            // 竖条高度缩放比例
            var scale = maxMagnitude > 0 ? height / maxMagnitude : 0;

            // 竖条X轴位置数组
            var stripAxisPositions = new Point[stripCount];

            for (var i = 0; i < stripCount; i++)
            {
                var x = stripWidth * i + spacing * i + xOffset;

                // 计算每个竖条的高度，但是不能超过绘制频谱图的视图高度
                var y = magnitudes[i] * scale;

                stripAxisPositions[i] = new Point(x, y);
            }

            // 获取最小的振幅高度
            var yBase = Math.Abs(magnitudes.Min());

            //生成一系列频谱竖条
            var geometry = new GeometryGroup();
            for (var i = 0; i < stripCount; i++)
            {
                var point = stripAxisPositions[i];
                var stripHeight = point.Y;
                if (stripHeight < 0)
                {
                    stripHeight = -stripHeight;
                }

                //每根竖条的四个角坐标。圆点在视图的左上角
                var endPoints = new[]
                {
                    new Point(point.X, point.Y), //左下角
                    new Point(point.X, point.Y + stripHeight), //左上角
                    new Point(point.X + stripWidth, point.Y + stripHeight), //右上角
                    new Point(point.X + stripWidth, point.Y) //右下角
                };

                var figure = new PathFigure
                {
                    StartPoint = endPoints[0]
                };

                figure.Segments.Add(new PolyLineSegment(endPoints, false));
                geometry.Children.Add(new PathGeometry { Figures = { figure } });
            }

            target.Data = geometry;
            var linearGradientBrush = new LinearGradientBrush(
                bottomColor, topColor, new Point(0, 0), new Point(0, 1)
            );
            target.Fill = linearGradientBrush;
        }
    }
}