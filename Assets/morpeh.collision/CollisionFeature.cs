using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Collision.Requests;
using Scellecs.Morpeh.Collision.Systems;

namespace Scellecs.Morpeh.Collision
{
    public class CollisionFeature : LateUpdateFeature
    {
        protected override void Initialize()
        {
            RegisterRequest<CreateBoxColliderRequest>();
            
            AddSystem(new CreateColliderSystem());
            AddSystem(new UpdateCollisionTreesSystem());
            AddSystem(new TreeTraversalSystem());
            AddSystem(new PushOutSystem());

            MarkComponentsDisposable();
        }
        
        private void MarkComponentsDisposable()
        {
            World.Default.GetStash<BoxColliderComponent>().AsDisposable();
        }
    }
}
