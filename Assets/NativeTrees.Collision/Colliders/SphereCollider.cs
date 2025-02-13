using System;
using Unity.Mathematics;

namespace NativeTrees
{
    public struct SphereCollider : IEquatable<SphereCollider>
    {
        public float3 Center;
        public float Radius;

        public SphereCollider(float3 center, float radius, float3 scale)
        {
            var maxScale = math.max(math.max(scale.x, scale.y), scale.z);
            Center = center;
            Radius = radius * maxScale;
        }

        public static explicit operator AABB(SphereCollider boxCollider)
        {
            return new AABB(boxCollider.Center - new float3(boxCollider.Radius), boxCollider.Center + new float3(boxCollider.Radius));
        }

        public bool Equals(SphereCollider other)
        {
            return Center.Equals(other.Center) && Radius.Equals(other.Radius);
        }

        public override bool Equals(object obj)
        {
            return obj is SphereCollider other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Center, Radius);
        }
    }
}