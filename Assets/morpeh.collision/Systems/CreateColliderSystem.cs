using NativeTrees;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Collision.Requests;
using Scellecs.Morpeh.Transform.Systems;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Scellecs.Morpeh.Collision.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CreateColliderSystem : LateUpdateSystem
    {
        private Filter _requests;

        private Stash<CreateBoxColliderRequest> _createBoxColliderRequests;
        private Stash<BoxColliderComponent> _boxColliderComponents;
        private Stash<StaticColliderTag> _staticColliderTags;
        private Stash<TriggerTag> _triggerTags;
        private Stash<RigidbodyComponent> _rigidbodyComponents;
        private Stash<CollisionEventsComponent> _collisionEventsComponents;
        private Stash<TransformComponent> _transformComponents;
        
        public override void OnAwake()
        {
            _requests = World.Filter
                .With<CreateBoxColliderRequest>()
                .With<TransformComponent>()
                .Build();

            _createBoxColliderRequests = World.GetStash<CreateBoxColliderRequest>();
            _boxColliderComponents = World.GetStash<BoxColliderComponent>();
            _staticColliderTags = World.GetStash<StaticColliderTag>();
            _triggerTags = World.GetStash<TriggerTag>();
            _rigidbodyComponents = World.GetStash<RigidbodyComponent>();
            _collisionEventsComponents = World.GetStash<CollisionEventsComponent>();
            _transformComponents = World.GetStash<TransformComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var entity in _requests)
            {
                ref var request = ref _createBoxColliderRequests.Get(entity);
                
                AddBoxColliderComponent(entity, request);
                AddCollisionEventsComponent(entity, request);
                
                if (request.IsStatic)
                    _staticColliderTags.Add(entity);

                if (request.IsTrigger)
                    _triggerTags.Add(entity);
                else
                    AddRigidbodyComponent(entity, request);
            }
        }

        private void AddBoxColliderComponent(Entity entity, CreateBoxColliderRequest request)
        {
            ref var collider = ref _boxColliderComponents.Add(entity);
            ref var transform = ref _transformComponents.Get(entity);
            collider.Layer = request.Layer;
            var capacity = request.IsStatic ? 5 : 5;
            
            if (!collider.OverlapResult.IsCreated)
                collider.OverlapResult = new NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>>(capacity, Allocator.Persistent);
            
            if (!collider.LastOverlapResult.IsCreated)
                collider.LastOverlapResult = new NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>>(capacity, Allocator.Persistent);
                
            var extents = request.Size * 0.5f;
            collider.OriginalBounds = new AABB(request.Center - extents, request.Center + extents);
            collider.WorldBounds = new OBB(
                aabb: collider.OriginalBounds,
                position: transform.Position(),
                rotation: transform.Rotation()
            );
        }
        
        private void AddCollisionEventsComponent(Entity entity, CreateBoxColliderRequest request)
        {
            ref var events = ref _collisionEventsComponents.Add(entity);
            var capacity = request.IsStatic ? 5 : 5;
            if (!events.OnCollisionEnter.IsCreated)
                events.OnCollisionEnter = new NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>>(capacity, Allocator.Persistent);
            
            if (!events.OnCollisionStay.IsCreated)
                events.OnCollisionStay = new NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>>(capacity, Allocator.Persistent);
            
            if (!events.OnCollisionExit.IsCreated)
                events.OnCollisionExit = new NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>>(capacity, Allocator.Persistent);
        }
        
        private void AddRigidbodyComponent(Entity entity, CreateBoxColliderRequest request)
        {
            ref var rigidbody = ref _rigidbodyComponents.Add(entity);
            rigidbody.Weight = !request.IsStatic ? request.Weight : int.MaxValue;
            var fp = request.FreezePosition;
            rigidbody.FreezePosition = new int3(fp.x ? 0 : 1, fp.y ? 0 : 1, fp.z ? 0: 1);
        }
    }
}