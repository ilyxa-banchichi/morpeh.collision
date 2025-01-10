using System;
using Unity.Collections;

namespace NativeTrees
{
    public static class NativeOctreeRangeOBBExtensions
    {
        public static void RangeOBBUnique<T>(this NativeOctree<T> octree, OBB range,
            NativeParallelHashSet<OverlapHolder<T>> results, int mask = ~0) 
            where T : unmanaged, IEquatable<T>, ILayerProvider, IOBBProvider
        {
            var vistor = new RangeOBBUniqueVisitor<T>()
            {
                results = results,
                queryOBB = range,
                mask = mask
            };
         
            octree.Range((AABB)range, ref vistor);
        }

        struct RangeOBBUniqueVisitor<T> : IOctreeRangeVisitor<T> 
            where T : unmanaged, IEquatable<T>, ILayerProvider, IOBBProvider
        {
            public NativeParallelHashSet<OverlapHolder<T>> results;
            public OBB queryOBB;
            public int mask;
            
            public bool OnVisit(T obj, AABB _, AABB __)
            {
                if (!LayerUtils.ShouldCollide(obj.Layer, mask))
                    return true;
                
                var overlap = obj.OBB.Overlaps(queryOBB);
                if (overlap.IsIntersecting)
                {
                    results.Add(new OverlapHolder<T>()
                    {
                        Overlap = overlap,
                        Obj = obj
                    });   
                }

                return true; // always keep iterating, we want to catch all objects
            }
        }
    }
}