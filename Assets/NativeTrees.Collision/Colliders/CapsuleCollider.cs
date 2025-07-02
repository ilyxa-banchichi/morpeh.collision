using System;
using Unity.Mathematics;

namespace NativeTrees
{
    public struct CapsuleCollider : IEquatable<CapsuleCollider>
    {
        public float3 Center;   // Центр капсулы
        public float Radius;    // Радиус капсулы
        public float Height;    // Высота капсулы (расстояние между центрами сфер на концах)
        public quaternion Rotation;

        public CapsuleCollider(float3 center, float radius, float height, quaternion orientation)
        {
            Center = center;
            Radius = radius;
            Height = height;
            Rotation = orientation;
        }
        
        /// <summary>
        /// Возвращает позиции центров верхней и нижней сфер капсулы с учетом вращения.
        /// </summary>
        public void GetSphereCenters(out float3 topSphere, out float3 bottomSphere)
        {
            // Вычисляем направление по оси капсулы с учетом её вращения
            float3 up = math.mul(Rotation, new float3(0, 1, 0)); // Локальная ось Y
            float halfHeight = math.max(0, (Height / 2) + Radius); // Половина высоты между сферами

            // Определяем центры сфер
            topSphere = Center + up * halfHeight;
            bottomSphere = Center - up * halfHeight;
        }

        public bool Equals(CapsuleCollider other)
        {
            return Center.Equals(other.Center) && Radius.Equals(other.Radius) && Height.Equals(other.Height);
        }

        public override bool Equals(object obj)
        {
            return obj is CapsuleCollider other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Center, Radius, Height);
        }
    }
}