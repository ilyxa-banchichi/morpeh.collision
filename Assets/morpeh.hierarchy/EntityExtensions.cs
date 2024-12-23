using Scellecs.Morpeh.Hierarchy.Systems;
using Unity.Burst;

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
                    HierarchyStashCache.ParentStash.Remove(entity);
                }
            }
            else
            {
                HierarchyStashCache.ParentStash.Set(entity, new ParentComponent() { Value = parent});
                HierarchyStashCache.ParentChangedRequestStash.Set(entity);
            }
        }
    }
}
