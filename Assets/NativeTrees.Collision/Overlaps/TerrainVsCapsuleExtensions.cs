using Unity.Mathematics;
using UnityEngine;

namespace NativeTrees
{
    public static class TerrainVsCapsuleExtensions
    {
        public static OverlapResult Overlaps(this CapsuleCollider capsule, TerrainCollider terrain)
        {
            return terrain.Overlaps(capsule);
        }

        public static OverlapResult Overlaps(this TerrainCollider terrain, CapsuleCollider capsule)
        {
            // Получаем центры верхней и нижней сфер капсулы
            capsule.GetSphereCenters(out float3 topSphere, out float3 bottomSphere);

            // Проекция центра капсулы на плоскость террейна (плоский XZ)
            float terrainX = capsule.Center.x;
            float terrainZ = capsule.Center.z;

            // Получаем интерполированную высоту террейна в точке проекции
            float terrainHeight = terrain.GetInterpolatedHeight(terrainX, terrainZ);

            // Сравниваем высоту террейна с капсулой (проверяем, пересекает ли капсула террейн)
            float capsuleTopHeight = topSphere.y;
            float capsuleBottomHeight = bottomSphere.y;

            // Проверка пересечения капсулы с террейном по оси Y
            if (capsuleBottomHeight <= terrainHeight && capsuleTopHeight >= terrainHeight)
            {
                // Определяем нормаль (вверх по оси Y) и глубину проникновения
                float penetrationDepth = terrainHeight - capsuleBottomHeight;
                return new OverlapResult
                {
                    IsIntersecting = true,
                    Axis = math.up(),  // Нормаль вверх
                    Depth = penetrationDepth
                };
            }

            // Если капсула не пересекает террейн
            return new OverlapResult
            {
                IsIntersecting = false,
                Axis = float3.zero,
                Depth = 0
            };
        }
    }
}