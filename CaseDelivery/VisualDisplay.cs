using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BestDelivery;

namespace CaseDelivery
{
    internal class VisualDisplay
    {
        private const int CanvasPadding = 20;
        private const double DefaultPointSize = 12;
        private const double DepotPointSize = 14;
        private const double LineThickness = 3;
        private const double AxisThickness = 1.5;
        private const double ArrowThickness = 1.5;
        private const double LabelFontSize = 12;

        private static readonly SolidColorBrush s_DepotBrush = new SolidColorBrush(Color.FromRgb(0, 100, 0));
        private static readonly SolidColorBrush s_OrderBrush = new SolidColorBrush(Color.FromRgb(0, 177, 79));
        private static readonly SolidColorBrush s_WhiteBrush = Brushes.White;
        private static readonly SolidColorBrush s_AxisBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));
        private static readonly SolidColorBrush s_TextBackground = new SolidColorBrush(Color.FromArgb(200, 13, 13, 13));

        private readonly Canvas _canvas;

        public VisualDisplay(Canvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        public void VisualizeRoute(Order[] orders, int[] route, BestDelivery.Point depot)//Визуализация маршрута
        {
            _canvas.Children.Clear();

            if (orders == null || route == null || orders.Length == 0)
                return;

            DrawCoordinateAxes();

            var allPoints = orders.Select(o => o.Destination).ToList();
            var bounds = CalculateBounds(allPoints);

            var scaledDepot = ScalePoint(depot, bounds);
            DrawPoint(scaledDepot, s_DepotBrush, "Склад", DepotPointSize);

            var previousScaled = scaledDepot;

            for (int i = 1; i < route.Length; i++)
            {
                var currentPoint = route[i] == -1 ? depot : orders.First(o => o.ID == route[i]).Destination;
                var currentScaled = ScalePoint(currentPoint, bounds);

                DrawRouteLine(previousScaled, currentScaled, isLastSegment: i == route.Length - 1);

                var label = route[i] == -1 ? "Склад" : $"Заказ {route[i]}";
                var brush = route[i] == -1 ? s_DepotBrush : s_OrderBrush;
                var size = route[i] == -1 ? DepotPointSize : DefaultPointSize;

                DrawPoint(currentScaled, brush, label, size);

                previousScaled = currentScaled;
            }
        }
        public static BestDelivery.Point ConvertCanvasToCoordinates(System.Windows.Point clickPos, Order[] orders, double canvasWidth, double canvasHeight)//Преобразуем точку по клику для Canvas
        {
            var allPoints = orders.Select(o => o.Destination).ToList();
            var bounds = CalculateBounds(allPoints);

            double x = (clickPos.X - 20) / (canvasWidth - 40) * (bounds.maxX - bounds.minX) + bounds.minX;
            double y = (canvasHeight - 20 - clickPos.Y) / (canvasHeight - 40) * (bounds.maxY - bounds.minY) + bounds.minY;

            return new BestDelivery.Point { X = x, Y = y };
        }

        private static (double minX, double maxX, double minY, double maxY) CalculateBounds(List<BestDelivery.Point> points)//Вычисляет точки для масштабирования
        {
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            double padding = 0.1 * (maxX - minX);
            return (minX - padding, maxX + padding, minY - padding, maxY + padding);
        }

        private System.Windows.Point ScalePoint(BestDelivery.Point point, (double minX, double maxX, double minY, double maxY) bounds)//Преобразует реальные координаты в координаты Canvas
        {
            double canvasWidth = _canvas.ActualWidth;
            double canvasHeight = _canvas.ActualHeight;

            double scaledX = (point.X - bounds.minX) / (bounds.maxX - bounds.minX) * (canvasWidth - 2 * CanvasPadding) + CanvasPadding;
            double scaledY = canvasHeight - CanvasPadding - (point.Y - bounds.minY) / (bounds.maxY - bounds.minY) * (canvasHeight - 2 * CanvasPadding);

            return new System.Windows.Point(scaledX, scaledY);
        }

        private void DrawCoordinateAxes()//Рисует оси X/Y со стрелками и подписями
        {
            double canvasWidth = _canvas.ActualWidth;
            double canvasHeight = _canvas.ActualHeight;

            DrawLine(new System.Windows.Point(CanvasPadding, canvasHeight - CanvasPadding),
                    new System.Windows.Point(canvasWidth - CanvasPadding, canvasHeight - CanvasPadding),
                    s_AxisBrush, AxisThickness);

            DrawLine(new System.Windows.Point(CanvasPadding, CanvasPadding),
                    new System.Windows.Point(CanvasPadding, canvasHeight - CanvasPadding),
                    s_AxisBrush, AxisThickness);

            DrawArrow(new System.Windows.Point(canvasWidth - CanvasPadding, canvasHeight - CanvasPadding),
                     new System.Windows.Point(canvasWidth - CanvasPadding - 10, canvasHeight - CanvasPadding - 5));
            DrawArrow(new System.Windows.Point(canvasWidth - CanvasPadding, canvasHeight - CanvasPadding),
                     new System.Windows.Point(canvasWidth - CanvasPadding - 10, canvasHeight - CanvasPadding + 5));

            DrawArrow(new System.Windows.Point(CanvasPadding, CanvasPadding),
                     new System.Windows.Point(CanvasPadding - 5, CanvasPadding + 10));
            DrawArrow(new System.Windows.Point(CanvasPadding, CanvasPadding),
                     new System.Windows.Point(CanvasPadding + 5, CanvasPadding + 10));

            CreateTextBlock("X", canvasWidth - 15, canvasHeight - 35, isBold: true);
            CreateTextBlock("Y", 25, 10, isBold: true);
        }

        private void DrawArrow(System.Windows.Point start, System.Windows.Point end)//Рисует стрелку
        {
            DrawLine(start, end, s_AxisBrush, ArrowThickness);
        }

        private void DrawLine(System.Windows.Point from, System.Windows.Point to, SolidColorBrush brush, double thickness)//Рисует линию с заданными параметрами
        {
            var line = new Line
            {
                X1 = from.X,
                Y1 = from.Y,
                X2 = to.X,
                Y2 = to.Y,
                Stroke = brush,
                StrokeThickness = thickness
            };
            _canvas.Children.Add(line);
        }

        private void DrawRouteLine(System.Windows.Point from, System.Windows.Point to, bool isLastSegment)//Рисует отрезок маршрута (пунктир для последнего сегмента)
        {
            var line = new Line
            {
                X1 = from.X,
                Y1 = from.Y,
                X2 = to.X,
                Y2 = to.Y,
                Stroke = isLastSegment ? s_OrderBrush : s_WhiteBrush,
                StrokeThickness = LineThickness,
                StrokeDashArray = isLastSegment ? new DoubleCollection { 4, 2 } : null
            };
            _canvas.Children.Add(line);
        }

        private void DrawPoint(System.Windows.Point center, SolidColorBrush color, string text, double size)//Рисует точку с подписью
        {
            var ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = color,
                Stroke = s_WhiteBrush,
                StrokeThickness = 1.5
            };

            Canvas.SetLeft(ellipse, center.X - size / 2);
            Canvas.SetTop(ellipse, center.Y - size / 2);
            _canvas.Children.Add(ellipse);

            CreateTextBlock(text, center.X + size / 2 + 2, center.Y - LabelFontSize, background: s_TextBackground);
        }

        private void CreateTextBlock(string text, double left, double top, bool isBold = false, SolidColorBrush background = null)//Создаёт текстовую подпись с фоном
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = LabelFontSize,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                Foreground = s_WhiteBrush,
                Background = background,
                Padding = new Thickness(3, 1, 3, 1)
            };

            Canvas.SetLeft(textBlock, left);
            Canvas.SetTop(textBlock, top);
            _canvas.Children.Add(textBlock);
        }
    }
}