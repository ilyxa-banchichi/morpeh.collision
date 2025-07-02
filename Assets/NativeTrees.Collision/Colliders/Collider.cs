using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace NativeTrees
{
    public unsafe struct Collider : IDisposable
    {
        public void* Bounds;
        public AABB AABB;
        public ColliderType Type;

        public void Dispose()
        {
            if (Type == ColliderType.Terrain && Bounds != null)
            {
                ref var terrain = ref ColliderCastUtils.ToTerrainColliderRef(this);
                    terrain.Dispose();
            }

            if (Bounds != null)
            {
                UnsafeUtility.Free(Bounds, Allocator.Persistent);
                Bounds = null;
            }

            AABB = default;
            Type = ColliderType.None;
        }
    }
}