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
        /// 画圆环条
        /// </summary>
        /// <param name="spectrumData"></param>
        /// <param name="height">绘制频谱图的视图高度</param>
        /// <param name="target"></param>
        /// <param name="inner"></param>
        /// <param name="outer"></param>
        /// <param name="xOffset">如果没有 xOffset/yOffset，圆心会在左上角 (0,0)，圆环大部分超出可视区域！</param>
        /// <param name="yOffset">如果没有 xOffset/yOffset，圆心会在左上角 (0,0)，圆环大部分超出可视区域！</param>
        /// <param name="radius">圆环半径</param>
        /// <param name="spacing">圆环上面小竖条间距</param>
        /// <param name="rotation">圆环旋转角度</param>
        public static void DrawCircularGradientStrips(
            this FrequencyDomainData spectrumData,
            double height, Path target, Color inner, Color outer, double xOffset, double yOffset, double radius,
            double spacing, double rotation)
        {
            var frequencies = spectrumData.Frequencies;
            var magnitudes = spectrumData.Magnitudes;
            var stripCount = magnitudes.Length;

            //旋转角度转弧度
            var rotationRadian = Math.PI / 180 * rotation;

            //等分圆周，每个（竖条+空白）对应的弧度
            var blockRadian = Math.PI * 2 / stripCount;

            //每个空隙对应的弧度
            var spacingRadian = Math.PI / 180 * spacing;

            //每个竖条对应的弧度
            var stripRadian = blockRadian - spacingRadian;

            var maxMagnitude = 0.0;
            foreach (var magnitude in magnitudes)
            {
                var mag = Math.Abs(magnitude);
                if (mag > maxMagnitude) maxMagnitude = mag;
            }

            // 计算去掉圆环还剩下多少高度空间
            var remainingHeight = (height - radius * 2) / 2;

            // 竖条高度缩放比例
            var scale = maxMagnitude > 0 ? remainingHeight / maxMagnitude : 0;

            var pointArray = new Point[stripCount];
            for (var i = 0; i < stripCount; i++)
            {
                var x = blockRadian * i + rotationRadian; // 弧度
                var y = Math.Abs(magnitudes[i]) * scale; // 弧度所对应的竖条的高度
                pointArray[i] = new Point(x, y);
            }

            var geometry = new PathGeometry();
            foreach (var point in pointArray)
            {
                var sinStart = Math.Sin(point.X);
                var sinEnd = Math.Sin(point.X + stripRadian);
                var cosStart = Math.Cos(point.X);
                var cosEnd = Math.Cos(point.X + stripRadian);

                var polygon = new[]
                {
                    new Point(cosStart * radius + xOffset, sinStart * radius + yOffset),
                    new Point(cosEnd * radius + xOffset, sinEnd * radius + yOffset),
                    new Point(cosEnd * (radius + point.Y) + xOffset, sinEnd * (radius + point.Y) + yOffset),
                    new Point(cosStart * (radius + point.Y) + xOffset, sinStart * (radius + point.Y) + yOffset)
                };

                var figure = new PathFigure
                {
                    StartPoint = polygon[0],
                    IsFilled = true
                };

                figure.Segments.Add(new PolyLineSegment(polygon, false));
                geometry.Figures.Add(figure);
            }

            target.Data = geometry;

            var maxHeight = pointArray.Max(point => point.Y);
            var brush = new LinearGradientBrush(new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0),
                    new GradientStop(inner, radius / (radius + maxHeight)),
                    new GradientStop(outer, 1)
                },
                new Point(xOffset, 0),
                new Point(xOffset, 1)
            );

            target.Fill = brush;
        }

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
        public static void DrawGradientCurve(
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