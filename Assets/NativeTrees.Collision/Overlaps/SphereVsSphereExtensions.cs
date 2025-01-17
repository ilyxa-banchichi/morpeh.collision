using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace NativeTrees
{
    public static class SphereVsSphereExtensions
    {
        public static OverlapResult Overlaps(this SphereCollider collider, SphereCollider other)
        {
            // Вектор между центрами сфер
            float3 delta = other.Center - collider.Center;

            // Квадрат расстояния между центрами
            float distanceSq = math.dot(delta, delta);

            // Сумма радиусов
            float radiusSum = collider.Radius + other.Radius;

            // Проверка на пересечение
            bool isIntersecting = distanceSq <= radiusSum * radiusSum;

            // Если нет пересечения, вернуть результат с IsIntersecting = false
            if (!isIntersecting)
            {
                return new OverlapResult
                {
                    IsIntersecting = false,
                    Axis = float3.zero,
                    Depth = 0
                };
            }

            // Вычисление глубины пересечения
            float distance = math.sqrt(distanceSq); // Реальное расстояние между центрами
            float depth = radiusSum - distance;

            // Нормализованный вектор направления пересечения (ось)
            float3 axis = math.normalizesafe(delta);

            return new OverlapResult
            {
                IsIntersecting = true,
                Axis = axis,
                Depth = depth
            };
        }
    }
}