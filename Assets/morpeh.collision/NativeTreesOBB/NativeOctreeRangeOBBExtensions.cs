using System;
using Unity.Collections;

namespace NativeTrees
{
    public static class NativeOctreeRangeOBBExtensions
    {
        public static void RangeOBB<T>(this NativeOctree<T> octree, OBB range, NativeList<OverlapHolder<T>> results) 
            where T : unmanaged, ILayerProvider, IOBBProvider, IEquatable<T>
        {
            var vistor = new RangeOBBVisitor<T>()
            {
                results = results,
                queryOBB = range
            };
         
            octree.Range((AABB)range, ref vistor);
        }

        struct RangeOBBVisitor<T> : IOctreeRangeVisitor<T> 
            where T : unmanaged, ILayerProvider, IOBBProvider, IEquatable<T>
        {
            public NativeList<OverlapHolder<T>> results;
            public OBB queryOBB;
            
            public bool OnVisit(T obj, AABB _, AABB __)
            {
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
        
        public static void RangeOBBUnique<T>(this NativeOctree<T> octree, OBB range, 
            NativeParallelHashSet<OverlapHolder<T>> results) 
            where T : unmanaged, IEquatable<T>, ILayerProvider, IOBBProvider
        {
            var vistor = new RangeOBBUniqueVisitor<T>()
            {
                results = results,
                queryOBB = range
            };
         
            octree.Range((AABB)range, ref vistor);
        }

        struct RangeOBBUniqueVisitor<T> : IOctreeRangeVisitor<T> 
            where T : unmanaged, IEquatable<T>, ILayerProvider, IOBBProvider
        {
            public NativeParallelHashSet<OverlapHolder<T>> results;
            public OBB queryOBB;
            
            public bool OnVisit(T obj, AABB _, AABB __)
            {
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