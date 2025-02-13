using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Collision.Requests;
using Scellecs.Morpeh.Collision.Systems;

namespace Scellecs.Morpeh.Collision
{
    public class CollisionFeature : LateUpdateFeature
    {
        private readonly ICollisionService _collisionService;
        
        public CollisionFeature(ICollisionService collisionService)
        {
            _collisionService = collisionService;
            _collisionService.Initialize(World.Default);
        }
        
        protected override void Initialize()
        {
            RegisterRequest<CreateBoxColliderRequest>();
            RegisterRequest<RemoveColliderRequest>();
            
            AddSystem(new CreateColliderSystem());
            AddSystem(new RemoveColliderSystem());
            
            AddSystem(new UpdateDynamicCollidersSystem());
            AddSystem(new UpdateCollisionTreesSystem());
            
            AddSystem(new TreeTraversalSystem());
            AddSystem(new CollisionResolutionSystem());
            AddSystem(new UpdateCollisionEventsSystem());

            MarkComponentsDisposable();
        }
        
        private void MarkComponentsDisposable()
        {
            World.Default.GetStash<ColliderComponent>().AsDisposable();
            World.Default.GetStash<CollisionEventsComponent>().AsDisposable();
            World.Default.GetStash<OctreeComponent>().AsDisposable();
        }
    }
}