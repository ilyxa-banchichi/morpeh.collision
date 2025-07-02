using NativeTrees;
using Scellecs.Morpeh.Collision.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using BoxCollider = NativeTrees.BoxCollider;
using Collider = NativeTrees.Collider;
using SphereCollider = NativeTrees.SphereCollider;

namespace Scellecs.Morpeh.Collision
{
    public unsafe class CollisionService : ICollisionService
    {
        private World _defaultWorld;
        private Filter _octree;
        private Stash<OctreeComponent> _octreeComponents;

        private NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>> _overlapResult;
        private SphereCollider* _sphereQueryPtr;
        private BoxCollider* _boxQueryPtr;

        public CollisionService()
        {
            _sphereQueryPtr = (SphereCollider*)UnsafeUtility.Malloc(sizeof(SphereCollider), 4, Allocator.Persistent);
            _boxQueryPtr = (BoxCollider*)UnsafeUtility.Malloc(sizeof(BoxCollider), 4, Allocator.Persistent);
            _overlapResult = new NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>>(5, Allocator.Persistent);
        }
        
        public void Dispose()
        {
            if (_overlapResult.IsCreated)
                _overlapResult.Dispose();

            if (_sphereQueryPtr != null)
            {
                UnsafeUtility.Free(_sphereQueryPtr, Allocator.Persistent);
                _sphereQueryPtr = null;
            }

            if (_boxQueryPtr != null)
            {
                UnsafeUtility.Free(_boxQueryPtr, Allocator.Persistent);
                _boxQueryPtr = null;
            }
        }
        
        public void Initialize(World world)
        {
            _defaultWorld = world;
            _octree = _defaultWorld.Filter.With<OctreeComponent>().Build();
            _octreeComponents = _defaultWorld.GetStash<OctreeComponent>();
        }
        
        public bool TryRaycastFromScreenPoint(
            Camera camera, 
            Vector2 position, 
            LayerMask mask, 
            out RaycastHit hit,
            float distance = 1000f)
        {
            var ray = camera.ScreenPointToRay(position);
#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.magenta);
#endif
            if (Raycast(ray, out hit, distance, mask))
                return true;

            hit = default;
            return false;
        }

        public bool Raycast(
            Ray ray, out RaycastHit hitInfo,
            float maxDistance = float.PositiveInfinity, int layerMask = ~0)
        {
            hitInfo = default;
            foreach (var entity in _octree)
            {
                ref OctreeComponent octree = ref _octreeComponents.Get(entity);
                return Raycast(octree, ray, out hitInfo, maxDistance, layerMask);
            }

            return false;
        }

        public static bool Raycast(
            OctreeComponent octree, Ray ray, out RaycastHit hitInfo,
            float maxDistance = float.PositiveInfinity, int layerMask = ~0)
        {
            hitInfo = new RaycastHit();

            var staticResult = octree.StaticColliders.RaycastOBB(
                ray,
                out var hitStatic,
                maxDistance,
                layerMask);

            var dynamicResult = octree.DynamicColliders.RaycastOBB(
                ray,
                out var hitDynamic,
                maxDistance,
                layerMask);

            OctreeRaycastHit<EntityHolder<Entity>> finalHit;
            float distance;

            if (!staticResult && !dynamicResult)
                return false;

            if (staticResult && !dynamicResult)
            {
                distance = math.distance(ray.origin, hitStatic.point);
                finalHit = hitStatic;
            }
            else if (!staticResult)
            {
                distance = math.distance(ray.origin, hitDynamic.point);
                finalHit = hitDynamic;
            }
            else
            {
                var distanceStatic = math.distance(ray.origin, hitStatic.point);
                var distanceDynamic = math.distance(ray.origin, hitDynamic.point);

                if (distanceDynamic < distanceStatic)
                {
                    distance = distanceDynamic;
                    finalHit = hitDynamic;
                }
                else
                {
                    distance = distanceStatic;
                    finalHit = hitStatic;
                }

            }

            hitInfo = new RaycastHit()
            {
                Entity = finalHit.obj.Entity,
                Layer = finalHit.obj.Layer,
                Collider = finalHit.obj.Collider,
                Point = finalHit.point,
                Distance = distance,
            };

            return true;
        }

        public int OverlapSphereNonAlloc(
            float3 position, float range, NativeArray<EntityHolder<Entity>> colliders, int layerMask = ~0)
        {
            SphereCollider sphereQuery = new SphereCollider(position, range, 1f);
            *_sphereQueryPtr = sphereQuery;
            
            var query = new Collider()
            {
                AABB = sphereQuery.ToAABB(),
                Bounds = _sphereQueryPtr,
                Type = ColliderType.Sphere,
            };

            return OverlapBoundsNonAlloc(query, colliders, layerMask);
        }
        
        public int OverlapBoxNonAlloc(float3 position, float3 extents, quaternion orientation,
            NativeArray<EntityHolder<Entity>> colliders, int layerMask = ~0)
        {
            float3 min = position - extents;
            float3 max = position + extents;
            BoxCollider boxQuery = new BoxCollider(new AABB(min, max), orientation, 1f);
            *_boxQueryPtr = boxQuery;
            
            var query = new Collider()
            {
                AABB = boxQuery.ToAABB(),
                Bounds = _boxQueryPtr,
                Type = ColliderType.Box,
            };

            return OverlapBoundsNonAlloc(query, colliders, layerMask);
        }
        
        private int OverlapBoundsNonAlloc(Collider query, 
            NativeArray<EntityHolder<Entity>> colliders, int layerMask = ~0)
        {
            foreach (var entity in _octree)
            {
                ref OctreeComponent octree = ref _octreeComponents.Get(entity);
                var job = new OverlapQueryJob()
                {
                    Octree = octree,
                    Query = query,
                    OverlapResult = _overlapResult,
                    LayerMask = layerMask,
                    Colliders = colliders,
                };
                
                job.Schedule().Complete();

                return colliders.Length;
            }

            return 0;
        }
        
        [BurstCompile]
        private struct OverlapQueryJob : IJob
        {
            [ReadOnly]
            public OctreeComponent Octree;
            
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public Collider Query;
            
            public NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>> OverlapResult;
            public int LayerMask;
            
            [WriteOnly]
            public NativeArray<EntityHolder<Entity>> Colliders;
            
            public void Execute()
            {
                OverlapResult.Clear();
                Octree.StaticColliders.RangeColliderUnique(Query, OverlapResult, LayerMask);
                Octree.DynamicColliders.RangeColliderUnique(Query, OverlapResult, LayerMask);

                var count = 0;
                int arrayLength = Colliders.Length;
                foreach (var overlapHolder in OverlapResult)
                {
                    if (arrayLength == count)
                        break;
                
                    Colliders[count] = overlapHolder.Obj;
                    count++;
                }
            }
        }
    }
}