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
        public T Entity;
        public int Layer { get; }
        public OBB OBB { get; }

        public EntityHolder(T entity, int layer, OBB obb)
        {
            Entity = entity;
            Layer = layer;
            OBB = obb;
        }
        
        public override bool Equals(object obj)
        {
            return obj is EntityHolder<T> other && Equals(other);
        }

        public bool Equals(EntityHolder<T> other)
        {
            return Entity.Equals(other.Entity) && Layer == other.Layer && OBB.Equals(other.OBB);
        }

        public override int GetHashCode()
        {
            return Entity.GetHashCode();
        }
    }
}