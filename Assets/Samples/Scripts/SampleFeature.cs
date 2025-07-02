using NativeTrees;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Collision;
using Scellecs.Morpeh.Transform.Components;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Scripts
{
    public class SampleFeature : UpdateFeature
    {
        private ICollisionService _collisionService;

        public SampleFeature(ICollisionService collisionService)
        {
            _collisionService = collisionService;
        }
        
        protected override void Initialize()
        {
            AddSystem(new InputSystem());
            AddSystem(new TestOverlapQuerySystem(_collisionService));
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class InputSystem : UpdateSystem
    {
        private Filter _player;
        
        public override void OnAwake()
        {
            _player = World.Filter
                .With<PlayerTag>()
                .With<TransformComponent>()
                .Build();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            var inputY = Input.GetAxis("Vertical");
            var inputX = Input.GetAxis("Horizontal");

            foreach (var player in _player)
            {
                ref var transform = ref player.GetComponent<TransformComponent>();
                transform.LocalPosition += new float3(inputX, -.3f, inputY) * deltaTime * 3f;
                transform.UpdateLocalToWorld();
            }
        }
    }
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TestOverlapQuerySystem : UpdateSystem
    {
        private const int Range = 5;
        
        private ICollisionService _collisionService;
        private Filter _player;
        private NativeArray<EntityHolder<Entity>> _array;

        public TestOverlapQuerySystem(ICollisionService collisionService)
        {
            _collisionService = collisionService;
        }

        public override void OnAwake()
        {
            _player = World.Filter
                .With<PlayerTag>()
                .With<TransformComponent>()
                .Build();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            foreach (var player in _player)
            {
                ref var transform = ref player.GetComponent<TransformComponent>();
                if (!_array.IsCreated)
                    _array = new NativeArray<EntityHolder<Entity>>(100, Allocator.Persistent);
            
                var count = CodeInstaller.CollisionService.OverlapSphereNonAlloc(transform.Position(), Range, _array);

                for (int i = 0; i < count; i++)
                {
                    var entity = _array[i].Entity;
                    if (entity.IsNullOrDisposed()) continue;
                    
                    if (entity.Has<GameObjectComponent>())
                    {
                        ref var go = ref entity.GetComponent<GameObjectComponent>();
                        var renderer = go.Transform.GetComponentInChildren<Renderer>();
                        if (!renderer) continue;
                        
                        var material = renderer.material;
                        material.color = Color.blue;
                    }
                }
            }
        }
    }
}
