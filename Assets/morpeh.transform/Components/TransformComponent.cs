using System;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Scellecs.Morpeh.Transform.Components 
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct TransformComponent : IComponent, IValidatableWithGameObject
    {
        public bool IgnoreParentDestroyed;
        public float3 LocalPosition;
        public quaternion LocalRotation;
        public float3 LocalScale;

        public float4x4 LocalToWorld;

        public void OnValidate(GameObject gameObject)
        {
            TransformHelper.ApplyUnityTransformToComponent(gameObject, ref this);
        }
    }
}
