using System;
using NativeTrees;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Collision.Requests;
using Scellecs.Morpeh.Transform.Components;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using BoxCollider = NativeTrees.BoxCollider;
using Object = UnityEngine.Object;
using SphereCollider = NativeTrees.SphereCollider;

namespace Scellecs.Morpeh.Collision.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed unsafe class CreateColliderSystem : LateUpdateSystem
    {
        private Filter _requests;

        private Stash<CreateBoxColliderRequest> _createBoxColliderRequests;
        private Stash<ColliderComponent> _colliderComponents;
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
            _colliderComponents = World.GetStash<ColliderComponent>();
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
                
                AddColliderComponent(entity, request);
                AddCollisionEventsComponent(entity, request);
                
                if (request.IsStatic)
                    _staticColliderTags.Add(entity);

                if (request.IsTrigger)
                    _triggerTags.Add(entity);
                else
                    AddRigidbodyComponent(entity, request);
            }
        }

        private void AddColliderComponent(Entity entity, CreateBoxColliderRequest request)
        {
            ref var collider = ref _colliderComponents.Add(entity);
            ref var transform = ref _transformComponents.Get(entity);
            collider.Layer = request.Layer;
            var capacity = request.IsStatic ? 5 : 5;
            
            if (!collider.OverlapResult.IsCreated)
                collider.OverlapResult = new NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>>(capacity, Allocator.Persistent);
            
            if (!collider.LastOverlapResult.IsCreated)
                collider.LastOverlapResult = new NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>>(capacity, Allocator.Persistent);

            collider.OriginalBounds.Type = collider.WorldBounds.Type = request.Type;
            switch (request.Type)
            {
                case ColliderType.Box:
                    CreateBoxCollider(ref collider, request, transform);
                    break;
                
                case ColliderType.Sphere:
                    CreateSphereCollider(ref collider, request, transform);
                    break;
                
                case ColliderType.Capsule:
                    throw new NotImplementedException();
                    break;
                
                case ColliderType.Terrain:
                    CreateTerrainCollider(ref collider, request, transform);
                    break;
            };
            
            //Object.Destroy(request.Collider);
        }

        private void CreateBoxCollider(ref ColliderComponent collider, 
            CreateBoxColliderRequest request, TransformComponent transform)
        {
            UnityEngine.BoxCollider boxCollider = request.Collider as UnityEngine.BoxCollider;
            var extents = boxCollider.size * 0.5f;

            BoxCollider original = new BoxCollider(new AABB(boxCollider.center - extents, boxCollider.center + extents), quaternion.identity);
            BoxCollider* originalPtr = (BoxCollider*)UnsafeUtility.Malloc(sizeof(BoxCollider), 4, Allocator.Persistent);
            *originalPtr = original;

            collider.OriginalBounds.Bounds = originalPtr;

            BoxCollider world = new BoxCollider((AABB)original, transform.Position(), transform.Rotation());
            BoxCollider* worldPtr = (BoxCollider*)UnsafeUtility.Malloc(sizeof(BoxCollider), 4, Allocator.Persistent);
            *worldPtr = world;
                
            collider.WorldBounds.Bounds = worldPtr;
                
            collider.Center = worldPtr->Center;
            collider.Extents = worldPtr->Extents;
        }
        
        private void CreateSphereCollider(ref ColliderComponent collider, 
            CreateBoxColliderRequest request, TransformComponent transform)
        {
            UnityEngine.SphereCollider sphereCollider = request.Collider as UnityEngine.SphereCollider;
            
            SphereCollider original = new SphereCollider(sphereCollider.center, sphereCollider.radius);
            SphereCollider* originalPtr = (SphereCollider*)UnsafeUtility.Malloc(sizeof(SphereCollider), 4, Allocator.Persistent);
            *originalPtr = original;
            
            collider.OriginalBounds.Bounds = originalPtr;
            
            SphereCollider world = new SphereCollider(original.Center + transform.Position(), original.Radius);
            SphereCollider* worldPtr = (SphereCollider*)UnsafeUtility.Malloc(sizeof(SphereCollider), 4, Allocator.Persistent);
            *worldPtr = world;
            
            collider.WorldBounds.Bounds = worldPtr;

            collider.Center = worldPtr->Center;
            collider.Extents = worldPtr->Radius;
        }
        
        private void CreateTerrainCollider(ref ColliderComponent collider, 
            CreateBoxColliderRequest request, TransformComponent transform)
        {
            var terrainCollider = request.Collider as UnityEngine.TerrainCollider;
            var terrainData = terrainCollider.terrainData;
            
            TerrainCollider world = new TerrainCollider();
            world.Width = terrainData.heightmapResolution;
            world.Height = terrainData.heightmapResolution;
            world.ScaleX = terrainData.size.x / world.Width;
            world.ScaleZ = terrainData.size.z / world.Height;
            world.Translation = transform.Position();

            world.HeightMap = new NativeArray<float>(world.Width * world.Height, Allocator.Persistent);
            
            world.MinHeight = float.MaxValue;
            world.MaxHeight = float.MinValue;

            // Заполняем высотную карту
            for (int z = 0; z < world.Height; z++)
            {
                for (int x = 0; x < world.Width; x++)
                {
                    var height = terrainData.GetHeight(x, z);
                    world.HeightMap[z * world.Width + x] = height;
                    world.MinHeight = math.min(height, world.MinHeight);
                    world.MaxHeight = math.max(height, world.MaxHeight);
                }
            }

            TerrainCollider* worldPtr = (TerrainCollider*)UnsafeUtility.Malloc(sizeof(TerrainCollider), 4, Allocator.Persistent);
            *worldPtr = world;
            
            collider.WorldBounds.Bounds = worldPtr;

            var aabb = ColliderCastUtils.ToAABB(collider.WorldBounds);
            collider.Center = aabb.Center;
            collider.Extents = aabb.Center * .5f;
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