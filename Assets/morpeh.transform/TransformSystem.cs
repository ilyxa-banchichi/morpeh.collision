using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using Unity.Mathematics;

namespace Scellecs.Morpeh
{
    [BurstCompile]
    internal struct TransformSystemJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeFilter Entities;

        public NativeStash<TransformComponent> TransformComponents;

        public void Execute(int index)
        {
            var entity = Entities[index];
            ref var cTransform = ref TransformComponents.Get(entity);
            
            ValidateChildrenListCreated(ref cTransform);

            var localToWorld = float4x4.TRS(cTransform.LocalPosition, cTransform.LocalRotation, cTransform.LocalScale);
            var parentId = cTransform.Parent;

            while (parentId != default && TransformComponents.Has(parentId))
            {
                ref var pt = ref TransformComponents.Get(parentId);
                var parentLocalToWorld = float4x4.TRS(pt.LocalPosition, pt.LocalRotation, pt.LocalScale);
                localToWorld = math.mul(parentLocalToWorld, localToWorld);

                parentId = pt.Parent;
            }

            cTransform.LocalToWorld = localToWorld;
        }

        private void ValidateChildrenListCreated(ref TransformComponent cTransform)
        {
            if (!cTransform.Children.IsCreated)
                cTransform.Children = new NativeList<Entity>(1, Allocator.Persistent);
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class TransformSystem : LateUpdateSystem
    {
        private Filter _transforms;
        private Stash<TransformComponent> _transformComponents;

        public override void OnAwake()
        {
            _transforms = World.Filter.With<TransformComponent>().Build();
            _transformComponents = World.GetStash<TransformComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            var nativeFilter = _transforms.AsNative();
            var job = new TransformSystemJob()
            {
                Entities = nativeFilter,
                TransformComponents = _transformComponents.AsNative()
            };

            job.Schedule(nativeFilter.length, 64).Complete();
        }
    }
}
