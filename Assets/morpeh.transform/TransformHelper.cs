using Scellecs.Morpeh.Collections;
using Scellecs.Morpeh.Providers;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Scellecs.Morpeh
{
    public static class TransformHelper
    {
        public static void InitializeTransformComponent(GameObject gameObject)
        {
            ApplyUnityTransformToComponent(gameObject);
            SetHierarchy(gameObject);
            var provider = gameObject.GetComponent<HierarchyCodeUniversalProvider>();
            provider.Initialized = true;
        }

        public static void SetHierarchy(GameObject gameObject)
        {
            if (!TryGetEntityWithTransform(gameObject, out var entity)) return;
            var transform = gameObject.transform;

            ref var cTransform = ref entity.GetComponent<TransformComponent>();

            var childs = transform.GetComponentsInChildren<HierarchyCodeUniversalProvider>();
            cTransform.Children = new NativeList<Entity>(childs.Length, Allocator.Persistent);
            
            for (int i = 1; i < childs.Length; i++)
            {
                var child = childs[i];
                InitializeTransformComponent(child.gameObject);
            }

            for (int i = 1; i < childs.Length; i++)
            {
                var child = childs[i];
                child.Entity.SetParent(entity);
            }
        }

        public static void ApplyUnityTransformToComponent(GameObject gameObject, ref TransformComponent cTransform)
        {
            var transform = gameObject.transform;

            cTransform.LocalPosition = transform.position;
            cTransform.LocalRotation = transform.rotation;
            cTransform.LocalScale = transform.lossyScale;

            cTransform.Parent = default;
            cTransform.LocalToWorld = float4x4.TRS(cTransform.LocalPosition, cTransform.LocalRotation,
                cTransform.LocalScale);
        }

        private static void ApplyUnityTransformToComponent(GameObject gameObject)
        {
            if (!TryGetEntityWithTransform(gameObject, out var entity))
                return;

            ref var cTransform = ref entity.GetComponent<TransformComponent>();
            ApplyUnityTransformToComponent(gameObject, ref cTransform);

        }

        private static bool TryGetEntityWithTransform(GameObject gameObject, out Entity entity)
        {
            entity = default;
            if (EntityProvider.map.TryGetValue(gameObject.GetInstanceID(), out var item))
            {
                 entity = item.entity;
                 if (entity.Has<TransformComponent>())
                     return true;
            }

            return false;
        }
    }
}
