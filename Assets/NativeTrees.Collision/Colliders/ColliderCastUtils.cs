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
            
            if (type == ColliderType.Terrain)
                return (AABB)(*ToTerrainCollider(collider));

            throw new InvalidCastException($"Cannot convert collider {type} ({(int)type}) to AABB");
        }
        
        public static BoxCollider* ToBoxCollider(void* collider)
        {
            return (BoxCollider*)collider;
        }
        
        public static SphereCollider* ToSphereCollider(void* collider)
        {
            return (SphereCollider*)collider;
        }
        
        public static TerrainCollider* ToTerrainCollider(void* collider)
        {
            return (TerrainCollider*)collider;
        }
    }
}