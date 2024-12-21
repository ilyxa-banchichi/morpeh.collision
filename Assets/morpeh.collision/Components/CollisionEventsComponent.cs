using System;
using NativeTrees;
using TriInspector;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;

namespace Scellecs.Morpeh.Collision.Components
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct CollisionEventsComponent : IComponent, IDisposable
    {
        public NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>> OnCollisionEnter;
        public NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>> OnCollisionStay;
        public NativeParallelHashSet<OverlapHolder<EntityHolder<Entity>>> OnCollisionExit;

#if UNITY_EDITOR
        [ShowInInspector]
        private OverlapHolder<EntityHolder<Entity>>[] _onCollisionEnter
        {
            get
            {
                var array = OnCollisionEnter.ToNativeArray(Allocator.Temp);
                OverlapHolder<EntityHolder<Entity>>[] e = array.ToArray();
                array.Dispose();

                return e;
            }
        }
        
        [ShowInInspector]
        private OverlapHolder<EntityHolder<Entity>>[] _onCollisionStay
        {
            get
            {
                var array = OnCollisionStay.ToNativeArray(Allocator.Temp);
                OverlapHolder<EntityHolder<Entity>>[] e = array.ToArray();
                array.Dispose();

                return e;
            }
        }
        
        [ShowInInspector]
        private OverlapHolder<EntityHolder<Entity>>[] _onCollisionExit
        {
            get
            {
                var array = OnCollisionExit.ToNativeArray(Allocator.Temp);
                OverlapHolder<EntityHolder<Entity>>[] e = array.ToArray();
                array.Dispose();

                return e;
            }
        }
#endif

        public void Dispose()
        {
            OnCollisionEnter.Dispose();
            OnCollisionStay.Dispose();
            OnCollisionExit.Dispose();
        }
    }
}