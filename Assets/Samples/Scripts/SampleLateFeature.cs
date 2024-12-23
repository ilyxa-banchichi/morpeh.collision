using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Transform.Systems;
using Unity.IL2CPP.CompilerServices;

namespace Samples.Scripts
{
    public class SampleLateFeature : LateUpdateFeature
    {
        protected override void Initialize()
        {
            AddSystem(new UpdateUnityViewSystem());
        }
    }
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateUnityViewSystem : LateUpdateSystem
    {
        private Filter _entities;
        
        public override void OnAwake()
        {
            _entities = World.Filter
                .With<GameObjectComponent>()
                .With<TransformComponent>()
                .Build();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            foreach (var entity in _entities)
            {
                ref var transform = ref entity.GetComponent<TransformComponent>();
                ref var view = ref entity.GetComponent<GameObjectComponent>();
                view.Transform.position = transform.Position();
            }
        }
    }
}