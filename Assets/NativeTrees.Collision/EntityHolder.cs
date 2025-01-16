using System;

namespace NativeTrees
{
    public interface ILayerProvider
    {
        int Layer { get; }
    }
    
    public interface IColliderProvider
    {
        Collider Collider { get; }
    }
    
    [Serializable]
    public struct EntityHolder<T> : IEquatable<EntityHolder<T>>, ILayerProvider, IColliderProvider 
        where T : unmanaged, IEquatable<T>
    {
        public T Entity;
        public int Layer { get; }
        public Collider Collider { get; }

        public EntityHolder(T entity, int layer, Collider collider)
        {
            Entity = entity;
            Layer = layer;
            Collider = collider;
        }
        
        public override bool Equals(object obj)
        {
            return obj is EntityHolder<T> other && Equals(other) && Collider.Type.Equals(other.Collider.Type);
        }

        public bool Equals(EntityHolder<T> other)
        {
            return Entity.Equals(other.Entity) && Layer == other.Layer;
        }

        public override int GetHashCode()
        {
            return Entity.GetHashCode();
        }
    }
}