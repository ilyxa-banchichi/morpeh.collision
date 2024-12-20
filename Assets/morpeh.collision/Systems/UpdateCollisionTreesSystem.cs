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
    public sealed class UpdateCollisionTreesSystem : LateUpdateSystem
    {
        private const float sz = 500;
        private static readonly AABB WorldBounds = new AABB(new float3(-sz, -sz, -sz), new float3(sz, sz, sz));
        
        private Filter _dynamicColliders;
        private Filter _staticColliders;
        private Filter _octrees;

        private Stash<BoxColliderComponent> _colliderComponents;
        private Stash<OctreeComponent> _octreeComponents;

        public override void OnAwake()
        {
            _staticColliders = World.Filter
                .With<StaticColliderTag>()
                .With<BoxColliderComponent>()
                .Build();
            
            _dynamicColliders = World.Filter
                .With<BoxColliderComponent>()
                .Without<StaticColliderTag>()
                .Build();

            _octrees = World.Filter.With<OctreeComponent>().Build();

            _colliderComponents = World.GetStash<BoxColliderComponent>();
            _octreeComponents = World.GetStash<OctreeComponent>();

            var octree = World.CreateEntity();
            ref var component = ref _octreeComponents.Add(octree);
            component.DynamicRigidbodies = CreateEmptyTree(1);
            component.StaticRigidbodies = CreateEmptyTree(1);
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var octree in _octrees)
            {
                ref var cOctree = ref _octreeComponents.Get(octree);

                var staticCollidersCount = _staticColliders.GetLengthSlow();
                if (staticCollidersCount != cOctree.LastStaticRigidbodiesCount)
                {
                    cOctree.LastStaticRigidbodiesCount = staticCollidersCount;
                    cOctree.StaticRigidbodies.Dispose();
                    cOctree.StaticRigidbodies = BuildTree(_staticColliders.AsNative());
                }

                if (_dynamicColliders.IsNotEmpty())
                {
                    cOctree.DynamicRigidbodies.Dispose();
                    cOctree.DynamicRigidbodies = BuildTree(_dynamicColliders.AsNative());
                }
            }
        }

        private NativeOctree<EntityHolder<Entity>> BuildTree(NativeFilter filter)
        {
            var octree = CreateEmptyTree((int)math.ceil(math.sqrt(filter.length)));
            var job = new PopulateJob()
            {
                Colliders = filter,
                Octree = octree,
                ColliderComponents = _colliderComponents.AsNative(),
            };

            var handle = job.Schedule();
            World.JobHandle = JobHandle.CombineDependencies(World.JobHandle, handle);

            return octree;
        }

        private NativeOctree<EntityHolder<Entity>> CreateEmptyTree(int objectsPerNode)
        {
            return new NativeOctree<EntityHolder<Entity>>(
                WorldBounds,
                objectsPerNode,
                10,
                Allocator.Persistent);
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct PopulateJob : IJob
        {
            public NativeFilter Colliders;
            public NativeOctree<EntityHolder<Entity>> Octree;

            [ReadOnly]
            public NativeStash<BoxColliderComponent> ColliderComponents;

            public void Execute()
            {
                for (int i = 0; i < Colliders.length; i++)
                {
                    var entity = Colliders[i];
                    ref var collider = ref ColliderComponents.Get(entity);
                    
                    var entityHolder = new EntityHolder<Entity>(entity, collider.Layer, collider.WorldBounds);
                    Octree.Insert(entityHolder, (AABB)collider.WorldBounds);
                }
            }
        }
    }
}
