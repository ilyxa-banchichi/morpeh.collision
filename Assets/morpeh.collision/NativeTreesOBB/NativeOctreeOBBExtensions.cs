namespace NativeTrees
{
    /// <summary>
    /// Convenience queries that operate just on the object's bounding boxes
    /// </summary>
    public static class NativeOctreeOBBExtensions
    {
        // /// <summary>
        // /// Performs a raycast on the octree just using the bounds of the objects in it
        // /// </summary>
        // public static bool RaycastAABB<T>(this NativeOctree<T> octree, Ray ray, out OctreeRaycastHit<T> hit, float maxDistance = float.PositiveInfinity) where T : unmanaged
        // {
        //     return octree.Raycast<RayAABBIntersecter<T>>(ray, out hit, maxDistance: maxDistance);
        // }
        //
        // struct RayAABBIntersecter<T> : IOctreeRayIntersecter<T>
        // {
        //     public bool IntersectRay(in PrecomputedRay ray, T obj, AABB objBounds, out float distance)
        //     {
        //         return objBounds.IntersectsRay(ray, out distance);
        //     }
        // }
    }
}