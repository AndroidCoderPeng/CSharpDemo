using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using CSharpDemo.Model;

namespace CSharpDemo.Utils
{
    public static class DrawPath
    {
        /// <summary>
        /// 画四周渐变边框
        /// </summary>
        /// <param name="bassScale">高低音转化比例</param>
        /// <param name="topBorder"></param>
        /// <param name="bottomBorder"></param>
        /// <param name="leftBorder"></param>
        /// <param name="rightBorder"></param>
        /// <param name="inner"></param>
        /// <param name="outer"></param>
        /// <param name="stroke">边框默认宽度</param>
        public static void DrawGradientBorder(
            this double bassScale,
            Rectangle topBorder, Rectangle bottomBorder, Rectangle leftBorder, Rectangle rightBorder, Color inner,
            Color outer, double stroke)
        {
            //边框粗细根据音频高低音变化
            var thickness = (int)(stroke * bassScale);
            // Console.WriteLine($@"bassScale: {bassScale}, thickness: {thickness}");

            topBorder.Height = thickness;
            bottomBorder.Height = thickness;
            leftBorder.Width = thickness;
            rightBorder.Width = thickness;

            topBorder.Fill = new LinearGradientBrush(outer, inner, 90);
            bottomBorder.Fill = new LinearGradientBrush(inner, outer, 90);
            leftBorder.Fill = new LinearGradientBrush(outer, inner, 0);
            rightBorder.Fill = new LinearGradientBrush(inner, outer, 0);
        }

        /// <summary>
        /// 画曲线
        /// </summary>
        /// <param name="spectrumData"></param>
        /// <param name="width">绘制频谱图的视图宽度</param>
        /// <param name="height">绘制频谱图的视图高度</param>
        /// <param name="target"></param>
        /// <param name="brush"></param>
        /// <param name="xOffset"></param>
        public static void DrawCurve(
            this TimeDomainData spectrumData,
            double width, double height, Path target, Brush brush, double xOffset)
        {
            var timeAxis = spectrumData.TimeAxis;
            var amplitude = spectrumData.Amplitude;
            var pointCount = timeAxis.Length;

            var maxAmplitude = 0.0;
            foreach (var magnitude in amplitude)
            {
                var mag = Math.Abs(magnitude);
                if (mag > maxAmplitude) maxAmplitude = mag;
            }

            // 波峰波谷缩放比例
            var scale = maxAmplitude > 0 ? (height / 2) / maxAmplitude : 0;
            
            var pointArray = new Point[pointCount];
            for (var i = 0; i < pointCount; i++)
            {
                var x = i * width / pointCount + xOffset;
                var y = height / 2 - amplitude[i] * scale;
                pointArray[i] = new Point(x, y);
            }

            var figure = new PathFigure
            {
                StartPoint = pointArray[0]
            };
            figure.Segments.Add(new PolyLineSegment(pointArray, true));

            target.Data = new PathGeometry { Figures = { figure } };
            target.StrokeThickness = 2;
            target.Stroke = brush;
        }

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
        /// <param name="spacing">条形与条形之间的间隔(像素)</param>
        public static void DrawGradientStrips(
            this FrequencyDomainData spectrumData,
            double width, double height, Path target, Color bottomColor, Color topColor, double xOffset, double spacing)
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

            //生成一系列频谱竖条
            var geometry = new GeometryGroup();

            // 找到最小高度作为基准线
            var minAbsY = double.MaxValue;
            foreach (var point in stripAxisPositions)
            {
                var absY = Math.Abs(point.Y);
                if (absY < minAbsY) minAbsY = absY;
            }

            for (var i = 0; i < stripCount; i++)
            {
                var point = stripAxisPositions[i];
                var absY = Math.Abs(point.Y);

                // 只绘制超出基准线的部分
                var stripHeight = absY - minAbsY;

                // Y轴翻转：WPF坐标系Y向下为正，竖条应从底部向上绘制
                var yBase = height;
                var stripTopY = yBase - stripHeight;

                // Console.WriteLine($@"X = {point.X},y = {point.Y}, stripTopY = {stripTopY}, height = {stripHeight}");

                //每根竖条的四个角坐标。圆点在视图的左上角
                var endPoints = new[]
                {
                    new Point(point.X, yBase), //左下角
                    new Point(point.X, stripTopY), //左上角
                    new Point(point.X + stripWidth, stripTopY), //右上角
                    new Point(point.X + stripWidth, yBase) //右下角
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