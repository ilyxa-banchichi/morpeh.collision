using Unity.Mathematics;

namespace NativeTrees
{
    public static class CapsuleVsSphereExtensions
    {
        public static OverlapResult Overlaps(this SphereCollider sphere, CapsuleCollider capsule)
        {
            return capsule.Overlaps(sphere);
        }
        
        public static OverlapResult Overlaps(this CapsuleCollider capsule, SphereCollider sphere)
        {
            // Получаем центры верхней и нижней сфер капсулы
            capsule.GetSphereCenters(out float3 topSphere, out float3 bottomSphere);

            // Находим ближайшую точку на капсуле к центру сферы
            float3 closestPoint = ClosestPointOnCapsule(topSphere, bottomSphere, sphere.Center);

            // Вычисляем расстояние между ближайшей точкой и центром сферы
            float distance = math.distance(closestPoint, sphere.Center);

            // Проверяем, пересекаются ли объекты
            float combinedRadius = capsule.Radius + sphere.Radius;

            if (distance <= combinedRadius)
            {
                // Глубина проникновения и нормаль
                float penetrationDepth = combinedRadius - distance;
                float3 axis = math.normalizesafe(sphere.Center - closestPoint);

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