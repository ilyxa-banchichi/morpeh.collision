using NativeTrees;
using NativeTrees.Unity;
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
    public struct CreateBoxColliderRequest : IComponent, IValidatableWithGameObject, IDrawGizmosSelected
    {
        public float3 Center;
        public float3 Size;
        
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
            Layer = gameObject.layer;
            IsStatic = gameObject.isStatic;
            Weight = math.clamp(Weight, 1, int.MaxValue);
        }

        public void OnDrawGizmosSelected(GameObject gameObject)
        {
            var aabb = new AABB(Center - Size * .5f, Center + Size * .5f);
            var obb = new OBB(aabb, gameObject.transform.position, gameObject.transform.rotation);
            GizmoExtensions.DrawOBB(obb, Color.blue);
            GizmoExtensions.DrawAABB((AABB)obb, Color.red);
        }
    }
}