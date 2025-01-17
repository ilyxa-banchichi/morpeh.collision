using NativeTrees;
using NativeTrees.Unity;
using TriInspector;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using BoxCollider = UnityEngine.BoxCollider;
using CapsuleCollider = UnityEngine.CapsuleCollider;
using SphereCollider = UnityEngine.SphereCollider;
using TerrainCollider = UnityEngine.TerrainCollider;

namespace Scellecs.Morpeh.Collision.Requests
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct CreateBoxColliderRequest : IComponent, IValidatableWithGameObject
    {
        [ReadOnly]
        public ColliderType Type;
        
        public UnityEngine.Collider Collider;
        
        [ReadOnly]
        public int Layer;
        
        [ReadOnly]
        public bool IsStatic;
        
        public bool IsTrigger;
        
        [Title("Rigidbody")]
        [ShowIf(nameof(IsTrigger), false)]
        public int Weight;
        
        [ShowIf(nameof(IsTrigger), false)]
        public bool3 FreezePosition;

        public void OnValidate(GameObject gameObject)
        {
            Collider = gameObject.GetComponent<UnityEngine.Collider>();
            if (Collider is BoxCollider)
                Type = ColliderType.Box;
            else if (Collider is SphereCollider)
                Type = ColliderType.Sphere;
            else if (Collider is CapsuleCollider)
                Type = ColliderType.Capsule;
            else if (Collider is TerrainCollider)
                Type = ColliderType.Terrain;
            
            Layer = gameObject.layer;
            IsStatic = gameObject.isStatic;
            Weight = math.clamp(Weight, 1, int.MaxValue);
        }
    }
}