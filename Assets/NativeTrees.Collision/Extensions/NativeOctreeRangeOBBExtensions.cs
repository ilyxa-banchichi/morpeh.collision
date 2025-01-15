using System;
using Unity.Collections;

namespace NativeTrees
{
    public static unsafe class NativeOctreeRangeOBBExtensions
    {
        public static void RangeColliderUnique<T>(this NativeOctree<T> octree, void* rangeCollider,
            ColliderType rangeColliderType,
            NativeParallelHashSet<OverlapHolder<T>> results, int mask = ~0) 
            where T : unmanaged, IEquatable<T>, ILayerProvider, IColliderProvider
        {
            var vistor = new RangeColliderUniqueVisitor<T>(results, rangeCollider, rangeColliderType, mask);
            octree.Range(ColliderCastUtils.ToAABB(rangeCollider, rangeColliderType), ref vistor);
        }

        struct RangeColliderUniqueVisitor<T> : IOctreeRangeVisitor<T> 
            where T : unmanaged, IEquatable<T>, ILayerProvider, IColliderProvider
        {
            public NativeParallelHashSet<OverlapHolder<T>> results;
            public int mask;

            private BoxCollider _boxCollider;
            private SphereCollider _sphereCollider;
            private ColliderType _type;

            public RangeColliderUniqueVisitor(
                NativeParallelHashSet<OverlapHolder<T>> results, 
                void* queryCollider, ColliderType queryColliderType, 
                int mask)
            {
                this.results = results;
                this.mask = mask;

                _type = queryColliderType;
                _boxCollider = default;
                _sphereCollider = default;
                
                if (_type == ColliderType.Box)
                    _boxCollider = *ColliderCastUtils.ToBoxCollider(queryCollider);
                else if (_type == ColliderType.Sphere)
                    _sphereCollider = *ColliderCastUtils.ToSphereCollider(queryCollider);
            }

            public bool OnVisit(T obj, AABB _, AABB __)
            {
                if (!LayerUtils.ShouldCollide(obj.Layer, mask))
                    return true;

                OverlapResult overlapResult = default;
                if (_type == ColliderType.Box)
                    overlapResult = OverlapBox(obj);
                else if (_type == ColliderType.Sphere)
                    overlapResult = OverlapSphere(obj);
                
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

            private OverlapResult OverlapBox(T obj)
            {
                if (obj.Type == ColliderType.Box)
                {
                    var other = ColliderCastUtils.ToBoxCollider(obj.Collider);
                    return other->Overlaps(_boxCollider);
                }
                else if (obj.Type == ColliderType.Sphere)
                {
                    var other = ColliderCastUtils.ToSphereCollider(obj.Collider);
                    return other->Overlaps(_boxCollider);
                }

                return default;
            }
            
            private OverlapResult OverlapSphere(T obj)
            {
                if (obj.Type == ColliderType.Box)
                {
                    var other = ColliderCastUtils.ToBoxCollider(obj.Collider);
                    return other->Overlaps(_sphereCollider);
                }
                else if (obj.Type == ColliderType.Sphere)
                {
                    var other = ColliderCastUtils.ToSphereCollider(obj.Collider);
                    return other->Overlaps(_sphereCollider);
                }

                return default;
            }
        }
    }
}