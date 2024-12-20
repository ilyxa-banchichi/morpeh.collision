using TriInspector;
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
        public float3 Center;
        public float3 Size;
        
        [ReadOnly]
        public int Layer;
        
        [ReadOnly]
        public bool IsStatic;
        
        [ReadOnly]
        public float3 InitPosition;
        
        [ReadOnly]
        public quaternion InitRotation;
        
        public bool IsTrigger;
        
        [Title("Rigidbody")]
        [ShowIf(nameof(IsTrigger), false)]
        public int Weight;
        
        [ShowIf(nameof(IsTrigger), false)]
        public bool3 FreezePosition;

        public void OnValidate(GameObject gameObject)
        {
            Layer = gameObject.layer;
            IsStatic = gameObject.isStatic;
            InitPosition = gameObject.transform.position;
            InitRotation = gameObject.transform.rotation;
            Weight = math.clamp(Weight, 1, int.MaxValue);
        }
    }
}