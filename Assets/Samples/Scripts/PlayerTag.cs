using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace Samples.Scripts
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct PlayerTag : IComponent { }
}