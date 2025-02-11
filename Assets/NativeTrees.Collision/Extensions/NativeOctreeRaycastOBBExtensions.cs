using System;
using UnityEngine;

namespace NativeTrees
{
    /// <summary>
    /// Convenience queries that operate just on the object's bounding boxes
    /// </summary>
    public static class NativeOctreeRaycastOBBExtensions
    {
        /// <summary>
        /// Performs a raycast on the octree just using the bounds of the objects in it
        /// </summary>
        public static bool RaycastOBB<T>(
            this NativeOctree<T> octree, Ray ray, out OctreeRaycastHit<T> hit, 
            float maxDistance = float.PositiveInfinity, int layerMask = ~0) 
            where T : unmanaged, ILayerProvider, IColliderProvider, IEquatable<T>
        {
            var intersecter = new RayOBBIntersecter<T>() { LayerMask = layerMask };
            return octree.Raycast(ray, out hit, intersecter: intersecter, maxDistance: maxDistance);
        }
        
        struct RayOBBIntersecter<T> : IOctreeRayIntersecter<T>
            where T : unmanaged, ILayerProvider, IColliderProvider, IEquatable<T>
        {
            public int LayerMask;
            
            public bool IntersectRay(in PrecomputedRay ray, T obj, AABB objBounds, out float distance)
            {
                distance = 0;
                if (!LayerUtils.ShouldCollide(obj.Layer, LayerMask))
                    return false;
                
                if (!objBounds.IntersectsRay(ray, out distance))
                    return false;
                
                if (obj.Collider.Type == ColliderType.Box)
                {
                    ref var other = ref ColliderCastUtils.ToBoxColliderRef(obj.Collider);
                    return other.IntersectsRay(ray, out distance);
                }
                else if (obj.Collider.Type == ColliderType.Sphere)
                {
                    ref var other = ref ColliderCastUtils.ToSphereColliderRef(obj.Collider);
                    return other.IntersectsRay(ray, out distance);
                }
                else if (obj.Collider.Type == ColliderType.Capsule)
                {
                    ref var other = ref ColliderCastUtils.ToCapsuleColliderRef(obj.Collider);
                    return other.IntersectsRay(ray, out distance);
                }
                else if (obj.Collider.Type == ColliderType.Terrain)
                {
                    ref var other = ref ColliderCastUtils.ToTerrainColliderRef(obj.Collider);
                    return other.IntersectsRay(ray, out distance);
                }

                return false;
            }
        }
    }
}