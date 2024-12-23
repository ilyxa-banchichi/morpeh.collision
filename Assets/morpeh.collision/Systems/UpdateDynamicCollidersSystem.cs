using NativeTrees;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Native;
using Scellecs.Morpeh.Transform.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;

namespace Scellecs.Morpeh.Collision.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateDynamicCollidersSystem : LateUpdateSystem
    {
        private Filter _dynamicColliders;
        
        private Stash<BoxColliderComponent> _colliderComponents;
        private Stash<TransformComponent> _transformComponents;
        
        public override void OnAwake()
        {
            _dynamicColliders = World.Filter
                .With<BoxColliderComponent>()
                .With<TransformComponent>()
                .Without<StaticColliderTag>()
                .Build();

            _colliderComponents = World.GetStash<BoxColliderComponent>();
            _transformComponents = World.GetStash<TransformComponent>();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            var dynamicCollidersNative = _dynamicColliders.AsNative();
            World.JobHandle = new Job()
            {
                Colliders = dynamicCollidersNative,
                ColliderComponents = _colliderComponents.AsNative(),
                TransformComponents = _transformComponents.AsNative()
            }.Schedule(dynamicCollidersNative.length, 64, World.JobHandle);
        }
        
        [BurstCompile]
        private struct Job : IJobParallelFor
        {
            [ReadOnly]
            public NativeFilter Colliders;
                        
            public NativeStash<BoxColliderComponent> ColliderComponents;
           
            [ReadOnly]
            public NativeStash<TransformComponent> TransformComponents;
            
            public void Execute(int index)
            {
                var entity = Colliders[index];
                ref var collider = ref ColliderComponents.Get(entity);
                ref var transform = ref TransformComponents.Get(entity);

                collider.WorldBounds = new OBB(
                    collider.OriginalBounds,
                    transform.Position(),
                    transform.Rotation()
                );
            }
        }
    }
}