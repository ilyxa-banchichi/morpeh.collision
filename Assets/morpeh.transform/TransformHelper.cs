using Scellecs.Morpeh.Collections;
using Scellecs.Morpeh.Hierarchy;
using Scellecs.Morpeh.Providers;
using Scellecs.Morpeh.Transform.Components;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Scellecs.Morpeh
{
    public static class TransformHelper
    {
        public static void InitializeTransformComponent(GameObject gameObject)
        {
            SetHierarchy(gameObject);
            var provider = gameObject.GetComponent<HierarchyCodeUniversalProvider>();
            provider.Initialized = true;
        }
        
        public static void ApplyUnityTransformToComponent(GameObject gameObject, ref TransformComponent cTransform)
        {
            var transform = gameObject.transform;

            cTransform.LocalPosition = transform.position;
            cTransform.LocalRotation = transform.rotation;
            cTransform.LocalScale = transform.lossyScale;
            cTransform.LocalToWorld = float4x4.TRS(cTransform.LocalPosition, cTransform.LocalRotation, cTransform.LocalScale);
        }

        private static void SetHierarchy(GameObject gameObject)
        {
            if (!gameObject.transform.parent) return;
            
            var parentProvider = gameObject.transform.parent.GetComponentInParent<HierarchyCodeUniversalProvider>();
            if (parentProvider)
            {
                if (EntityProvider.map.TryGetValue(gameObject.GetInstanceID(), out var item))
                if (EntityProvider.map.TryGetValue(parentProvider.gameObject.GetInstanceID(), out var parentItem)) 
                {
                    item.entity.SetParent(parentItem.entity);
                }
            }
        }
    }
}
