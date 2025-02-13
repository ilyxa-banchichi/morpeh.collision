using System;
using NativeTrees;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Native;
using Scellecs.Morpeh.Transform.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using UnityEngine;
using BoxCollider = NativeTrees.BoxCollider;
using CapsuleCollider = NativeTrees.CapsuleCollider;
using SphereCollider = NativeTrees.SphereCollider;

namespace Scellecs.Morpeh.Collision.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateDynamicCollidersSystem : LateUpdateSystem
    {
        private Filter _dynamicColliders;
        
        private Stash<ColliderComponent> _colliderComponents;
        private Stash<TransformComponent> _transformComponents;
        
        public override void OnAwake()
        {
            _dynamicColliders = World.Filter
                .With<ColliderComponent>()
                .With<TransformComponent>()
                .Without<StaticColliderTag>()
                .Build();

            _colliderComponents = World.GetStash<ColliderComponent>();
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
            
            World.JobHandle.Complete();
        }
        
        [BurstCompile]
        private struct Job : IJobParallelFor
        {
            [ReadOnly]
            public NativeFilter Colliders;
                        
            public NativeStash<ColliderComponent> ColliderComponents;
           
            [ReadOnly]
            public NativeStash<TransformComponent> TransformComponents;
            
            public void Execute(int index)
            {
                var entity = Colliders[index];
                ref ColliderComponent collider = ref ColliderComponents.Get(entity);
                ref TransformComponent transform = ref TransformComponents.Get(entity);
                
                switch (collider.WorldBounds.Type)
                {
                    case ColliderType.Box:
                        ref var boxPtr = ref ColliderCastUtils.ToBoxColliderRef(collider.WorldBounds);
                        boxPtr = new BoxCollider(
                            ColliderCastUtils.ToAABB(collider.OriginalBounds),
                            transform.Position(),
                            transform.Rotation(),
                            transform.Scale()
                        );

                        collider.Center = boxPtr.Center;
                        
                        break;
                
                    case ColliderType.Sphere:
                        ref var spherePtr = ref ColliderCastUtils.ToSphereColliderRef(collider.WorldBounds);
                        ref var original = ref ColliderCastUtils.ToSphereColliderRef(collider.OriginalBounds);
                        spherePtr = new SphereCollider(
                            original.Center + transform.Position(),
                            original.Radius,
                            transform.Scale()
                        );

                        collider.Center = spherePtr.Center;
                        
                        break;
                    
                    case ColliderType.Capsule:
                        ref var capsule = ref ColliderCastUtils.ToCapsuleColliderRef(collider.WorldBounds);
                        ref var originalCapsule = ref ColliderCastUtils.ToCapsuleColliderRef(collider.OriginalBounds);
                        capsule = new CapsuleCollider(
                            originalCapsule.Center + transform.Position(), 
                            originalCapsule.Radius, originalCapsule.Height, transform.Rotation());

                        collider.Center = capsule.Center;
                        
                        break;
                    
                    case ColliderType.Terrain:
#if UNITY_EDITOR
                        throw new ArgumentException("Terrain collider cannot be dynamic");
#endif
                        break;
                };
            }
        }
    }
}