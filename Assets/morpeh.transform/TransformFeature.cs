using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Transforms;

namespace Scellecs.Morpeh
{
    public class TransformFeature : LateUpdateFeature
    {
        protected override void Initialize()
        {
            TransformCache.RefreshCache(World.Default);
            AddSystem(new LocalToWorldSystem());
        }
    }
}
