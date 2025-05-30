using BestDelivery;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CaseDelivery
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string PriorityDescription => _priorityManager.Description;
        public Brush PriorityColor => _priorityManager.Color;
        public double OrderPriority
        {
            get => _priorityManager.CurrentPriority;
            set => _priorityManager.SetPriority(value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly VisualDisplay _routeVisualizer;
        private readonly OrderManager _orderManager;
        private readonly PriorityManager _priorityManager;


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _routeVisualizer = new VisualDisplay(RouteCanvas);
            _orderManager = new OrderManager();
            _priorityManager = new PriorityManager(this);
        }

        public void OnPropertyChanged(string propertyName)//Обновляет PropertyChanged
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CalculateRoute_Click(object sender, RoutedEventArgs e)//Обработчик кнопки "Построить маршрут"
        {
            try
            {
                _orderManager.LoadOrders(DatasetComboBox.SelectedIndex + 1);
                UpdateRoute();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void RouteCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)//Обработчик клика по Canvas
        {
            if (!_orderManager.CanAddOrder) return;

            var clickPos = e.GetPosition(RouteCanvas);
            _orderManager.AddOrderFromClick(clickPos, OrderPriority, RouteCanvas.ActualWidth, RouteCanvas.ActualHeight);
            UpdateRoute();
        }

        private void UpdateRoute()//Обновляет маршрут
        {
            var routeResult = NearestNeighborAlgorithm.FindOptimalRoute(_orderManager.CurrentOrders);
            _routeVisualizer.VisualizeRoute(_orderManager.CurrentOrders, routeResult.Route, _orderManager.Depot);
            DisplayRouteInfo(routeResult);
        }

        private void DisplayRouteInfo(RouteResult routeResult)//Отображает информацию о маршруте
        {
            RouteListBox.ItemsSource = RouteResult.RouteInfoFormatter(routeResult);
            if (RoutingTestLogic.TestRoutingSolution(_orderManager.Depot, _orderManager.CurrentOrders.Where(o => o.ID != -1).ToArray(), routeResult.Route, out double routeCost))
            {
                RouteCostText.Text =$"Стоимость маршрута: {routeCost:F2}";
            }
            else RouteCostText.Text= "Ошибка в маршруте";
        }
    }
}