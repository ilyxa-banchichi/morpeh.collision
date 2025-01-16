using System;
using NativeTrees;
using TriInspector;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Scellecs.Morpeh.Collision.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct ColliderComponent : IComponent, IDisposable
    {
         public Collider OriginalBounds;
         public Collider WorldBounds;
         
         public float3 Center;
         public float3 Extents;
        
         public int Layer;
         public NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>> OverlapResult;
         public NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>> LastOverlapResult;

#if UNITY_EDITOR
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
            OverlapResult.Dispose();
            OriginalBounds.Dispose();
            WorldBounds.Dispose();
        }
    }
}