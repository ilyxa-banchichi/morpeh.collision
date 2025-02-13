using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

namespace NativeTrees
{
    public struct BoxCollider : IEquatable<BoxCollider>
    {
        public float3 Center;
        public float3 Extents;
        public float3 X;
        public float3 Y;
        public float3 Z;
        
        public BoxCollider(AABB aabb, float3 position, quaternion rotation, float3 scale) 
            : this(new AABB(aabb.min + position, aabb.max + position), rotation, scale) { }

        public BoxCollider(AABB aabb, quaternion rotation, float3 scale)
        {
            // Центр OBB совпадает с центром AABB
            Center = aabb.Center;

            // Половина размеров AABB (Extents)
            Extents = aabb.Size * 0.5f * scale;

            // Локальные оси AABB преобразуются с учетом вращения
            float3x3 rotationMatrix = new float3x3(rotation);

            X = rotationMatrix.c0; // Локальная ось X
            Y = rotationMatrix.c1; // Локальная ось Y
            Z = rotationMatrix.c2; // Локальная ось Z
        }
        
        public static explicit operator AABB(BoxCollider boxCollider)
        {
            // Вычисляем абсолютные значения осей, умноженные на размеры
            float3 absX = abs(boxCollider.X) * boxCollider.Extents.x;
            float3 absY = abs(boxCollider.Y) * boxCollider.Extents.y;
            float3 absZ = abs(boxCollider.Z) * boxCollider.Extents.z;

            // Полный охват (радиус AABB)
            float3 radius = absX + absY + absZ;

            // Минимальная и максимальная точки AABB
            float3 min = boxCollider.Center - radius;
            float3 max = boxCollider.Center + radius;

            return new AABB(min, max);
        }

        public bool Equals(BoxCollider other)
        {
            return Center.Equals(other.Center) && Extents.Equals(other.Extents) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is BoxCollider other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Center, Extents, X, Y, Z);
        }
    }
}