using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace NativeTrees
{
    public unsafe struct Collider : IDisposable
    {
        public void* Bounds;
        public ColliderType Type;

        public void Dispose()
        {
            if (Type == ColliderType.Terrain && Bounds != null)
            {
                ref var terrain = ref ColliderCastUtils.ToTerrainColliderRef(this);
                if (terrain.HeightMap.IsCreated)
                    terrain.HeightMap.Dispose();
            }
            
            UnsafeUtility.Free(Bounds, Allocator.Persistent);
        }
    }
}