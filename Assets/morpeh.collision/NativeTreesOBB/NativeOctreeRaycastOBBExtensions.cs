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
            this NativeOctree<T> octree, Ray ray, out OctreeRaycastHit<T> hit, float maxDistance = float.PositiveInfinity) 
            where T : unmanaged, ILayerProvider, IOBBProvider, IEquatable<T>
        {
            return octree.Raycast<RayOBBIntersecter<T>>(ray, out hit, maxDistance: maxDistance);
        }
        
        struct RayOBBIntersecter<T> : IOctreeRayIntersecter<T>
            where T : unmanaged, ILayerProvider, IOBBProvider, IEquatable<T>
        {
            public bool IntersectRay(in PrecomputedRay ray, T obj, AABB objBounds, out float distance)
            {
                return obj.OBB.IntersectsRay(ray, out distance);
            }
        }
    }
}