using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Native;
using Scellecs.Morpeh.Transform.Components;
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
    public sealed class CollisionResolutionSystem : LateUpdateSystem
    {
        private Filter _dynamicRigidbodies;
        
        private Stash<ColliderComponent> _colliderComponents;
        private Stash<RigidbodyComponent> _rigidbodyComponents;
        private Stash<TransformComponent> _transformComponents;
        
        public override void OnAwake()
        {
            _dynamicRigidbodies = World.Filter
                .With<RigidbodyComponent>()
                .With<ColliderComponent>()
                .With<TransformComponent>()
                .Without<StaticColliderTag>()
                .Build();

            _colliderComponents = World.GetStash<ColliderComponent>();
            _rigidbodyComponents = World.GetStash<RigidbodyComponent>();
            _transformComponents = World.GetStash<TransformComponent>();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            var dynamicRigidbodiesNative = _dynamicRigidbodies.AsNative();
            World.JobHandle = new Job()
            {
                Colliders = dynamicRigidbodiesNative,
                ColliderComponents = _colliderComponents.AsNative(),
                RigidbodyComponents = _rigidbodyComponents.AsNative(),
                TransformComponents = _transformComponents.AsNative()
            }.Schedule(dynamicRigidbodiesNative.length, 64, World.JobHandle);
            
            World.JobHandle.Complete();
        }
        
        [BurstCompile]
        private struct Job : IJobParallelFor
        {
            [ReadOnly]
            public NativeFilter Colliders;
                        
            [ReadOnly]
            public NativeStash<ColliderComponent> ColliderComponents;
            
            [ReadOnly]
            public NativeStash<RigidbodyComponent> RigidbodyComponents;
            
            public NativeStash<TransformComponent> TransformComponents;

            public void Execute(int index)
            {
                var entity = Colliders[index];
                ref var collider = ref ColliderComponents.Get(entity);
                ref var rigidbody = ref RigidbodyComponents.Get(entity);
                ref var transform = ref TransformComponents.Get(entity);

                foreach (var overlap in collider.OverlapResult)
                {
                    var other = overlap.Obj.Entity;
                    var o = overlap.Overlap;

                    if (o.Depth < 0.001f) continue;
                    if (!RigidbodyComponents.Has(other)) continue;
                    if (!ColliderComponents.Has(other)) continue;
                    
                    ref var otherRigidbody = ref RigidbodyComponents.Get(other);
                    if (rigidbody.Weight > otherRigidbody.Weight) continue;

                    ref var otherCollider = ref ColliderComponents.Get(other);
                    var vec = collider.Center - otherCollider.Center;
                    var dir = math.sign(math.dot(vec, o.Axis));

                    o.Axis *= rigidbody.FreezePosition;
                    var delta = dir * o.Axis * o.Depth;
                    if (rigidbody.Weight == otherRigidbody.Weight)
                        delta *= .5f;
                    
                    transform.Translate(delta);
                }
            }
        }
    }
}