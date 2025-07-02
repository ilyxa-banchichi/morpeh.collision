using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace NativeTrees
{
    public static unsafe class ColliderCastUtils
    {
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