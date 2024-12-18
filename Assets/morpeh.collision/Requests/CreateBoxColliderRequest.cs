using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace Scellecs.Morpeh.Collision.Requests
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct CreateBoxColliderRequest : IComponent
    {
        public int Layer;
        public int Weight;
        
        public float3 Center;
        public float3 Size;
        
        public bool3 FreezePosition;
        public bool IsStatic;

        public float3 InitPosition;
        public quaternion InitRotation;
    }
}