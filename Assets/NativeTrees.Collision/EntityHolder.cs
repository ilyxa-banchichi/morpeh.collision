using System;

namespace NativeTrees
{
    public interface ILayerProvider
    {
        int Layer { get; }
    }
    
    public unsafe interface IColliderProvider
    {
        void* Collider { get; }
        ColliderType Type { get; }
    }
    
    [Serializable]
    public unsafe struct EntityHolder<T> : IEquatable<EntityHolder<T>>, ILayerProvider, IColliderProvider 
        where T : unmanaged, IEquatable<T>
    {
        public T Entity;
        public int Layer { get; }
        public void* Collider { get; }
        public ColliderType Type { get; }

        public EntityHolder(T entity, int layer, void* collider, ColliderType type)
        {
            Entity = entity;
            Layer = layer;
            Collider = collider;
            Type = type;
        }
        
        public override bool Equals(object obj)
        {
            return obj is EntityHolder<T> other && Equals(other) && Type.Equals(other.Type);
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