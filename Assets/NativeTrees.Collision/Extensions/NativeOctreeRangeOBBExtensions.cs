using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace NativeTrees
{
    public static class NativeOctreeRangeOBBExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            public NativeParallelHashSet<OverlapHolder<T>> Results;
            
            private readonly int _mask;
            private readonly ColliderType _type;
            private readonly BoxCollider _boxCollider;
            private readonly SphereCollider _sphereCollider;
            private CapsuleCollider _capsuleCollider;

            public RangeColliderUniqueVisitor(
                NativeParallelHashSet<OverlapHolder<T>> results, 
                Collider range, 
                int mask)
            {
                this.Results = results;
                this._mask = mask;

                _type = range.Type;
                _boxCollider = default;
                _sphereCollider = default;
                _capsuleCollider = default;
                
                if (_type == ColliderType.Box)
                    _boxCollider = ColliderCastUtils.ToBoxColliderRef(range);
                else if (_type == ColliderType.Sphere)
                    _sphereCollider = ColliderCastUtils.ToSphereColliderRef(range);
                else if (_type == ColliderType.Capsule)
                    _capsuleCollider = ColliderCastUtils.ToCapsuleColliderRef(range);
#if UNITY_EDITOR
                else if (_type == ColliderType.Terrain)
                    throw new ArgumentException("Terrain collider cannot be dynamic");
#endif
            }

            public bool OnVisit(T obj, AABB aabb1, AABB aabb2)
            {
                if (!LayerUtils.ShouldCollide(obj.Layer, _mask))
                    return true;

                if (!aabb1.Overlaps(aabb2))
                    return true;

                OverlapResult overlapResult = default;
                if (_type == ColliderType.Box)
                    overlapResult = OverlapBox(obj);
                else if (_type == ColliderType.Sphere)
                    overlapResult = OverlapSphere(obj);
                else if (_type == ColliderType.Capsule)
                    overlapResult = OverlapCapsule(obj);
                
                if (overlapResult.IsIntersecting)
                {
                    Results.Add(new OverlapHolder<T>()
                    {
                        Overlap = overlapResult,
                        Obj = obj
                    });   
                }

                return true; // always keep iterating, we want to catch all objects
            }

            private OverlapResult OverlapBox(T obj)
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
                else if (obj.Collider.Type == ColliderType.Capsule)
                {
                    ref var other = ref ColliderCastUtils.ToCapsuleColliderRef(obj.Collider);
                    return other.Overlaps(_boxCollider);
                }
                else if (obj.Collider.Type == ColliderType.Terrain)
                {
                    ref var other = ref ColliderCastUtils.ToTerrainColliderRef(obj.Collider);
                    return other.Overlaps(_boxCollider);
                }

                return default;
            }
            
            private OverlapResult OverlapSphere(T obj)
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
                else if (obj.Collider.Type == ColliderType.Capsule)
                {
                    ref var other = ref ColliderCastUtils.ToCapsuleColliderRef(obj.Collider);
                    return other.Overlaps(_sphereCollider);
                }
                else if (obj.Collider.Type == ColliderType.Terrain)
                {
                    ref var other = ref ColliderCastUtils.ToTerrainColliderRef(obj.Collider);
                    return other.Overlaps(_sphereCollider);
                }

                return default;
            }
            
            private OverlapResult OverlapCapsule(T obj)
            {
                if (obj.Collider.Type == ColliderType.Box)
                {
                    ref var other = ref ColliderCastUtils.ToBoxColliderRef(obj.Collider);
                    return other.Overlaps(_capsuleCollider);
                }
                else if (obj.Collider.Type == ColliderType.Sphere)
                {
                    ref var other = ref ColliderCastUtils.ToSphereColliderRef(obj.Collider);
                    return other.Overlaps(_capsuleCollider);
                }
                else if (obj.Collider.Type == ColliderType.Capsule)
                {
                    ref var other = ref ColliderCastUtils.ToCapsuleColliderRef(obj.Collider);
                    return other.Overlaps(_capsuleCollider);
                }
                else if (obj.Collider.Type == ColliderType.Terrain)
                {
                    ref var other = ref ColliderCastUtils.ToTerrainColliderRef(obj.Collider);
                    return other.Overlaps(_capsuleCollider);
                }

                return default;
            }
        }
    }
}