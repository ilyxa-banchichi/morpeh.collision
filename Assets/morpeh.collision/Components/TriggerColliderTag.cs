using Unity.IL2CPP.CompilerServices;

namespace Scellecs.Morpeh.Collision.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct TriggerColliderTag : IComponent { }
}