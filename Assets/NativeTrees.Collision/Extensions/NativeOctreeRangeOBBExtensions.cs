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
            if (range.Type == ColliderType.Box)
            {
                var boxRangeColliderUniqueVisitor = new BoxRangeColliderUniqueVisitor<T>(
                    ref ColliderCastUtils.ToBoxColliderRef(range), mask, results);
                octree.Range(ColliderCastUtils.ToAABB(range), ref boxRangeColliderUniqueVisitor);
            }
            else if (range.Type == ColliderType.Sphere)
            {
                var sphereRangeColliderUniqueVisitor = new SphereRangeColliderUniqueVisitor<T>(
                    ref ColliderCastUtils.ToSphereColliderRef(range), mask, results);
                octree.Range(ColliderCastUtils.ToAABB(range), ref sphereRangeColliderUniqueVisitor);
            }
            else if (range.Type == ColliderType.Capsule)
            {
                var capsuleRangeColliderUniqueVisitor = new CapsuleRangeColliderUniqueVisitor<T>(
                    ref ColliderCastUtils.ToCapsuleColliderRef(range), mask, results);
                octree.Range(ColliderCastUtils.ToAABB(range), ref capsuleRangeColliderUniqueVisitor);
            }
#if UNITY_EDITOR
            else if (range.Type == ColliderType.Terrain)
                throw new ArgumentException("Terrain collider cannot be dynamic");
#endif
        }

        private readonly struct BaseRangeColliderUniqueVisitor<T> where T : unmanaged, ILayerProvider
        {
            private readonly int _mask;
            
            public BaseRangeColliderUniqueVisitor(int mask)
            {
                _mask = mask;
            }

            public bool ShouldCollide(ref T obj, ref AABB aabb1, ref AABB aabb2)
            {
                if (!LayerUtils.ShouldCollide(obj.Layer, _mask))
                    return false;

                if (!aabb1.Overlaps(aabb2))
                    return false;

                return true;
            }
        }

        private readonly struct BoxRangeColliderUniqueVisitor<T> : IOctreeRangeVisitor<T> 
            where T : unmanaged, IEquatable<T>, ILayerProvider, IColliderProvider
        {
            private readonly NativeParallelHashSet<OverlapHolder<T>> _results;
            private readonly BaseRangeColliderUniqueVisitor<T> _base;
            private readonly BoxCollider _boxCollider;

            public BoxRangeColliderUniqueVisitor(ref BoxCollider range, int mask,
                NativeParallelHashSet<OverlapHolder<T>> results)
            {
                _results = results;
                _base = new BaseRangeColliderUniqueVisitor<T>(mask);
                _boxCollider = range;
            }

            public bool OnVisit(T obj, AABB aabb1, AABB aabb2)
            {
                if (!_base.ShouldCollide(ref obj, ref aabb1, ref aabb2))
                    return true;

                var overlapResult = OverlapBox(ref obj);
                if (overlapResult.IsIntersecting)
                {
                    _results.Add(new OverlapHolder<T>()
                    {
                        Overlap = overlapResult,
                        Obj = obj
                    });   
                }

                return true; // always keep iterating, we want to catch all objects
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private OverlapResult OverlapBox(ref T obj)
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
        }
        
        private readonly struct SphereRangeColliderUniqueVisitor<T> : IOctreeRangeVisitor<T> 
            where T : unmanaged, IEquatable<T>, ILayerProvider, IColliderProvider
        {
            private readonly NativeParallelHashSet<OverlapHolder<T>> _results;
            private readonly BaseRangeColliderUniqueVisitor<T> _base;
            private readonly SphereCollider _sphereCollider;

            public SphereRangeColliderUniqueVisitor(ref SphereCollider range, int mask,
                NativeParallelHashSet<OverlapHolder<T>> results)
            {
                _results = results;
                _base = new BaseRangeColliderUniqueVisitor<T>(mask);
                _sphereCollider = range;
            }

            public bool OnVisit(T obj, AABB aabb1, AABB aabb2)
            {
                if (!_base.ShouldCollide(ref obj, ref aabb1, ref aabb2))
                    return true;

                var overlapResult = OverlapSphere(ref obj);
                if (overlapResult.IsIntersecting)
                {
                    _results.Add(new OverlapHolder<T>()
                    {
                        Overlap = overlapResult,
                        Obj = obj
                    });   
                }

                return true; // always keep iterating, we want to catch all objects
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private OverlapResult OverlapSphere(ref T obj)
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
        }
        
        private readonly struct CapsuleRangeColliderUniqueVisitor<T> : IOctreeRangeVisitor<T> 
            where T : unmanaged, IEquatable<T>, ILayerProvider, IColliderProvider
        {
            private readonly NativeParallelHashSet<OverlapHolder<T>> _results;
            private readonly BaseRangeColliderUniqueVisitor<T> _base;
            private readonly CapsuleCollider _capsuleCollider;

            public CapsuleRangeColliderUniqueVisitor(ref CapsuleCollider range, int mask,
                NativeParallelHashSet<OverlapHolder<T>> results)
            {
                _results = results;
                _base = new BaseRangeColliderUniqueVisitor<T>(mask);
                _capsuleCollider = range;
            }

            public bool OnVisit(T obj, AABB aabb1, AABB aabb2)
            {
                if (!_base.ShouldCollide(ref obj, ref aabb1, ref aabb2))
                    return true;

                var overlapResult = OverlapCapsule(ref obj);
                if (overlapResult.IsIntersecting)
                {
                    _results.Add(new OverlapHolder<T>()
                    {
                        Overlap = overlapResult,
                        Obj = obj
                    });   
                }

                return true; // always keep iterating, we want to catch all objects
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private OverlapResult OverlapCapsule(ref T obj)
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