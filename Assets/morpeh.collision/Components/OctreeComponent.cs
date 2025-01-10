using System;
using NativeTrees;
using Unity.IL2CPP.CompilerServices;

namespace Scellecs.Morpeh.Collision.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct OctreeComponent : IComponent, IDisposable
    {
        public NativeOctree<EntityHolder<Entity>> DynamicColliders;
        public NativeOctree<EntityHolder<Entity>> StaticColliders;
        public int LastStaticCollidersCount;
        
        public void Dispose()
        {
            if (DynamicColliders.IsValid)
                DynamicColliders.Dispose();
            
            if (StaticColliders.IsValid)
                StaticColliders.Dispose();
        }
    }
}
