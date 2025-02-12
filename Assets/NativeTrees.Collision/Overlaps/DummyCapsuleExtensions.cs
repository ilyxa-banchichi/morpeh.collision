using System;
using UnityEngine;

namespace NativeTrees
{
    public static class DummyCapsuleExtensions
    {
        public static OverlapResult Overlaps(this in CapsuleCollider capsule1, in CapsuleCollider capsule2)
        {
#if UNITY_EDITOR
            Debug.LogException(new NotImplementedException("Capsule Collider is not implemented"));
#endif
            return new OverlapResult() { IsIntersecting = false };
        }
        
        public static OverlapResult Overlaps(this in BoxCollider box, in CapsuleCollider capsule)
        {
            return capsule.Overlaps(box);
        }
        
        public static OverlapResult Overlaps(this in CapsuleCollider capsule, in BoxCollider box)
        {
#if UNITY_EDITOR
            Debug.LogException(new NotImplementedException("Capsule Collider is not implemented"));
#endif
            return new OverlapResult() { IsIntersecting = false };
        }
        
        public static OverlapResult Overlaps(this in SphereCollider sphere, in CapsuleCollider capsule)
        {
            return capsule.Overlaps(sphere);
        }
        
        public static OverlapResult Overlaps(this in CapsuleCollider capsule, in SphereCollider sphere)
        {
#if UNITY_EDITOR
            Debug.LogException(new NotImplementedException("Capsule Collider is not implemented"));
#endif
            return new OverlapResult() { IsIntersecting = false };
        }
        
        public static OverlapResult Overlaps(this in TerrainCollider terrain, in CapsuleCollider capsule)
        {
            return capsule.Overlaps(terrain);
        }
        
        public static OverlapResult Overlaps(this in CapsuleCollider capsule, in TerrainCollider terrain)
        {
#if UNITY_EDITOR
            Debug.LogException(new NotImplementedException("Capsule Collider is not implemented"));
#endif
            return new OverlapResult() { IsIntersecting = false };
        }
    }
}