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
    public struct BoxColliderComponent : IComponent, IDisposable
    {
         public AABB OriginalBounds;
         public OBB WorldBounds;
         public int Layer;
         public int Weight;
         public int3 FreezePosition;
         public NativeParallelHashSet<EntityHolder<Entity>> OverlapResult;

#if UNITY_EDITOR
        [ShowInInspector]
        private float3 _center => WorldBounds.Center;
        
        [ShowInInspector]
        private float3 _size => WorldBounds.Extents * 2f;
#endif

        public void Dispose()
        {
            OverlapResult.Dispose();
        }
    }
}