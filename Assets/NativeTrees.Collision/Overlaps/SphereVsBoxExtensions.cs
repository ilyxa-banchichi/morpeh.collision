using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace NativeTrees
{
    public static class SphereVsBoxExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OverlapResult Overlaps(this BoxCollider box, SphereCollider sphere)
        {
            return sphere.Overlaps(box);
        }

        public static OverlapResult Overlaps(this SphereCollider sphere, BoxCollider box)
        {
            // Вектор от центра коробки к центру сферы
            float3 delta = sphere.Center - box.Center;

            // Находим ближайшую точку на коробке к центру сферы
            float3 closestPoint = box.Center;

            // Проекция delta на локальные оси коробки
            float3 projection = new float3(
                math.dot(delta, box.X),
                math.dot(delta, box.Y),
                math.dot(delta, box.Z)
            );

            // Ограничиваем проекцию по границам коробки
            projection.x = math.clamp(projection.x, -box.Extents.x, box.Extents.x);
            projection.y = math.clamp(projection.y, -box.Extents.y, box.Extents.y);
            projection.z = math.clamp(projection.z, -box.Extents.z, box.Extents.z);

            // Вычисляем ближайшую точку
            closestPoint += projection.x * box.X;
            closestPoint += projection.y * box.Y;
            closestPoint += projection.z * box.Z;

            // Вектор от центра сферы до ближайшей точки
            float3 sphereToClosest = closestPoint - sphere.Center;

            // Квадрат расстояния до ближайшей точки
            float distanceSq = math.dot(sphereToClosest, sphereToClosest);

            // Проверка пересечения
            bool isIntersecting = distanceSq <= sphere.Radius * sphere.Radius;

            // Если пересечения нет
            if (!isIntersecting)
            {
                return new OverlapResult
                {
                    IsIntersecting = false,
                    Axis = float3.zero,
                    Depth = 0
                };
            }

            // Глубина пересечения
            float distance = math.sqrt(distanceSq);
            float depth = sphere.Radius - distance;

            // Нормализованная ось пересечения
            float3 axis = math.normalizesafe(sphereToClosest);

            return new OverlapResult
            {
                IsIntersecting = true,
                Axis = axis,
                Depth = depth
            };
        }
    }
}