using System;
using Unity.Mathematics;

namespace NativeTrees
{
    public static class CapsuleVsBoxExtensions
    {
        public static OverlapResult Overlaps(this BoxCollider box, CapsuleCollider capsule)
        {
            return capsule.Overlaps(box);
        }
        
        public static OverlapResult Overlaps(this CapsuleCollider capsule, BoxCollider box)
        {
            capsule.GetSphereCenters(out float3 topSphere, out float3 bottomSphere);

            // Проверяем расстояние от обоих концов капсулы до бокса
            float3 closestToTop = ClosestPointOnOBB(box, topSphere);
            float3 closestToBottom = ClosestPointOnOBB(box, bottomSphere);

            // Вычисляем расстояния до ближайших точек
            float distanceToTop = math.distance(closestToTop, topSphere);
            float distanceToBottom = math.distance(closestToBottom, bottomSphere);

            // Если расстояние до любой из сфер меньше радиуса капсулы, есть пересечение
            if (distanceToTop <= capsule.Radius || distanceToBottom <= capsule.Radius)
            {
                // Определяем глубину проникновения и ось пересечения
                float penetrationDepth = capsule.Radius - math.min(distanceToTop, distanceToBottom);
                float3 intersectionPoint = distanceToTop < distanceToBottom ? closestToTop : closestToBottom;
                float3 axis = math.normalize(intersectionPoint - (distanceToTop < distanceToBottom ? topSphere : bottomSphere));

                return new OverlapResult
                {
                    IsIntersecting = true,
                    Axis = axis,
                    Depth = penetrationDepth
                };
            }

            // Если пересечений нет
            return new OverlapResult
            {
                IsIntersecting = false,
                Axis = float3.zero,
                Depth = 0
            };
        }
        
        private static float3 ClosestPointOnOBB(BoxCollider box, float3 point)
        {
            // Вычисляем вектор от центра бокса до точки
            float3 localPoint = point - box.Center;

            // Начинаем с центра бокса
            float3 closestPoint = box.Center;

            // Проверяем проекции на каждую из осей бокса
            Span<float3> axes = stackalloc float3[]{ box.X, box.Y, box.Z };
            float3 extents = box.Extents;

            for (int i = 0; i < 3; i++)
            {
                // Проекция точки на текущую ось
                float projection = math.dot(localPoint, axes[i]);

                // Ограничиваем проекцию пределами размера бокса
                projection = math.clamp(projection, -extents[i], extents[i]);

                // Добавляем к ближайшей точке
                closestPoint += projection * axes[i];
            }

            return closestPoint;
        }
    }
}