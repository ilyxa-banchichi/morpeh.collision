using System;
using Unity.Mathematics;
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
            Center = aabb.Center;
            Extents = aabb.Size * 0.5f * scale;

            float3x3 rotationMatrix = new float3x3(rotation);

            X = rotationMatrix.c0;
            Y = rotationMatrix.c1;
            Z = rotationMatrix.c2;
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