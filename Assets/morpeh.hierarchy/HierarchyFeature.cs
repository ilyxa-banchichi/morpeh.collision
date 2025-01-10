using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Hierarchy;
using Scellecs.Morpeh.Hierarchy.Systems;

namespace Scellecs.Morpeh
{
    public class HierarchyFeature : LateUpdateFeature
    {
        protected override void Initialize()
        {
            HierarchyStashCache.RefreshCache(World.Default);
            RegisterRequest<ParentChangedRequest>();
            AddSystem(new ParentSystem());

            MarkComponentsDisposable();
        }
        
        private void MarkComponentsDisposable()
        {
            World.Default.GetStash<ChildComponent>().AsDisposable();
        }
    }
}
