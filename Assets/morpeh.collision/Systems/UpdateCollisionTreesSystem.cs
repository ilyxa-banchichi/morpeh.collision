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
        private static readonly AABB WorldBounds = new AABB(new float3(0f, -100, 0), new float3(250, 250, 250));
        
        private Filter _dynamicColliders;
        private Filter _staticColliders;
        private Filter _octrees;

        private Stash<ColliderComponent> _colliderComponents;
        private Stash<OctreeComponent> _octreeComponents;

        public override void OnAwake()
        {
            _staticColliders = World.Filter
                .With<StaticColliderTag>()
                .With<ColliderComponent>()
                .Build();
            
            _dynamicColliders = World.Filter
                .With<ColliderComponent>()
                .Without<StaticColliderTag>()
                .Build();

            _octrees = World.Filter.With<OctreeComponent>().Build();

            _colliderComponents = World.GetStash<ColliderComponent>();
            _octreeComponents = World.GetStash<OctreeComponent>();

            var octree = World.CreateEntity();
            ref var component = ref _octreeComponents.Add(octree);
            component.DynamicColliders = CreateEmptyTree(1);
            component.StaticColliders = CreateEmptyTree(1);
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var octree in _octrees)
            {
                ref OctreeComponent cOctree = ref _octreeComponents.Get(octree);

                var staticCollidersCount = _staticColliders.GetLengthSlow();
                if (staticCollidersCount != cOctree.LastStaticCollidersCount)
                {
                    cOctree.LastStaticCollidersCount = staticCollidersCount;
                    cOctree.StaticColliders.Dispose();
                    cOctree.StaticColliders = BuildTree(_staticColliders.AsNative());
                }

                if (_dynamicColliders.IsNotEmpty())
                {
                    cOctree.DynamicColliders.Dispose();
                    cOctree.DynamicColliders = BuildTree(_dynamicColliders.AsNative());
                }
            }
            
            World.JobHandle.Complete();
        }

        public override void Dispose()
        {
            foreach (var octree in _octrees)
            {
                World.RemoveEntity(octree);
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
        private unsafe struct PopulateJob : IJob
        {
            public NativeFilter Colliders;
            public NativeOctree<EntityHolder<Entity>> Octree;

            [ReadOnly]
            public NativeStash<ColliderComponent> ColliderComponents;

            public void Execute()
            {
                for (int i = 0; i < Colliders.length; i++)
                {
                    var entity = Colliders[i];
                    ref ColliderComponent collider = ref ColliderComponents.Get(entity);
                    
                    var entityHolder = new EntityHolder<Entity>(entity, collider.Layer, collider.WorldBounds);
                    Octree.Insert(entityHolder, collider.WorldBounds.AABB);
                }
            }
        }
    }
}
