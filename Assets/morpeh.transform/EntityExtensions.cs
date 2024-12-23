using Unity.Burst;
using Unity.Collections;

namespace Scellecs.Morpeh
{
    public static class EntityExtensions
    {
        [BurstDiscard]
        public static void DestroyWithHierarchy(this Entity entity)
        {
            ref var transform = ref TransformCache.Stash.Get(entity, out var exist);
            if (exist)
                entity.ResetParent(ref transform);

            TransformCache.World.RemoveEntity(entity);
        }

        [BurstDiscard]
        public static void SetParent(this Entity entity, Entity parent)
        {
            ref var transform = ref TransformCache.Stash.Get(entity, out var exist);
            if (exist)
            {
                entity.ResetParent(ref transform);

                ref var parentTransform = ref TransformCache.Stash.Get(parent, out exist);
                if (exist)
                {
                    parentTransform.Children.Add(entity);
                    transform.ChangeParent(parent);
                }
            }
        }

        [BurstDiscard]
        public static void ResetParent(this Entity entity, ref TransformComponent transform)
        {
            if (transform.Parent != default)
            {
                ref var parentTransform = ref TransformCache.Stash.Get(transform.Parent, out var exist);
                if (exist)
                {
                    var index = parentTransform.Children.IndexOf(entity);
                    if (index != -1)
                        parentTransform.Children.RemoveAt(index);
                }

                transform.ChangeParent(default);
            }
        }
    }
}
