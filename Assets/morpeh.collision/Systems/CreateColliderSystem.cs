using NativeTrees;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Collision.Requests;
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
        
        public override void OnAwake()
        {
            _requests = World.Filter.With<CreateBoxColliderRequest>().Build();

            _createBoxColliderRequests = World.GetStash<CreateBoxColliderRequest>();
            _boxColliderComponents = World.GetStash<BoxColliderComponent>();
            _staticColliderTags = World.GetStash<StaticColliderTag>();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var entity in _requests)
            {
                ref var request = ref _createBoxColliderRequests.Get(entity);
                ref var collider = ref _boxColliderComponents.Get(entity, out var hasComponent);
                if (!hasComponent)
                    collider = ref _boxColliderComponents.Add(entity);

                collider.Layer = request.Layer;
                collider.Weight = request.Weight;
                
                var fp = request.FreezePosition;
                collider.FreezePosition = new int3(fp.x ? 0 : 1, fp.y ? 0 : 1, fp.z ? 0: 1);

                var capacity = request.IsStatic ? 0 : 5;
                if (!collider.OverlapResult.IsCreated)
                    collider.OverlapResult = new NativeParallelHashSet<EntityHolder<Entity>>(capacity, Allocator.Persistent);

                var extents = request.Size * 0.5f;
                collider.OriginalBounds = new AABB(request.Center - extents, request.Center + extents);
                collider.WorldBounds = new OBB(
                    aabb: collider.OriginalBounds,
                    position: request.InitPosition,
                    rotation: request.InitRotation
                );

                if (request.IsStatic)
                    _staticColliderTags.Add(entity);
            }
        }
    }
}