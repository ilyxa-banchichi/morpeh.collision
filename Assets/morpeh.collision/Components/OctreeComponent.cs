using NativeTrees;
using Unity.IL2CPP.CompilerServices;

namespace Scellecs.Morpeh.Collision.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct OctreeComponent : IComponent
    {
        public NativeOctree<EntityHolder<Entity>> DynamicRigidbodies;
        public NativeOctree<EntityHolder<Entity>> StaticRigidbodies;
        public int LastStaticRigidbodiesCount;
        
        // public NativeOctree<EntityHolder<Entity>> DynamicTriggers;
        // public NativeOctree<EntityHolder<Entity>> StaticTriggers;
        // public int LastStaticTriggersCount;
    }
}
