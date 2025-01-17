using Unity.Mathematics;

namespace NativeTrees
{
    public static class CapsuleVsCapsuleExtensions
    {
        public static OverlapResult Overlaps(this CapsuleCollider collider, CapsuleCollider other)
        {
            // Получаем центры верхней и нижней сфер для обеих капсул
            collider.GetSphereCenters(out float3 topSphere1, out float3 bottomSphere1);
            other.GetSphereCenters(out float3 topSphere2, out float3 bottomSphere2);

            // Вычисляем минимальное расстояние между капсулами
            float3 closestPoint1 = ClosestPointOnCapsule(topSphere1, bottomSphere1, topSphere2);
            float3 closestPoint2 = ClosestPointOnCapsule(topSphere1, bottomSphere1, bottomSphere2);
            float3 closestPoint3 = ClosestPointOnCapsule(topSphere2, bottomSphere2, topSphere1);
            float3 closestPoint4 = ClosestPointOnCapsule(topSphere2, bottomSphere2, bottomSphere1);

            // Находим минимальное расстояние между ближайшими точками
            float distance = math.min(
                math.distance(closestPoint1, topSphere2),
                math.min(math.distance(closestPoint2, bottomSphere2),
                    math.min(math.distance(closestPoint3, topSphere1),
                        math.distance(closestPoint4, bottomSphere1)))
            );

            // Проверяем, пересекаются ли капсулы (сумма радиусов + высота)
            float combinedRadius = collider.Radius + other.Radius;
            if (distance <= combinedRadius)
            {
                // Глубина проникновения и нормаль
                float penetrationDepth = combinedRadius - distance;
                float3 axis = math.normalize(closestPoint1 - closestPoint2); // Нормаль к поверхности пересечения

                return new OverlapResult
                {
                    IsIntersecting = true,
                    Axis = axis,
                    Depth = penetrationDepth
                };
            }

            // Если пересечения нет
            return new OverlapResult
            {
                IsIntersecting = false,
                Axis = float3.zero,
                Depth = 0
            };
        }
        
        private static float3 ClosestPointOnCapsule(float3 topSphere, float3 bottomSphere, float3 point)
        {
            // Вычисляем вектор от нижней сферы к верхней
            float3 capsuleAxis = topSphere - bottomSphere;
            float capsuleLength = math.length(capsuleAxis);
            float3 capsuleDirection = capsuleAxis / capsuleLength;

            // Вектор от нижней сферы к точке
            float3 toPoint = point - bottomSphere;

            // Проекция вектора на ось капсулы
            float projection = math.dot(toPoint, capsuleDirection);

            // Ограничиваем проекцию пределами капсулы
            projection = math.clamp(projection, 0, capsuleLength);

            // Находим ближайшую точку
            return bottomSphere + capsuleDirection * projection;
        }
    }
}