using System;
using NativeTrees;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Scellecs.Morpeh.Collision.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct OctreeComponent : IComponent, IDisposable
    {
        public NativeOctree<EntityHolder<Entity>> DynamicColliders;
        public NativeOctree<EntityHolder<Entity>> StaticColliders;
        public int LastStaticCollidersCount;
        
        public void Dispose()
        {
            // if (DynamicColliders.IsValid)
                DynamicColliders.Dispose();
            
            // if (StaticColliders.IsValid)
                StaticColliders.Dispose();
        }
    }

    public static class OctreeComponentExtensions
    {
        public static bool Raycast(this OctreeComponent octree, Ray ray, out RaycastHit hit, 
            float maxDistance = float.PositiveInfinity, int layerMask = ~0)
        {
            hit = new RaycastHit();
            
            var staticResult = octree.StaticColliders.RaycastOBB(ray, out var hitStatic, maxDistance, layerMask);
            var dynamicResult = octree.DynamicColliders.RaycastOBB(ray, out var hitDynamic, maxDistance, layerMask);

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

            hit = new RaycastHit()
            {
                EntityHolder = finalHit.obj,
                Point = finalHit.point,
                Distance = distance,
            };
                
            return true;
        }
    }

    public struct RaycastHit
    {
        public EntityHolder<Entity> EntityHolder;
        public float3 Point;
        public float Distance;
        // ToDo: Normal
        // public float3 Normal;
    }
}
