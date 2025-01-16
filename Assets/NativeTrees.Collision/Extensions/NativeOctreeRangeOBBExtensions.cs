using System;
using Unity.Collections;

namespace NativeTrees
{
    public static class NativeOctreeRangeOBBExtensions
    {
        public static void RangeColliderUnique<T>(this NativeOctree<T> octree, Collider range,
            NativeParallelHashSet<OverlapHolder<T>> results, int mask = ~0) 
            where T : unmanaged, IEquatable<T>, ILayerProvider, IColliderProvider
        {
            var vistor = new RangeColliderUniqueVisitor<T>(results, range, mask);
            octree.Range(ColliderCastUtils.ToAABB(range), ref vistor);
        }

        struct RangeColliderUniqueVisitor<T> : IOctreeRangeVisitor<T> 
            where T : unmanaged, IEquatable<T>, ILayerProvider, IColliderProvider
        {
            public NativeParallelHashSet<OverlapHolder<T>> results;
            public int mask;

            private ColliderType _type;
            private BoxCollider _boxCollider;
            private SphereCollider _sphereCollider;

            public RangeColliderUniqueVisitor(
                NativeParallelHashSet<OverlapHolder<T>> results, 
                Collider range, 
                int mask)
            {
                this.results = results;
                this.mask = mask;

                _type = range.Type;
                _boxCollider = default;
                _sphereCollider = default;
                
                if (_type == ColliderType.Box)
                    _boxCollider = ColliderCastUtils.ToBoxColliderRef(range);
                else if (_type == ColliderType.Sphere)
                    _sphereCollider = ColliderCastUtils.ToSphereColliderRef(range);
#if UNITY_EDITOR
                else if (_type == ColliderType.Terrain)
                    throw new ArgumentException("Terrain collider cannot be dynamic");
#endif
            }

            public bool OnVisit(T obj, AABB aabb1, AABB aabb2)
            {
                if (!LayerUtils.ShouldCollide(obj.Layer, mask))
                    return true;

                OverlapResult overlapResult = default;
                if (_type == ColliderType.Box)
                    overlapResult = OverlapBox(obj, aabb1, aabb2);
                else if (_type == ColliderType.Sphere)
                    overlapResult = OverlapSphere(obj, aabb1, aabb2);
                
                if (overlapResult.IsIntersecting)
                {
                    results.Add(new OverlapHolder<T>()
                    {
                        Overlap = overlapResult,
                        Obj = obj
                    });   
                }

                return true; // always keep iterating, we want to catch all objects
            }

            private OverlapResult OverlapBox(T obj, AABB aabb1, AABB aabb2)
            {
                if (obj.Collider.Type == ColliderType.Box)
                {
                    ref var other = ref ColliderCastUtils.ToBoxColliderRef(obj.Collider);
                    return other.Overlaps(_boxCollider);
                }
                else if (obj.Collider.Type == ColliderType.Sphere)
                {
                    ref var other = ref ColliderCastUtils.ToSphereColliderRef(obj.Collider);
                    return other.Overlaps(_boxCollider);
                }
                else if (obj.Collider.Type == ColliderType.Terrain)
                {
                    if (!aabb1.Overlaps(aabb2))
                        return new OverlapResult() { IsIntersecting = false };
                    
                    ref var other = ref ColliderCastUtils.ToTerrainColliderRef(obj.Collider);
                    return other.Overlaps(_boxCollider);
                }

                return default;
            }
            
            private OverlapResult OverlapSphere(T obj, AABB aabb1, AABB aabb2)
            {
                if (obj.Collider.Type == ColliderType.Box)
                {
                    ref var other = ref ColliderCastUtils.ToBoxColliderRef(obj.Collider);
                    return other.Overlaps(_sphereCollider);
                }
                else if (obj.Collider.Type == ColliderType.Sphere)
                {
                    ref var other = ref ColliderCastUtils.ToSphereColliderRef(obj.Collider);
                    return other.Overlaps(_sphereCollider);
                }
                else if (obj.Collider.Type == ColliderType.Terrain)
                {
                    if (!aabb1.Overlaps(aabb2))
                        return new OverlapResult() { IsIntersecting = false };
                    
                    ref var other = ref ColliderCastUtils.ToTerrainColliderRef(obj.Collider);
                    return other.Overlaps(_sphereCollider);
                }

                return default;
            }
        }
    }
}