using System.Threading;
using NativeTrees;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using UnityEngine;

namespace Scellecs.Morpeh.Collision.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TreeTraversalSystem : LateUpdateSystem
    {
        private Filter _colliders;
        private Filter _dynamicColliders;
        private Filter _octrees;
        
        private Stash<ColliderComponent> _colliderComponents;
        private Stash<OctreeComponent> _octreeComponents;
        
        public override void OnAwake()
        {
            _colliders = World.Filter.With<ColliderComponent>().Build();
            _dynamicColliders = World.Filter
                .With<ColliderComponent>()
                .Without<StaticColliderTag>()
                .Build();

            _octrees = World.Filter.With<OctreeComponent>().Build();
                
            _colliderComponents = World.GetStash<ColliderComponent>();
            _octreeComponents = World.GetStash<OctreeComponent>();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            var collidersNative = _colliders.AsNative();
            var dynamicCollidersNative = _dynamicColliders.AsNative();

            World.JobHandle = new CleanUpJob()
            {
                Colliders = collidersNative,
                ColliderComponents = _colliderComponents.AsNative()
            }.Schedule(collidersNative.length, 64, World.JobHandle);
            
            World.JobHandle.Complete();

            var layerCollisionMasks = LayerUtils.GetMasksNative(Allocator.TempJob);
            foreach (var octree in _octrees)
            {
                ref var cOctree = ref _octreeComponents.Get(octree);
                World.JobHandle = new TreeTraversalJob()
                {
                    Colliders = dynamicCollidersNative,
                    ColliderComponents = _colliderComponents.AsNative(),
                    Octree = cOctree,
                    LayerCollisionMasks = layerCollisionMasks
                }.Schedule(dynamicCollidersNative.length, 4, World.JobHandle);
            }
            
            World.JobHandle.Complete();
            layerCollisionMasks.Dispose();
        }
        
        [BurstCompile]
        private struct CleanUpJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeFilter Colliders;
            
            public NativeStash<ColliderComponent> ColliderComponents;
            
            public void Execute(int index)
            {
                var entity = Colliders[index];
                ref var collider = ref ColliderComponents.Get(entity);
                collider.OverlapResult.Clear();
            }
        }
        
        [BurstCompile]
        private struct TreeTraversalJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeFilter Colliders;
            
            [ReadOnly]
            public OctreeComponent Octree;

            [ReadOnly]
            public NativeArray<int> LayerCollisionMasks;
            
            public NativeStash<ColliderComponent> ColliderComponents;
            
            public void Execute(int index)
            {
                var entity = Colliders[index];
                ref ColliderComponent collider = ref ColliderComponents.Get(entity);
                var overlapHolder = new OverlapHolder<EntityHolder<Entity>>()
                {
                    Obj = new EntityHolder<Entity>(entity, collider.Layer, collider.WorldBounds)
                };
                
                Octree.StaticColliders.RangeColliderUnique(collider.WorldBounds, collider.OverlapResult, LayerCollisionMasks[collider.Layer]);
                foreach (var o in collider.OverlapResult)
                {
                    ref ColliderComponent otherCollider = ref ColliderComponents.Get(o.Obj.Entity);
                    while (Interlocked.CompareExchange(ref otherCollider.OverlapResultMultiThreadWriteFlag, 1, 0) != 0) { }
                    otherCollider.OverlapResult.Add(overlapHolder);
                    Interlocked.Exchange(ref otherCollider.OverlapResultMultiThreadWriteFlag, 0);
                }
                Octree.DynamicColliders.RangeColliderUnique(collider.WorldBounds, collider.OverlapResult, LayerCollisionMasks[collider.Layer]);
                
                if (collider.OverlapResult.Contains(overlapHolder))
                    collider.OverlapResult.Remove(overlapHolder);
            }
        }
    }
}