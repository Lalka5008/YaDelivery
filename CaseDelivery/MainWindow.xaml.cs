using BestDelivery;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CaseDelivery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _nextOrderId = 1;
        private Order[] currentOrders;
        private BestDelivery.Point depot;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void CalculateRoute_Click(object sender, RoutedEventArgs e)
        {
            int datasetIndex = DatasetComboBox.SelectedIndex + 1;
            switch (datasetIndex)
            {
                case 1:
                    currentOrders = OrderArrays.GetOrderArray1();
                    break;
                case 2:
                    currentOrders = OrderArrays.GetOrderArray2();
                    break;
                case 3:
                    currentOrders = OrderArrays.GetOrderArray3();
                    break;
                case 4:
                    currentOrders = OrderArrays.GetOrderArray4();
                    break;
                case 5:
                    currentOrders = OrderArrays.GetOrderArray5();
                    break;
                case 6:
                    currentOrders = OrderArrays.GetOrderArray6();
                    break;
            }

            depot = currentOrders.First(o => o.ID == -1).Destination;
            _nextOrderId = currentOrders.Max(o => o.ID) + 1;
            // Построение маршрута с получением информации о сегментах
            var routeResult = NearestNeighborAlgorithm.FindOptimalRoute(currentOrders);
            int[] route = routeResult.Route;
            var segments = routeResult.Segments;

            VisualizeRoute(route);
            ShowRouteInfo(route, segments);

            if (RoutingTestLogic.TestRoutingSolution(
                depot,
                currentOrders.Where(o => o.ID != -1).ToArray(),
                route,
                out double routeCost))
            {
                RouteCostText.Text = $"Стоимость маршрута: {routeCost:F2}";
            }
            else
            {
                RouteCostText.Text = "Ошибка в маршруте";
            }
        }
        private void RouteCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentOrders == null || depot.Equals(default(BestDelivery.Point)))
                return;

            // Получаем координаты клика
            System.Windows.Point clickPos = e.GetPosition(RouteCanvas);

            // Находим границы, как в VisualizeRoute
            var allPoints = currentOrders.Select(o => o.Destination).ToList();
            double minX = allPoints.Min(p => p.X);
            double maxX = allPoints.Max(p => p.X);
            double minY = allPoints.Min(p => p.Y);
            double maxY = allPoints.Max(p => p.Y);

            double padding = 0.1 * (maxX - minX);
            minX -= padding;
            maxX += padding;
            minY -= padding;
            maxY += padding;

            double canvasWidth = RouteCanvas.ActualWidth;
            double canvasHeight = RouteCanvas.ActualHeight;

            double x = (clickPos.X - 20) / (canvasWidth - 40) * (maxX - minX) + minX;
            double y = (canvasHeight - 20 - clickPos.Y) / (canvasHeight - 40) * (maxY - minY) + minY;

            var newPoint = new BestDelivery.Point { X = x, Y = y };

            var newOrder = new Order
            {
                ID = _nextOrderId++,
                Destination = newPoint,
                Priority = 0.2
            };

            var ordersList = currentOrders.ToList();
            ordersList.Add(newOrder);
            currentOrders = ordersList.ToArray();

            // Перестраиваем маршрут
            var routeResult = NearestNeighborAlgorithm.FindOptimalRoute(currentOrders);
            int[] route = routeResult.Route;
            var segments = routeResult.Segments;

            // Отображаем
            VisualizeRoute(route);
            ShowRouteInfo(route, segments);

            if (RoutingTestLogic.TestRoutingSolution(depot, currentOrders.Where(o => o.ID != -1).ToArray(), route, out double routeCost))
            {
                RouteCostText.Text = $"Стоимость маршрута: {routeCost:F2}";
            }
            else
            {
                RouteCostText.Text = "Ошибка в маршруте";
            }
        }

        private void VisualizeRoute(int[] route)
        {
            RouteCanvas.Children.Clear();

            if (currentOrders == null || currentOrders.Length == 0)
                return;

            // Добавляем оси координат
            DrawCoordinateAxes();

            var allPoints = currentOrders.Select(o => o.Destination).ToList();

            // Находим границы для масштабирования
            double minX = allPoints.Min(p => p.X);
            double maxX = allPoints.Max(p => p.X);
            double minY = allPoints.Min(p => p.Y);
            double maxY = allPoints.Max(p => p.Y);

            double padding = 0.1 * (maxX - minX);
            minX -= padding;
            maxX += padding;
            minY -= padding;
            maxY += padding;

            // Функция для масштабирования координат
            BestDelivery.Point ScalePoint(double x, double y)
            {
                double canvasWidth = RouteCanvas.ActualWidth;
                double canvasHeight = RouteCanvas.ActualHeight;

                double scaledX = (x - minX) / (maxX - minX) * (canvasWidth - 40) + 20;
                double scaledY = canvasHeight - 20 - (y - minY) / (maxY - minY) * (canvasHeight - 40);
                return new BestDelivery.Point { X = scaledX, Y = scaledY };
            }

            // Рисуем точки и соединяем их линиями
            BestDelivery.Point prevPoint = depot;
            BestDelivery.Point prevScaled = ScalePoint(prevPoint.X, prevPoint.Y);

            // Рисуем склад (темно-зеленый)
            DrawPoint(prevScaled, new SolidColorBrush(Color.FromArgb(255, 0, 100, 0)), "Склад", 14);

            for (int i = 1; i < route.Length; i++)
            {
                BestDelivery.Point currentPoint;
                if (route[i] == -1)
                {
                    currentPoint = depot;
                }
                else
                {
                    currentPoint = currentOrders.First(o => o.ID == route[i]).Destination;
                }

                BestDelivery.Point currentScaled = ScalePoint(currentPoint.X, currentPoint.Y);

                // Рисуем линию
                DrawLine(prevScaled, currentScaled, i == route.Length - 1);

                if (route[i] != -1)
                {
                    // Точки заказов (зеленый Delivery Club)
                    DrawPoint(currentScaled, new SolidColorBrush(Color.FromArgb(255, 0, 177, 79)), $"Заказ {route[i]}", 12);
                }
                else
                {
                    DrawPoint(currentScaled, new SolidColorBrush(Color.FromArgb(255, 0, 100, 0)), "Склад", 14);
                }

                prevPoint = currentPoint;
                prevScaled = currentScaled;
            }
        }

        private void DrawCoordinateAxes()
        {
            double canvasWidth = RouteCanvas.ActualWidth;
            double canvasHeight = RouteCanvas.ActualHeight;

            // Ось X
            var xAxis = new Line
            {
                X1 = 20,
                Y1 = canvasHeight - 20,
                X2 = canvasWidth - 20,
                Y2 = canvasHeight - 20,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)),
                StrokeThickness = 1.5
            };
            RouteCanvas.Children.Add(xAxis);

            // Ось Y
            var yAxis = new Line
            {
                X1 = 20,
                Y1 = 20,
                X2 = 20,
                Y2 = canvasHeight - 20,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)),
                StrokeThickness = 1.5
            };
            RouteCanvas.Children.Add(yAxis);

            // Стрелки для осей
            DrawArrow(new System.Windows.Point(canvasWidth - 20, canvasHeight - 20),
                     new System.Windows.Point(canvasWidth - 30, canvasHeight - 25)); // X
            DrawArrow(new System.Windows.Point(canvasWidth - 20, canvasHeight - 20),
                     new System.Windows.Point(canvasWidth - 30, canvasHeight - 15)); // X
            DrawArrow(new System.Windows.Point(20, 20),
                     new System.Windows.Point(15, 30)); // Y
            DrawArrow(new System.Windows.Point(20, 20),
                     new System.Windows.Point(25, 30)); // Y

            // Подписи осей
            var xLabel = new TextBlock
            {
                Text = "X",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(xLabel, canvasWidth - 15);
            Canvas.SetTop(xLabel, canvasHeight - 35);
            RouteCanvas.Children.Add(xLabel);

            var yLabel = new TextBlock
            {
                Text = "Y",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(yLabel, 25);
            Canvas.SetTop(yLabel, 10);
            RouteCanvas.Children.Add(yLabel);
        }

        private void DrawArrow(System.Windows.Point start, System.Windows.Point end)
        {
            var arrowLine = new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)),
                StrokeThickness = 1.5
            };
            RouteCanvas.Children.Add(arrowLine);
        }

        private void DrawPoint(BestDelivery.Point center, Brush color, string text, double size)
        {
            // Круг
            var ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = color,
                Stroke = Brushes.White,
                StrokeThickness = 1.5
            };

            Canvas.SetLeft(ellipse, center.X - size / 2);
            Canvas.SetTop(ellipse, center.Y - size / 2);
            RouteCanvas.Children.Add(ellipse);

            // Текст
            var label = new TextBlock
            {
                Text = text,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(200, 13, 13, 13)),
                Padding = new Thickness(3, 1, 3, 1)
            };

            Canvas.SetLeft(label, center.X + size / 2 + 2);
            Canvas.SetTop(label, center.Y - label.FontSize);
            RouteCanvas.Children.Add(label);
        }

        private void DrawLine(BestDelivery.Point from, BestDelivery.Point to, bool isLastSegment)
        {
            var line = new Line
            {
                X1 = from.X,
                Y1 = from.Y,
                X2 = to.X,
                Y2 = to.Y,
                Stroke = isLastSegment ?
                    new SolidColorBrush(Color.FromArgb(255, 0, 177, 79)) : // Зеленый Delivery Club для последнего сегмента
                    Brushes.White, // Белый для обычных сегментов
                StrokeThickness = 3,
                StrokeDashArray = isLastSegment ? new DoubleCollection { 4, 2 } : null
            };

            RouteCanvas.Children.Add(line);
        }

        private void ShowRouteInfo(int[] route, List<NearestNeighborAlgorithm.RouteSegment> segments)
        {
            var routeInfo = new List<string>();

            routeInfo.Add("Последовательность точек:");
            for (int i = 0; i < route.Length; i++)
            {
                string pointName = route[i] == -1 ? "Склад" : $"Заказ {route[i]}";
                routeInfo.Add($"{i + 1}. {pointName}");
            }
            routeInfo.Add("Детали сегментов:");
            foreach (var segment in segments)
            {
                routeInfo.Add(segment.ToString());
            }
            RouteListBox.ItemsSource = routeInfo;
        }
    }
}