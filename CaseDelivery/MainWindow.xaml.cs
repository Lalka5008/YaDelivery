using BestDelivery;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static CaseDelivery.NearestNeighborAlgorithm;

namespace CaseDelivery
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private int _nextOrderId = 1;
        private Order[] currentOrders;
        private BestDelivery.Point depot;
        private readonly VisualDisplay _routeVisualizer;
        private double _orderPriority;
        private string _priorityDescription;
        private Brush _priorityColor;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; 
            _routeVisualizer = new VisualDisplay(RouteCanvas);
            UpdatePriorityInfo();
        }

        private void CalculateRoute_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedDataset();
            InitializeDepotAndOrderId();
            UpdateRoute();
        }
        public double OrderPriority
        {
            get => _orderPriority;
            set
            {
                if (_orderPriority != value)
                {
                    _orderPriority = value;
                    UpdatePriorityInfo();
                    OnPropertyChanged(nameof(OrderPriority));
                }
            }
        }
        public string PriorityDescription
        {
            get => _priorityDescription;
            set
            {
                _priorityDescription = value;
                OnPropertyChanged(nameof(PriorityDescription));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Brush PriorityColor
        {
            get => _priorityColor;
            set
            {
                _priorityColor = value;
                OnPropertyChanged(nameof(PriorityColor));
            }
        }
        private void UpdatePriorityInfo()
        {
            if (OrderPriority <= 0.3)
            {
                PriorityDescription = "Низкий приоритет";
                PriorityColor = Brushes.LimeGreen;
            }
            else if (OrderPriority <= 0.7)
            {
                PriorityDescription = "Средний приоритет";
                PriorityColor = Brushes.Gold;
            }
            else if (OrderPriority <= 0.9)
            {
                PriorityDescription = "Высокий приоритет";
                PriorityColor = Brushes.OrangeRed;
            }
            else
            {
                PriorityDescription = "Наивысший приоритет";
                PriorityColor = Brushes.Red;
            }
        }
        private void RouteCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!CanAddNewOrder()) return;

            var clickPos = e.GetPosition(RouteCanvas);
            var newPoint = ConvertCanvasToCoordinates(clickPos);
            AddNewOrder(newPoint, OrderPrioritySlider.Value);
            UpdateRoute();
        }

        private void LoadSelectedDataset()
        {
            int datasetIndex = DatasetComboBox.SelectedIndex + 1;
            currentOrders = OrderArraysProvider.GetOrders(datasetIndex);
        }

        private void InitializeDepotAndOrderId()
        {
            try
            {
                depot = currentOrders.First(o => o.ID == -1).Destination;
                _nextOrderId = currentOrders.Max(o => o.ID) + 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Выберите набор данных");
            }
            
        }

        private BestDelivery.Point ConvertCanvasToCoordinates(System.Windows.Point clickPos)
        {
            var allPoints = currentOrders.Select(o => o.Destination).ToList();
            var bounds = CalculateBounds(allPoints);

            double canvasWidth = RouteCanvas.ActualWidth;
            double canvasHeight = RouteCanvas.ActualHeight;

            double x = (clickPos.X - 20) / (canvasWidth - 40) * (bounds.maxX - bounds.minX) + bounds.minX;
            double y = (canvasHeight - 20 - clickPos.Y) / (canvasHeight - 40) * (bounds.maxY - bounds.minY) + bounds.minY;

            return new BestDelivery.Point { X = x, Y = y };
        }

        private (double minX, double maxX, double minY, double maxY) CalculateBounds(List<BestDelivery.Point> points)
        {
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            double padding = 0.1 * (maxX - minX);
            return (minX - padding, maxX + padding, minY - padding, maxY + padding);
        }

        private void AddNewOrder(BestDelivery.Point point, double priority)
        {
            var newOrder = new Order
            {
                ID = _nextOrderId++,
                Destination = point,
                Priority = priority
            };

            currentOrders = currentOrders.Append(newOrder).ToArray();
        }

        private void UpdateRoute()
        {
            var routeResult = NearestNeighborAlgorithm.FindOptimalRoute(currentOrders);
            VisualizeRoute(routeResult);
            ShowRouteInfo(routeResult);
            UpdateRouteCost(routeResult.Route);
        }

        private void VisualizeRoute(RouteResult routeResult)
        {
            _routeVisualizer.VisualizeRoute(currentOrders, routeResult.Route, depot);
        }

        private void ShowRouteInfo(RouteResult routeResult)
        {
            var routeInfo = new List<string>();

            routeInfo.Add("Последовательность точек:");
            for (int i = 0; i < routeResult.Route.Length; i++)
            {
                string pointName = routeResult.Route[i] == -1 ? "Склад" : $"Заказ {routeResult.Route[i]}";
                routeInfo.Add($"{i + 1}. {pointName}");
            }

            routeInfo.Add("Детали сегментов:");
            foreach (var segment in routeResult.Segments)
            {
                routeInfo.Add(segment.ToString());
            }

            RouteListBox.ItemsSource = routeInfo;
        }

        private void UpdateRouteCost(int[] route)
        {
            if (RoutingTestLogic.TestRoutingSolution(depot, currentOrders.Where(o => o.ID != -1).ToArray(), route, out double routeCost))
            {
                RouteCostText.Text = $"Стоимость маршрута: {routeCost:F2}";
            }
            else
            {
                RouteCostText.Text = "Ошибка в маршруте";
            }
        }

        private bool CanAddNewOrder()
        {
            return currentOrders != null && !depot.Equals(default(BestDelivery.Point));
        }
    }

    public static class OrderArraysProvider
    {
        public static Order[] GetOrders(int datasetIndex)
        {
            return datasetIndex switch
            {
                1 => OrderArrays.GetOrderArray1(),
                2 => OrderArrays.GetOrderArray2(),
                3 => OrderArrays.GetOrderArray3(),
                4 => OrderArrays.GetOrderArray4(),
                5 => OrderArrays.GetOrderArray5(),
                6 => OrderArrays.GetOrderArray6(),
                _ => Array.Empty<Order>()
            };
        }
    }
}