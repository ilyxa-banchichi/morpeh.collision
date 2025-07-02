using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Samples.Scripts
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct GameObjectComponent : IComponent, IValidatableWithGameObject
    {
        public Transform Transform;

        public void OnValidate(GameObject gameObject)
        {
            Transform = gameObject.transform;
        }
    }
}