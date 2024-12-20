using NativeTrees;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;

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
        
        private Stash<BoxColliderComponent> _colliderComponents;
        private Stash<OctreeComponent> _octreeComponents;

        public override void OnAwake()
        {
            _colliders = World.Filter.With<BoxColliderComponent>().Build();
            _dynamicColliders = World.Filter
                .With<BoxColliderComponent>()
                .Without<StaticColliderTag>()
                .Build();

            _octrees = World.Filter.With<OctreeComponent>().Build();
                
            _colliderComponents = World.GetStash<BoxColliderComponent>();
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

            foreach (var octree in _octrees)
            {
                ref var cOctree = ref _octreeComponents.Get(octree);
                World.JobHandle = new Job()
                {
                    Colliders = dynamicCollidersNative,
                    ColliderComponents = _colliderComponents.AsNative(),
                    Octree = cOctree,
                }.Schedule(dynamicCollidersNative.length, 64, World.JobHandle);
            }
        }
        
        [BurstCompile]
        private struct CleanUpJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeFilter Colliders;
            
            public NativeStash<BoxColliderComponent> ColliderComponents;
            
            public void Execute(int index)
            {
                var entity = Colliders[index];
                ref var collider = ref ColliderComponents.Get(entity);
                collider.OverlapResult.Clear();
            }
        }
        
        [BurstCompile]
        private struct Job : IJobParallelFor
        {
            [ReadOnly]
            public NativeFilter Colliders;
            
            [ReadOnly]
            public OctreeComponent Octree;
            
            public NativeStash<BoxColliderComponent> ColliderComponents;
            
            public void Execute(int index)
            {
                var entity = Colliders[index];
                ref var collider = ref ColliderComponents.Get(entity);
                
                collider.OverlapResult.Clear();
                Octree.DynamicRigidbodies.RangeOBBUnique(collider.WorldBounds, collider.OverlapResult);
                Octree.StaticRigidbodies.RangeOBBUnique(collider.WorldBounds, collider.OverlapResult);
                
                var e = new EntityHolder<Entity>(entity, collider.Layer, collider.WorldBounds);
                var o = new OverlapHolder<EntityHolder<Entity>>() { Obj = e };
                
                if (collider.OverlapResult.Contains(o))
                    collider.OverlapResult.Remove(o);
            }
        }
    }
}