using System;
using NativeTrees;
using TriInspector;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Scellecs.Morpeh.Collision.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public unsafe struct ColliderComponent : IComponent, IDisposable
    {
         public void* OriginalBounds;
         public void* WorldBounds;
         public float3 Center;
         public float3 Extents;
         public ColliderType Type;
         public int Layer;
         public NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>> OverlapResult;
         public NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>> LastOverlapResult;

#if UNITY_EDITOR
        // [ShowInInspector]
        // private float3 _center => WorldBounds.Center;
        //
        // [ShowInInspector]
        // private float3 _size => WorldBounds.Extents * 2f;

        [ShowInInspector]
        private OverlapHolder<EntityHolder<Entity>>[] _overlapEntities
        {
            get
            {
                var array = OverlapResult.ToNativeArray(Allocator.Temp);
                OverlapHolder<EntityHolder<Entity>>[] e = array.ToArray();
                array.Dispose();

                return e;
            }
        }
        
        [ShowInInspector]
        private OverlapHolder<EntityHolder<Entity>>[] _lastOverlapResult
        {
            get
            {
                var array = LastOverlapResult.ToNativeArray(Allocator.Temp);
                OverlapHolder<EntityHolder<Entity>>[] e = array.ToArray();
                array.Dispose();

                return e;
            }
        }
#endif

        public void Dispose()
        {
            if (Type == ColliderType.Terrain)
            {
                var terrain = ColliderCastUtils.ToTerrainCollider(WorldBounds);
                terrain->HeightMap.Dispose();
            }
            
            UnsafeUtility.Free(OriginalBounds, Allocator.Persistent);
            UnsafeUtility.Free(WorldBounds, Allocator.Persistent);
            OverlapResult.Dispose();
        }
    }
}