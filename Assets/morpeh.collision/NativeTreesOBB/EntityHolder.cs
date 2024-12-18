using System;

namespace NativeTrees
{
    public interface ILayerProvider
    {
        int Layer { get; }
    }
    
    public interface IOBBProvider
    {
        OBB OBB { get; }
    }
    
    public struct EntityHolder<T> : IEquatable<EntityHolder<T>>, ILayerProvider, IOBBProvider 
        where T : unmanaged, IEquatable<T>
    {
        public T Obj;
        public int Layer { get; }
        public OBB OBB { get; }

        public EntityHolder(T obj, int layer, OBB obb)
        {
            Obj = obj;
            Layer = layer;
            OBB = obb;
        }
        
        public override bool Equals(object obj)
        {
            return obj is EntityHolder<T> other && Equals(other);
        }

        public bool Equals(EntityHolder<T> other)
        {
            return Obj.Equals(other.Obj) && Layer == other.Layer && OBB.Equals(other.OBB);
        }

        public override int GetHashCode()
        {
            return Obj.GetHashCode();
        }
    }
}