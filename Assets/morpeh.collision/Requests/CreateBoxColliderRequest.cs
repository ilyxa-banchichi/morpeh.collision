using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Scellecs.Morpeh.Collision.Requests
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct CreateBoxColliderRequest : IComponent, IValidatableWithGameObject
    {
        public int Layer;
        public int Weight;
        
        public float3 Center;
        public float3 Size;
        
        public bool3 FreezePosition;
        public bool IsStatic;

        public float3 InitPosition;
        public quaternion InitRotation;

        public void OnValidate(GameObject gameObject)
        {
            Layer = gameObject.layer;
            IsStatic = gameObject.isStatic;
            InitPosition = gameObject.transform.position;
            InitRotation = gameObject.transform.rotation;
        }
    }
}