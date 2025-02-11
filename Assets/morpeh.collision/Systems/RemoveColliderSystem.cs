using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Collision.Requests;
using Unity.IL2CPP.CompilerServices;

namespace Scellecs.Morpeh.Collision.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RemoveColliderSystem : LateUpdateSystem
    {
        private Filter _requests;

        private Stash<ColliderComponent> _boxColliderComponents;
        private Stash<StaticColliderTag> _staticColliderTags;
        private Stash<TriggerColliderTag> _triggerTags;
        private Stash<RigidbodyComponent> _rigidbodyComponents;
        private Stash<CollisionEventsComponent> _collisionEventsComponents;
        
        public override void OnAwake()
        {
            _requests = World.Filter.With<RemoveColliderRequest>().Build();

            _boxColliderComponents = World.GetStash<ColliderComponent>();
            _staticColliderTags = World.GetStash<StaticColliderTag>();
            _triggerTags = World.GetStash<TriggerColliderTag>();
            _rigidbodyComponents = World.GetStash<RigidbodyComponent>();
            _collisionEventsComponents = World.GetStash<CollisionEventsComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var entity in _requests)
            {
                if (_boxColliderComponents.Has(entity))
                    _boxColliderComponents.Remove(entity);
                
                if (_staticColliderTags.Has(entity))
                    _staticColliderTags.Remove(entity);
                
                if (_triggerTags.Has(entity))
                    _triggerTags.Remove(entity);
                
                if (_rigidbodyComponents.Has(entity))
                    _rigidbodyComponents.Remove(entity);
                
                if (_collisionEventsComponents.Has(entity))
                    _collisionEventsComponents.Remove(entity);
            }
        }
    }
}