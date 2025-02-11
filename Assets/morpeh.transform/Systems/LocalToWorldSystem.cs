using Scellecs.Morpeh.Addons.Systems;
using Scellecs.Morpeh.Hierarchy.Systems;
using Scellecs.Morpeh.Native;
using Scellecs.Morpeh.Transform.Components;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace Scellecs.Morpeh.Transforms.Systems
{
    public sealed class LocalToWorldSystem : LateUpdateSystem
    {
        private Filter rootFilter;
        private Filter parentFilter;

        private Stash<TransformComponent> transformStash;
        private Stash<ParentComponent> parentStash;
        private Stash<ChildComponent> childStash;

        public override void OnAwake()
        {
            rootFilter = World.Filter
                .With<TransformComponent>()
                .Without<ParentComponent>()
                .Build();

            parentFilter = World.Filter
                .With<TransformComponent>()
                .With<ChildComponent>()
                .Without<ParentComponent>()
                .Build();

            transformStash = World.GetStash<TransformComponent>();
            parentStash = World.GetStash<ParentComponent>();
            childStash = World.GetStash<ChildComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            var rootFilterNative = rootFilter.AsNative();
            var parentFilterNative = parentFilter.AsNative();

            var transformStashNative = transformStash.AsNative();
            var parentStashNative = parentStash.AsNative();
            var childStashNative = childStash.AsNative();

            var rootJob = new ComputeRootLocalToWorldJob
            {
                rootFilter = rootFilterNative,
                transformStash = transformStashNative,
            }
            .ScheduleParallel(rootFilterNative.length, 32, default);

            var childJob = new ComputeChildLocalToWorldJob
            {
                parentFilter = parentFilterNative,
                transformStash = transformStashNative,
                parentStash = parentStashNative,
                childStash = childStashNative
            }
            .ScheduleParallel(parentFilterNative.length, 32, rootJob);

            childJob.Complete();
        }

        public void Dispose() { }
    }

    [BurstCompile]
    internal struct ComputeRootLocalToWorldJob : IJobFor
    {
        public NativeFilter rootFilter;
        public NativeStash<TransformComponent> transformStash;

        public void Execute(int index)
        {
            var entityId = rootFilter[index];
            ref TransformComponent transform = ref transformStash.Get(entityId);

            transform.LocalToWorld = float4x4.TRS(
                transform.LocalPosition,
                transform.LocalRotation,
                transform.LocalScale
            );
        }
    }

    [BurstCompile]
    internal struct ComputeChildLocalToWorldJob : IJobFor
    {
        public NativeFilter parentFilter;
        public NativeStash<TransformComponent> transformStash;
        public NativeStash<ParentComponent> parentStash;
        public NativeStash<ChildComponent> childStash;

        public void Execute(int index)
        {
            var entityId = parentFilter[index];
            var children = childStash.Get(entityId);
            ref TransformComponent transform = ref transformStash.Get(entityId);

            for (int i = 0; i < children.Value.Length; i++)
            {
                ChildLocalToWorldFromTransformMatrix(transform.LocalToWorld, children.Value[i]);
            }
        }

        private void ChildLocalToWorldFromTransformMatrix(in float4x4 parentLocalToWorld, Entity childEntity)
        {
            ref TransformComponent transform = ref transformStash.Get(childEntity);
            var hasParent = parentStash.Has(childEntity);

            if (hasParent)
            {
                var trs = float4x4.TRS(
                    transform.LocalPosition,
                    transform.LocalRotation,
                    transform.LocalScale
                );
                var ltw = math.mul(parentLocalToWorld, trs);
                transform.LocalToWorld = ltw;
            }

            ChildComponent children = childStash.Get(childEntity, out bool hasChildren);

            if (hasChildren)
            {
                for (int i = 0; i < children.Value.Length; i++)
                {
                    ChildLocalToWorldFromTransformMatrix(transform.LocalToWorld, children.Value[i]);
                }
            }
        }
    }
}