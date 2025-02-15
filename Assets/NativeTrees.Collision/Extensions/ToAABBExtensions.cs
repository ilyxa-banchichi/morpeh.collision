using System;
using Unity.Mathematics;

namespace NativeTrees
{
    public static class ToAABBExtensions
    {
        public static AABB ToAABB(this BoxCollider boxCollider)
        {
            // Вычисляем абсолютные значения осей, умноженные на размеры
            float3 absX = math.abs(boxCollider.X) * boxCollider.Extents.x;
            float3 absY = math.abs(boxCollider.Y) * boxCollider.Extents.y;
            float3 absZ = math.abs(boxCollider.Z) * boxCollider.Extents.z;

            // Полный охват (радиус AABB)
            float3 radius = absX + absY + absZ;

            // Минимальная и максимальная точки AABB
            float3 min = boxCollider.Center - radius;
            float3 max = boxCollider.Center + radius;

            return new AABB(min, max);
        }
        
        public static AABB ToAABB(this CapsuleCollider capsuleCollider)
        {
            // Вычисляем центры верхней и нижней сфер капсулы
            capsuleCollider.GetSphereCenters(out float3 topSphere, out float3 bottomSphere);

            // Максимальное смещение по радиусу вдоль всех осей
            float3 radiusOffset = new float3(capsuleCollider.Radius, capsuleCollider.Radius, capsuleCollider.Radius);

            // Учитываем вращение: расширяем AABB для направления вращенной капсулы
            Span<float3> sphereExtremes = stackalloc float3[]
            {
                math.mul(capsuleCollider.Rotation, topSphere - capsuleCollider.Center),
                math.mul(capsuleCollider.Rotation, bottomSphere - capsuleCollider.Center)
            };

            // Вычисляем крайние точки
            float3 capsuleMin = math.min(sphereExtremes[0], sphereExtremes[1]) - radiusOffset + capsuleCollider.Center;
            float3 capsuleMax = math.max(sphereExtremes[0], sphereExtremes[1]) + radiusOffset + capsuleCollider.Center;

            return new AABB(capsuleMin, capsuleMax);
        }
        
        public static AABB ToAABB(this SphereCollider boxCollider)
        {
            return new AABB(boxCollider.Center - new float3(boxCollider.Radius), boxCollider.Center + new float3(boxCollider.Radius));
        }
        
        public static AABB ToAABB(this TerrainCollider terrainCollider)
        {
            float3 min = new float3(0, terrainCollider.MinHeight, 0);
            float3 max = new float3(terrainCollider.ScaleX * (terrainCollider.Width - 1), terrainCollider.MaxHeight, terrainCollider.ScaleZ * (terrainCollider.Height - 1));

            return new AABB(min + terrainCollider.Translation, max + terrainCollider.Translation);
        }
    }
}