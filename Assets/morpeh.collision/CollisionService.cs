using NativeTrees;
using Scellecs.Morpeh.Collision.Components;
using Unity.Mathematics;
using UnityEngine;

namespace Scellecs.Morpeh.Collision
{
    public class CollisionService : ICollisionService
    {
        private World _defaultWorld;
        private Filter _octree;
        private Stash<OctreeComponent> _octreeComponents;
        
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
    }
}