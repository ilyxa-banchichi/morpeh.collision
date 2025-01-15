using System;

namespace NativeTrees
{
    public static unsafe class ColliderCastUtils
    {
        public static AABB ToAABB(void* collider, ColliderType type)
        {
            if (type == ColliderType.Box)
                return (AABB)(*ToBoxCollider(collider));

            if (type == ColliderType.Sphere)
                return (AABB)(*ToSphereCollider(collider));

            throw new InvalidCastException($"Cannot convert collider {type} to AABB");
        }
        
        public static BoxCollider* ToBoxCollider(void* collider)
        {
            return (BoxCollider*)collider;
        }
        
        public static SphereCollider* ToSphereCollider(void* collider)
        {
            return (SphereCollider*)collider;
        }
    }
}