using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Hierarchy;
using Scellecs.Morpeh.Hierarchy.Systems;

namespace Scellecs.Morpeh
{
    public class HierarchyFeature : LateUpdateFeature
    {
        protected override void Initialize()
        {
            RegisterRequest<ParentChangedRequest>();
            
            HierarchyStashCache.RefreshCache(World.Default);
            AddSystem(new ParentSystem());
        }
    }
}
