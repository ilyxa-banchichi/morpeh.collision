namespace NativeTrees
{
    public static class DummyCapsuleExtensions
    {
        public static OverlapResult Overlaps(this in CapsuleCollider capsule1, in CapsuleCollider capsule2)
        {
            return new OverlapResult() { IsIntersecting = false };
        }
        
        public static OverlapResult Overlaps(this in BoxCollider box, in CapsuleCollider capsule)
        {
            return capsule.Overlaps(box);
        }
        
        public static OverlapResult Overlaps(this in CapsuleCollider capsule, in BoxCollider box)
        {
            return new OverlapResult() { IsIntersecting = false };
        }
        
        public static OverlapResult Overlaps(this in SphereCollider sphere, in CapsuleCollider capsule)
        {
            return capsule.Overlaps(sphere);
        }
        
        public static OverlapResult Overlaps(this in CapsuleCollider capsule, in SphereCollider sphere)
        {
            return new OverlapResult() { IsIntersecting = false };
        }
        
        public static OverlapResult Overlaps(this in TerrainCollider terrain, in CapsuleCollider capsule)
        {
            return capsule.Overlaps(terrain);
        }
        
        public static OverlapResult Overlaps(this in CapsuleCollider capsule, in TerrainCollider terrain)
        {
            return new OverlapResult() { IsIntersecting = false };
        }
    }
}