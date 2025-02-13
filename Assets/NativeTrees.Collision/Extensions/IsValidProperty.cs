using Unity.Mathematics;

namespace NativeTrees
{
    public static class NativeOctreeExtensions
    {
        public static bool IsValid<T>(this NativeOctree<T> octree) where T : unmanaged =>
            math.any(octree.Bounds.max != octree.Bounds.min);
    }
}
