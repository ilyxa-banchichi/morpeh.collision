using System;
using Unity.Mathematics;

namespace NativeTrees
{
    public static class TerrainVsBoxExtensions
    {
        public static OverlapResult Overlaps(this BoxCollider box, TerrainCollider terrain)
        {
            return terrain.Overlaps(box);
        }
        
        public static OverlapResult Overlaps(this TerrainCollider terrain, BoxCollider box)
        {
            bool isIntersecting = false;
            float3 closestAxis = float3.zero;
            float maxDepth = 0;
            
            Span<float3> corners = stackalloc float3[8];
            corners[0] = box.Center + new float3(-box.Extents.x, -box.Extents.y, -box.Extents.z);
            corners[1] = box.Center + new float3( box.Extents.x, -box.Extents.y, -box.Extents.z);
            corners[2] = box.Center + new float3(-box.Extents.x, -box.Extents.y,  box.Extents.z);
            corners[3] = box.Center + new float3( box.Extents.x, -box.Extents.y,  box.Extents.z);
            corners[4] = box.Center + new float3(-box.Extents.x,  box.Extents.y, -box.Extents.z);
            corners[5] = box.Center + new float3( box.Extents.x,  box.Extents.y, -box.Extents.z);
            corners[6] = box.Center + new float3(-box.Extents.x,  box.Extents.y,  box.Extents.z);
            corners[7] = box.Center + new float3( box.Extents.x,  box.Extents.y,  box.Extents.z);

            // Перебираем все углы коробки
            for (int i = 0; i < 8; i++)
            {
                // Получаем координаты текущего угла коробки
                float3 corner = corners[i];

                // Получаем высоту террейна в точке (X, Z)
                float height = terrain.GetInterpolatedHeight(corner.x, corner.z);

                // Проверяем, если точка ниже высоты террейна
                if (corner.y < height)
                {
                    isIntersecting = true;
                    float depth = height - corner.y;
                    if (depth > maxDepth)
                    {
                        maxDepth = depth;
                    }
                }
            }

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