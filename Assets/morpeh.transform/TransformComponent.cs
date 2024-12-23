using System;
using Scellecs.Morpeh;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Scellecs.Morpeh
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct TransformComponent : IComponent, IDisposable, IValidatableWithGameObject
    {
        public bool IgnoreParentDestroyed;
        public float3 LocalPosition;
        public quaternion LocalRotation;
        public float3 LocalScale;

        public float4x4 LocalToWorld;

        [HideInInspector]
        public Entity Parent;

        [HideInInspector]
        public NativeList<Entity> Children;

        public void Dispose()
        {
            this.DestroyHierarchy();
            Children.Dispose();
        }

        public void OnValidate(GameObject gameObject)
        {
            TransformHelper.ApplyUnityTransformToComponent(gameObject, ref this);
            
            // Кулхак чтобы в эдиторе Parent был, как в билде, на случай, если TransformComponent добавлен
            // не через HierarchyCodeUniversalProvider
            Parent = new Entity();
        }
    }
}
