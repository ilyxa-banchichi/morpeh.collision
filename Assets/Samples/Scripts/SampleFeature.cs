using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Addons.Systems;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Scripts
{
    public class SampleFeature : UpdateFeature
    {
        protected override void Initialize()
        {
            AddSystem(new InputSystem());
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
                transform.LocalPosition += new float3(inputX, 0f, inputY) * deltaTime * 3f;
            }
        }
    }
}
