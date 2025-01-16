using Unity.Mathematics;

namespace NativeTrees
{
    public struct OverlapResult
    {
        public bool IsIntersecting;
        public float3 Axis;
        public float Depth;
    }
}