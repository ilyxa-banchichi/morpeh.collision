using System;
using NativeTrees;
using TriInspector;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine.Serialization;

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
        
         public int Layer;
         public NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>> OverlapResult;
         public int OverlapResultMultiThreadWriteFlag;
         public NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>> LastOverlapResult;

#if UNITY_EDITOR
        private ColliderType _type => WorldBounds.Type;
        
        [Title(nameof(BoxCollider))]
        [InlineProperty]
        [HideLabel]
        [ShowInInspector]
        [ShowIf(nameof(_type), ColliderType.Box)]
        private BoxCollider _box
        {
            get
            {
                if (WorldBounds.Type == ColliderType.Box)
                    return ColliderCastUtils.ToBoxColliderRef(WorldBounds);
                return default;
            }
        }
        
        [Title(nameof(SphereCollider))]
        [InlineProperty]
        [HideLabel]
        [ShowInInspector]
        [ShowIf(nameof(_type), ColliderType.Sphere)]
        private SphereCollider _sphere
        {
            get
            {
                if (WorldBounds.Type == ColliderType.Sphere)
                    return ColliderCastUtils.ToSphereColliderRef(WorldBounds);
                return default;
            }
        }

        [Title(nameof(CapsuleCollider))]
        [InlineProperty]
        [HideLabel]
        [ShowInInspector]
        [ShowIf(nameof(_type), ColliderType.Capsule)]
        private CapsuleCollider _capsule
        {
            get
            {
                if (WorldBounds.Type == ColliderType.Capsule)
                    return ColliderCastUtils.ToCapsuleColliderRef(WorldBounds);
                return default;
            }
        }
        
        [Title(nameof(TerrainCollider))]
        [InlineProperty]
        [HideLabel]
        [ShowInInspector]
        [ShowIf(nameof(_type), ColliderType.Terrain)]
        private TerrainCollider _terrain
        {
            get
            {
                if (WorldBounds.Type == ColliderType.Terrain)
                    return ColliderCastUtils.ToTerrainColliderRef(WorldBounds);
                return default;
            }
        }
        
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
        private int _overlapResultCapasity => OverlapResult.Capacity;
        
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
        
        [ShowInInspector]
        private int _lastOverlapResultCapasity => LastOverlapResult.Capacity;
#endif

        public void Dispose()
        {
            if (LastOverlapResult.IsCreated)
                LastOverlapResult.Dispose();
            
            if (OverlapResult.IsCreated)
                OverlapResult.Dispose();
            
            OriginalBounds.Dispose();
            WorldBounds.Dispose();
        }
    }
}