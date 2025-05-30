using BestDelivery;

namespace CaseDelivery
{
    public class OrderManager
    {
        private int _nextOrderId = 1;
        private Order[] _currentOrders;
        private BestDelivery.Point _depot;

        public Order[] CurrentOrders => _currentOrders;
        public BestDelivery.Point Depot => _depot;
        public bool CanAddOrder => _currentOrders != null && !_depot.Equals(default(BestDelivery.Point));

        public void LoadOrders(int datasetIndex)
        {
            _currentOrders = OrderManager.GetOrders(datasetIndex);
            _depot = _currentOrders.First(o => o.ID == -1).Destination;
            _nextOrderId = _currentOrders.Max(o => o.ID) + 1;
        }

        public void AddOrderFromClick(System.Windows.Point clickPos, double priority, double canvasWidth, double canvasHeight)//Добавляет заказ по координатам
        {
            var newPoint = VisualDisplay.ConvertCanvasToCoordinates(clickPos, _currentOrders, canvasWidth, canvasHeight);
            AddOrder(newPoint, priority);
        }

        public void AddOrder(BestDelivery.Point point, double priority)//добавляет заказ в массив
        {
            var newOrder = new Order
            {
                ID = _nextOrderId++,
                Destination = point,
                Priority = priority
            };

            _currentOrders = _currentOrders.Append(newOrder).ToArray();
        }
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
                _ => throw new ArgumentException("Неверный индекс набора данных")
            };
        }
    }
}
