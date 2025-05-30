using BestDelivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseDelivery
{
    public class RouteResult//Используем для итогового маршрута
    {
        public int[] Route { get; }
        public List<RouteSegment> Segments { get; }

        public RouteResult(int[] route, List<RouteSegment> segments)
        {
            Route = route;
            Segments = segments;
        }
        public static List<string> RouteInfoFormatter(RouteResult routeResult)//Для UI
        {
            var routeInfo = new List<string>
            {
                "Последовательность точек:"
            };

            for (int i = 0; i < routeResult.Route.Length; i++)
            {
                string pointName = routeResult.Route[i] == -1 ? "Склад" : $"Заказ {routeResult.Route[i]}";
                routeInfo.Add($"{i + 1}. {pointName}");
            }

            routeInfo.Add("Детали сегментов:");
            routeInfo.AddRange(routeResult.Segments.Select(segment => segment.ToString()));

            return routeInfo;
        }
    }

    public class RouteSegment//Детали одного отрезка маршрута
    {
        public int FromId { get; }
        public int ToId { get; }
        public double Distance { get; }
        public double WeightedDistance { get; }
        public double Priority { get; }

        public RouteSegment(int fromId, int toId, double distance, double weightedDistance, double priority)
        {
            FromId = fromId;
            ToId = toId;
            Distance = distance;
            WeightedDistance = weightedDistance;
            Priority = priority;
        }

        public override string ToString()
        {
            return $"{FromId} -> {ToId}: {Distance:F2} (приоритет: {Priority:F2}, взвеш. расстояние: {WeightedDistance:F2})";
        }
    }
}
