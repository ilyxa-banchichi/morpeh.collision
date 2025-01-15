using System;
using System.Collections.Generic;

namespace NativeTrees
{
    [Serializable]
    public struct OverlapHolder<T> : IEquatable<OverlapHolder<T>>
        where T : unmanaged, IEquatable<T>, ILayerProvider, IColliderProvider
    {
        public OverlapResult Overlap;
        public T Obj;

        public override bool Equals(object obj)
        {
            return obj is OverlapHolder<T> other && Equals(other);
        }
        
        public bool Equals(OverlapHolder<T> other)
        {
            return Obj.Equals(other.Obj);
        }

        public override int GetHashCode()
        {
            return Obj.GetHashCode();
        }
    }
}