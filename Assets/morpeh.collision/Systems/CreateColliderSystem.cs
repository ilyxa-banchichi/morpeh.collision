using System.Data;
using NativeTrees;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Collision.Requests;
using Scellecs.Morpeh.Transform.Components;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using BoxCollider = NativeTrees.BoxCollider;
using CapsuleCollider = NativeTrees.CapsuleCollider;
using SphereCollider = NativeTrees.SphereCollider;
using TerrainCollider = NativeTrees.TerrainCollider;

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
        private Stash<TriggerColliderTag> _triggerTags;
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
            _triggerTags = World.GetStash<TriggerColliderTag>();
            _rigidbodyComponents = World.GetStash<RigidbodyComponent>();
            _collisionEventsComponents = World.GetStash<CollisionEventsComponent>();
            _transformComponents = World.GetStash<TransformComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var entity in _requests)
            {
                ref CreateBoxColliderRequest request = ref _createBoxColliderRequests.Get(entity);
                
                if (request.Data.Type == ColliderType.None)
                {
                    Debug.LogException(new DataException($"CreateBoxColliderRequest is not valid on {entity.ToString()}. Collider type is None"));
                    continue;
                }
                
                AddColliderComponent(entity, request);
                AddCollisionEventsComponent(entity);
                
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
            ref ColliderComponent collider = ref _colliderComponents.Add(entity);
            ref TransformComponent transform = ref _transformComponents.Get(entity);
            collider.Layer = request.Layer;
            var capacity = 5;
            
            if (!collider.OverlapResult.IsCreated)
                collider.OverlapResult = new NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>>(capacity, Allocator.Persistent);
            
            if (!collider.LastOverlapResult.IsCreated)
                collider.LastOverlapResult = new NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>>(capacity, Allocator.Persistent);
            
            collider.OriginalBounds.Type = collider.WorldBounds.Type = request.Data.Type;
            switch (request.Data.Type)
            {
                case ColliderType.Box:
                    CreateBoxCollider(ref collider, request.Data.Center, request.Data.Size, transform);
                    break;
                
                case ColliderType.Sphere:
                    CreateSphereCollider(ref collider, request.Data.Center, request.Data.Radius, transform);
                    break;
                
                case ColliderType.Capsule:
                    CreateCapsuleCollider(ref collider, request.Data.Center, request.Data.Radius, request.Data.Height, transform);
                    break;
                
                case ColliderType.Terrain:
                    CreateTerrainCollider(ref collider, request.Data.TerrainData, transform);
                    break;
            };
            
            // if (request.ColliderObject)
            //     Object.Destroy(request.ColliderObject);
        }

        private void CreateBoxCollider(ref ColliderComponent collider, float3 center, float3 size,
            TransformComponent transform)
        {
            var extents = size * 0.5f;
            var min = center - extents;
            var max = center + extents;
            
            BoxCollider original = new BoxCollider(new AABB(min, max), quaternion.identity, 1f);
            BoxCollider* originalPtr = (BoxCollider*)UnsafeUtility.Malloc(sizeof(BoxCollider), 4, Allocator.Persistent);
            *originalPtr = original;

            collider.OriginalBounds.Bounds = originalPtr;

            BoxCollider world = new BoxCollider((AABB)original, transform.Position(), transform.Rotation(), transform.Scale());
            BoxCollider* worldPtr = (BoxCollider*)UnsafeUtility.Malloc(sizeof(BoxCollider), 4, Allocator.Persistent);
            *worldPtr = world;
                
            collider.WorldBounds.Bounds = worldPtr;
                
            collider.Center = worldPtr->Center;
        }
        
        private void CreateSphereCollider(ref ColliderComponent collider, float3 center, 
            float radius, TransformComponent transform)
        {
            SphereCollider original = new SphereCollider(center, radius, 1f);
            SphereCollider* originalPtr = (SphereCollider*)UnsafeUtility.Malloc(sizeof(SphereCollider), 4, Allocator.Persistent);
            *originalPtr = original;
            
            collider.OriginalBounds.Bounds = originalPtr;
            
            SphereCollider world = new SphereCollider(original.Center + transform.Position(), original.Radius, transform.Scale());
            SphereCollider* worldPtr = (SphereCollider*)UnsafeUtility.Malloc(sizeof(SphereCollider), 4, Allocator.Persistent);
            *worldPtr = world;
            
            collider.WorldBounds.Bounds = worldPtr;

            collider.Center = worldPtr->Center;
        }
        
        private void CreateCapsuleCollider(ref ColliderComponent collider, float3 center, 
            float radius, float height, TransformComponent transform)
        {
            CapsuleCollider original = new CapsuleCollider(center, radius, height, quaternion.identity);
            CapsuleCollider* originalPtr = (CapsuleCollider*)UnsafeUtility.Malloc(sizeof(CapsuleCollider), 4, Allocator.Persistent);
            *originalPtr = original;
            
            collider.OriginalBounds.Bounds = originalPtr;
            
            CapsuleCollider world = new CapsuleCollider(original.Center + transform.Position(), original.Radius, original.Height, transform.Rotation());
            CapsuleCollider* worldPtr = (CapsuleCollider*)UnsafeUtility.Malloc(sizeof(CapsuleCollider), 4, Allocator.Persistent);
            *worldPtr = world;
            
            collider.WorldBounds.Bounds = worldPtr;

            var aabb = ColliderCastUtils.ToAABB(collider.WorldBounds);
            collider.Center = aabb.Center;
        }
        
        private void CreateTerrainCollider(ref ColliderComponent collider, 
            TerrainData terrainData, TransformComponent transform)
        {
            TerrainCollider world = new TerrainCollider();
            world.Width = terrainData.heightmapResolution;
            world.Height = terrainData.heightmapResolution;
            world.ScaleX = terrainData.size.x / world.Width;
            world.ScaleZ = terrainData.size.z / world.Height;
            world.Translation = transform.Position();

            world.HeightMap = new NativeArray<float>(world.Width * world.Height, Allocator.Persistent);
            
            world.MinHeight = float.MaxValue;
            world.MaxHeight = float.MinValue;

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
        }
        
        private void AddCollisionEventsComponent(Entity entity)
        {
            ref CollisionEventsComponent events = ref _collisionEventsComponents.Add(entity);
            var capacity = 5;
            if (!events.OnCollisionEnter.IsCreated)
                events.OnCollisionEnter = new NativeParallelHashSet<EntityHolder<Entity>>(capacity, Allocator.Persistent);
            
            if (!events.OnCollisionStay.IsCreated)
                events.OnCollisionStay = new NativeParallelHashSet<EntityHolder<Entity>>(capacity, Allocator.Persistent);
            
            if (!events.OnCollisionExit.IsCreated)
                events.OnCollisionExit = new NativeParallelHashSet<EntityHolder<Entity>>(capacity, Allocator.Persistent);
        }
        
        private void AddRigidbodyComponent(Entity entity, CreateBoxColliderRequest request)
        {
            ref RigidbodyComponent rigidbody = ref _rigidbodyComponents.Add(entity);
            rigidbody.Weight = !request.IsStatic ? request.Weight : int.MaxValue;
            var fp = request.FreezePosition;
            rigidbody.FreezePosition = new int3(fp.x ? 0 : 1, fp.y ? 0 : 1, fp.z ? 0: 1);
        }
    }
}