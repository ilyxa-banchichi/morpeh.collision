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
                if (!LayerUtils.ShouldCollide(obj.Layer, LayerMask))
                {
                    distance = 0;
                    return false;
                }

                //ToDo: void
                distance = 99999;
                return true;
                //return obj.OBB.IntersectsRay(ray, out distance);
            }
        }
    }
}