using System;
using System.Collections.Generic;
using System.Linq;
using BestDelivery;

namespace CaseDelivery
{
    public class NearestNeighborAlgorithm
    {
        public static RouteResult FindOptimalRoute(Order[] allPoints) //находит оптимальный путь
        {
            if (allPoints == null || allPoints.Length == 0)
                return new RouteResult(new int[] { -1 }, new List<RouteSegment>());

            var depot = allPoints.FirstOrDefault(o => o.ID == -1);
            if (depot.ID == 0)
                throw new ArgumentException("Массив заказов должен содержать склад с ID = -1");

            List<Point> points = allPoints.Select(o => o.Destination).ToList();
            double[,] distanceMatrix = BuildDistanceMatrix(points, allPoints);

            int[] route = BuildNearestNeighborRoute(allPoints, distanceMatrix);

            List<RouteSegment> routeSegments = CalculateRouteSegments(route, allPoints, distanceMatrix);

            return new RouteResult(route, routeSegments);
        }

        private static int[] BuildNearestNeighborRoute(Order[] allPoints, double[,] distanceMatrix)// Построение маршрута ближайшего соседа
        {
            List<int> route = new List<int> { -1 };
            List<int> unvisited = Enumerable.Range(0, allPoints.Length)
                                          .Where(i => allPoints[i].ID != -1)
                                          .ToList();

            int currentIndex = Array.FindIndex(allPoints, o => o.ID == -1);
            Point currentPoint = allPoints[currentIndex].Destination;

            while (unvisited.Count > 0)
            {
                int nearestIndex = FindNearestPoint(currentIndex, unvisited, distanceMatrix);
                int orderId = allPoints[nearestIndex].ID;

                route.Add(orderId);
                currentIndex = nearestIndex;
                currentPoint = allPoints[currentIndex].Destination;
                unvisited.Remove(currentIndex);
            }

            route.Add(-1);
            return route.ToArray();
        }

        private static int FindNearestPoint(int fromIndex, List<int> unvisitedIndices, double[,] distanceMatrix)//Нахождение ближайшей точки 
        {
            int nearestIndex = unvisitedIndices[0];
            double minDistance = distanceMatrix[fromIndex, nearestIndex];

            foreach (int index in unvisitedIndices)
            {
                double distance = distanceMatrix[fromIndex, index];
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = index;
                }
            }
            return nearestIndex;
        }

        private static List<RouteSegment> CalculateRouteSegments(int[] route, Order[] allPoints, double[,] distanceMatrix)// Рассчитывает сегменты
        {
            var segments = new List<RouteSegment>();

            for (int i = 0; i < route.Length - 1; i++)
            {
                int fromId = route[i];
                int toId = route[i + 1];

                int fromIndex = Array.FindIndex(allPoints, o => o.ID == fromId);
                int toIndex = Array.FindIndex(allPoints, o => o.ID == toId);

                double actualDistance = RoutingTestLogic.CalculateDistance(
                    allPoints[fromIndex].Destination,
                    allPoints[toIndex].Destination);

                double priority = toId == -1 ? 1.0 : allPoints[toIndex].Priority;
                double weightedDistance = actualDistance / priority;

                segments.Add(new RouteSegment(
                    fromId,
                    toId,
                    actualDistance,
                    weightedDistance,
                    priority));
            }

            return segments;
        }

        private static double[,] BuildDistanceMatrix(List<Point> points, Order[] allPoints) //строим матрицу весов
        {
            int n = points.Count;
            double[,] matrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = 0;
                    }
                    else
                    {
                        double distance = RoutingTestLogic.CalculateDistance(points[i], points[j]);
                        double priority = allPoints[j].Priority;
                        matrix[i, j] = distance / priority;
                    }
                }
            }

            return matrix;
        }
    }
}