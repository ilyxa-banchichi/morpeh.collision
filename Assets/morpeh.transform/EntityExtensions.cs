using System.Runtime.CompilerServices;
using Scellecs.Morpeh.Hierarchy.Systems;
using Unity.Burst;
using Unity.Mathematics;

namespace Scellecs.Morpeh.Hierarchy
{
    public static class EntityExtensions
    {
        [BurstDiscard]
        public static void SetParent(this Entity entity, Entity parent)
        {
            if (parent == default) 
            {
                if (HierarchyStashCache.ParentStash.Has(entity))
                {
                    var oldParent = HierarchyStashCache.ParentStash.Get(entity).Value;
                    HierarchyStashCache.ParentStash.Remove(entity);
                    ref var transform = ref TransformCache.Stash.Get(entity);
                    transform.LocalPosition = transform.Position();
                    transform.LocalRotation = transform.Rotation();
                    transform.LocalScale = transform.Scale();
                }
            }
            else
            {
                HierarchyStashCache.ParentStash.Set(entity, new ParentComponent() { Value = parent});
                HierarchyStashCache.ParentChangedRequestStash.Set(entity);
                
                ref var transform = ref TransformCache.Stash.Get(entity);
                ref var parentTransform = ref TransformCache.Stash.Get(parent);
                var newLocalMatrix = math.mul(math.inverse(parentTransform.LocalToWorld), transform.LocalToWorld);

                transform.LocalPosition = TransformComponentExtensions.Position(newLocalMatrix);
                transform.LocalRotation = TransformComponentExtensions.Rotation(newLocalMatrix);
                transform.LocalScale = TransformComponentExtensions.Scale(newLocalMatrix);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this Entity entity) => HierarchyStashCache.World.RemoveEntity(entity);
        
        [BurstDiscard]
        public static void DestroyHierarchy(this Entity entity)
        {
            if (HierarchyStashCache.ChildStash.Has(entity))
            {
                var children = HierarchyStashCache.ChildStash.Get(entity);
                for (int i = 0; i < children.Value.Length; i++)
                    DestroyHierarchy(children.Value[i]);
            }

            entity.Destroy();
        }
    }
}
