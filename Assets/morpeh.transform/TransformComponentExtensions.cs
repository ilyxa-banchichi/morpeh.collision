using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Scellecs.Morpeh
{
    public static class TransformComponentExtensions
    {
        public static float3 Position(in this TransformComponent component) => Position(component.LocalToWorld);
        public static float3 Position(float4x4 localToWorld) => localToWorld.c3.xyz;

        public static void SetPositionNative(ref this TransformComponent component, float3 position,
            NativeStash<TransformComponent> stash)
        {
            component.LocalToWorld.c3.xyz = position;

            if (component.Parent == default)
            {
                component.LocalPosition = position;
            }
            else
            {
                ref var pt = ref stash.Get(component.Parent);
                float4x4 worldToLocal = math.inverse(pt.LocalToWorld);
                component.LocalPosition = math.mul(worldToLocal, new float4(position, 1)).xyz;
            }
        }

        [BurstDiscard]
        public static void SetPosition(ref this TransformComponent component, float3 position)
        {
            component.LocalToWorld.c3.xyz = position;

            if (component.Parent == default)
            {
                component.LocalPosition = position;
            }
            else
            {
                ref var pt = ref TransformCache.Stash.Get(component.Parent);
                float4x4 worldToLocal = math.inverse(pt.LocalToWorld);
                component.LocalPosition = math.mul(worldToLocal, new float4(position, 1)).xyz;
            }
        }

        public static quaternion Rotation(in this TransformComponent component) => Rotation(component.LocalToWorld);
        public static quaternion Rotation(float4x4 localToWorld)
        {
            return new quaternion(math.orthonormalize(new float3x3(localToWorld)));
        }

        public static float3 Scale(in this TransformComponent component) => Scale(component.LocalToWorld);
        public static float3 Scale(float4x4 localToWorld)
        {
            return new float3(
                math.length(localToWorld.c0.xyz),
                math.length(localToWorld.c1.xyz),
                math.length(localToWorld.c2.xyz));
        }

        [BurstDiscard]
        public static void ChangeParent(ref this TransformComponent component, Entity parent)
        {
            if (parent == default)
            {
                component.Parent = default;
                component.LocalPosition = component.Position();
                component.LocalRotation = component.Rotation();
                component.LocalScale = component.Scale();
                component.LocalToWorld = float4x4.TRS(component.LocalPosition, component.LocalRotation, component.LocalScale);
            }
            else
            {
                component.Parent = parent;
                ref var pt = ref TransformCache.Stash.Get(parent);
                var trs = math.mul(math.inverse(pt.LocalToWorld), component.LocalToWorld);

                component.LocalPosition = Position(trs);
                component.LocalRotation = Rotation(trs);
                component.LocalScale = Scale(trs);
                component.LocalToWorld = float4x4.TRS(component.LocalPosition, component.LocalRotation, component.LocalScale);
            }
        }

        [BurstDiscard]
        public static void DestroyHierarchy(in this TransformComponent component)
        {
            foreach (var child in component.Children)
            {
                if (TransformCache.Stash.Has(child) &&
                    !TransformCache.Stash.Get(child).IgnoreParentDestroyed)
                {
                    TransformCache.World.RemoveEntity(child);
                }
            }
        }

        [BurstDiscard]
        public static void GetAllChild(in this TransformComponent component, ref NativeList<Entity> list)
        {
            var stash = TransformCache.Stash.AsNative();
            component.GetAllChildNative(ref list, stash);
        }

        public static void GetAllChildNative(in this TransformComponent component, ref NativeList<Entity> list,
            NativeStash<TransformComponent> stash)
        {
            var queue = new NativeQueue<Entity>(Allocator.Temp);

            for (int i = 0; i < component.Children.Length; i++)
                queue.Enqueue(component.Children[i]);

            list.AddRange(component.Children.AsArray());

            while (!queue.IsEmpty())
            {
                var child = queue.Dequeue();
                ref var c = ref stash.Get(child);

                for (int i = 0; i < c.Children.Length; i++)
                    queue.Enqueue(c.Children[i]);

                list.AddRange(c.Children.AsArray());
            }

            queue.Dispose();
        }
    }
}
