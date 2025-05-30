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
