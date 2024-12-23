using Scellecs.Morpeh.Addons.Feature;

namespace Scellecs.Morpeh
{
    public class TransformFeature : LateUpdateFeature
    {
        protected override void Initialize()
        {
            AddSystem(new TransformSystem());
        }
    }
}
