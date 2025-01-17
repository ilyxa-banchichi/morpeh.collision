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

        public CapsuleCollider(float3 center, float radius, float height, quaternion rotation)
        {
            Center = center;
            Radius = radius;
            Height = height;
            Rotation = rotation;
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
        
        public static explicit operator AABB(CapsuleCollider capsuleCollider)
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