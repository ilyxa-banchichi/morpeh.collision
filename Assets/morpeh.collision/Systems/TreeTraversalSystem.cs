using NativeTrees;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using Unity.Mathematics;

namespace Scellecs.Morpeh.Collision.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TreeTraversalSystem : LateUpdateSystem
    {
        private Filter _dynamicColliders;
        private Filter _octrees;
        
        private Stash<BoxColliderComponent> _colliderComponents;
        private Stash<OctreeComponent> _octreeComponents;

        public override void OnAwake()
        {
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
            var nativeFilter = _dynamicColliders.AsNative();

            foreach (var octree in _octrees)
            {
                ref var cOctree = ref _octreeComponents.Get(octree);
                var job = new Job()
                {
                    Colliders = nativeFilter,
                    ColliderComponents = _colliderComponents.AsNative(),
                    Octree = cOctree,
                };

                job.Schedule(nativeFilter.length, 64).Complete();
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
                if (collider.OverlapResult.Contains(e))
                    collider.OverlapResult.Remove(e);
            }
        }
    }
}