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
    public sealed class UpdateCollisionEventsSystem : LateUpdateSystem
    {
        private Filter _colliders;
        
        private Stash<BoxColliderComponent> _colliderComponents;
        private Stash<CollisionEventsComponent> _collisionEventsComponents;

        public override void OnAwake()
        {
            _colliders = World.Filter
                .With<BoxColliderComponent>()
                .With<CollisionEventsComponent>()
                .Build();
                
            _colliderComponents = World.GetStash<BoxColliderComponent>();
            _collisionEventsComponents = World.GetStash<CollisionEventsComponent>();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            var collidersNative = _colliders.AsNative();
            World.JobHandle = new  Job()
            {
                Colliders = collidersNative,
                ColliderComponents = _colliderComponents.AsNative(),
                CollisionEventsComponents = _collisionEventsComponents.AsNative()
            }.Schedule(collidersNative.length, 64, World.JobHandle);
            
            World.JobHandle.Complete();
        }
        
        [BurstCompile]
        private struct Job : IJobParallelFor
        {
            [ReadOnly]
            public NativeFilter Colliders;
            
            public NativeStash<BoxColliderComponent> ColliderComponents;
            public NativeStash<CollisionEventsComponent> CollisionEventsComponents;
            
            public void Execute(int index)
            {
                var entity = Colliders[index];
                ref var collider = ref ColliderComponents.Get(entity);
                ref var events = ref CollisionEventsComponents.Get(entity);
                
                events.OnCollisionEnter.Clear();
                events.OnCollisionStay.Clear();
                events.OnCollisionExit.Clear();

                foreach (var obj in collider.OverlapResult)
                {
                    if (!collider.LastOverlapResult.Contains(obj))
                    {
                        events.OnCollisionEnter.Add(obj);
                    }
                }
                
                foreach (var obj in collider.OverlapResult)
                {
                    if (collider.LastOverlapResult.Contains(obj))
                    {
                        events.OnCollisionStay.Add(obj);
                    }
                }
                
                foreach (var obj in collider.LastOverlapResult)
                {
                    if (!collider.OverlapResult.Contains(obj))
                    {
                        events.OnCollisionExit.Add(obj);
                    }
                }
                
                collider.LastOverlapResult.Clear();
                foreach (var obj in collider.OverlapResult)
                    collider.LastOverlapResult.Add(obj);
            }
        }
    }
}