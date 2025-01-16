using Unity.Mathematics;
using UnityEngine;

namespace NativeTrees
{
    public static class TerrainVsSphereExtensions
    {
        public static OverlapResult Overlaps(this SphereCollider sphere, TerrainCollider terrain)
        {
            return terrain.Overlaps(sphere);
        }
        
        public static OverlapResult Overlaps(this TerrainCollider terrain, SphereCollider sphere)
        {
            bool isIntersecting = false;
            float3 closestAxis = float3.zero;
            float maxDepth = 0;

            // Перебираем несколько точек по окружности сферы
            int sampleCount = 8;  // Можем увеличить количество проверок для точности
            float sumHeight = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                // Точка на окружности сферы
                float angle = math.radians(i * (360f / sampleCount));
                float x = sphere.Center.x + sphere.Radius * math.cos(angle);
                float z = sphere.Center.z + sphere.Radius * math.sin(angle);

                // Получаем высоту террейна в этой точке
                float height = terrain.GetInterpolatedHeight(x, z);
                sumHeight += height;

                // Проверяем, если точка ниже высоты террейна
                if (sphere.Center.y - sphere.Radius < height)
                    isIntersecting = true;
            }
            
            maxDepth = sumHeight / sampleCount + sphere.Radius - sphere.Center.y;
            closestAxis = math.up();
            
            return new OverlapResult
            {
                IsIntersecting = isIntersecting,
                Axis = closestAxis,
                Depth = maxDepth
            };
        }
    }
}