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
    public sealed class PushOutSystem : LateUpdateSystem
    {
        private Filter _dynamicRigidbodies;
        
        private Stash<BoxColliderComponent> _colliderComponents;
        private Stash<RigidbodyComponent> _rigidbodyComponents;
        
        public override void OnAwake()
        {
            _dynamicRigidbodies = World.Filter
                .With<RigidbodyComponent>()
                .With<BoxColliderComponent>()
                .Without<StaticColliderTag>()
                .Build();

            _colliderComponents = World.GetStash<BoxColliderComponent>();
            _rigidbodyComponents = World.GetStash<RigidbodyComponent>();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            var dynamicRigidbodiesNative = _dynamicRigidbodies.AsNative();
            World.JobHandle = new Job()
            {
                Colliders = dynamicRigidbodiesNative,
                ColliderComponents = _colliderComponents.AsNative(),
                RigidbodyComponents = _rigidbodyComponents.AsNative()
            }.Schedule(dynamicRigidbodiesNative.length, 64, World.JobHandle);
        }
        
        [BurstCompile]
        private struct Job : IJobParallelFor
        {
            [ReadOnly]
            public NativeFilter Colliders;
                        
            [ReadOnly]
            public NativeStash<BoxColliderComponent> ColliderComponents;
            
            public NativeStash<RigidbodyComponent> RigidbodyComponents;
            
            public void Execute(int index)
            {
                var entity = Colliders[index];
                ref var collider = ref ColliderComponents.Get(entity);
                ref var rigidbody = ref RigidbodyComponents.Get(entity);

                rigidbody.Delta = 0;
                foreach (var overlap in collider.OverlapResult)
                {
                    var other = overlap.Obj.Entity;
                    var o = overlap.Overlap;

                    if (!RigidbodyComponents.Has(other)) continue;
                    if (!ColliderComponents.Has(other)) continue;
                    
                    ref var otherRigidbody = ref RigidbodyComponents.Get(other);
                    if (rigidbody.Weight > otherRigidbody.Weight) continue;

                    ref var otherCollider = ref ColliderComponents.Get(other);
                    var vec = collider.WorldBounds.Center - otherCollider.WorldBounds.Center;
                    var dir = math.dot(vec, o.Axis);
                    rigidbody.Delta += dir * o.Axis * o.Depth;
                }
            }
        }
    }
}