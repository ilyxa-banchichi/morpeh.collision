using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace NativeTrees
{
    public static unsafe class ColliderCastUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AABB ToAABB(Collider collider)
        {
            if (collider.Type == ColliderType.Box)
                return (AABB)(ToBoxColliderRef(collider));

            if (collider.Type == ColliderType.Sphere)
                return (AABB)(ToSphereColliderRef(collider));
            
            if (collider.Type == ColliderType.Capsule)
                return (AABB)(ToCapsuleColliderRef(collider));
            
            if (collider.Type == ColliderType.Terrain)
                return (AABB)(ToTerrainColliderRef(collider));

#if UNITY_EDITOR
            throw new InvalidCastException($"Cannot convert collider {collider.Type} ({(int)collider.Type}) to AABB");
#endif

            return default;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref BoxCollider ToBoxColliderRef(Collider collider)
        {
            return ref UnsafeUtility.AsRef<BoxCollider>(collider.Bounds);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref SphereCollider ToSphereColliderRef(Collider collider)
        {
            return ref UnsafeUtility.AsRef<SphereCollider>(collider.Bounds);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref CapsuleCollider ToCapsuleColliderRef(Collider collider)
        {
            return ref UnsafeUtility.AsRef<CapsuleCollider>(collider.Bounds);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TerrainCollider ToTerrainColliderRef(Collider collider)
        {
            return ref UnsafeUtility.AsRef<TerrainCollider>(collider.Bounds);
        }
    }
}