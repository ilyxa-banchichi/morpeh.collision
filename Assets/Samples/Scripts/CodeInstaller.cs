using System;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Addons.Feature.Unity;
using Scellecs.Morpeh.Collision;
using Scellecs.Morpeh.Collision.Components;
using UnityEngine;

namespace Samples.Scripts
{
    public class CodeInstaller : BaseFeaturesInstaller
    {
        protected override void InitializeShared() { }

        protected override UpdateFeature[] InitializeUpdateFeatures()
        {
            return Array.Empty<UpdateFeature>();
        }

        protected override FixedUpdateFeature[] InitializeFixedUpdateFeatures()
        {
            return Array.Empty<FixedUpdateFeature>();
        }

        protected override LateUpdateFeature[] InitializeLateUpdateFeatures()
        {
            return new[]
            {
                new CollisionFeature()
            };
        }
        
        protected void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            foreach (var octree in defaultWorld.Filter.With<OctreeComponent>().Build())
            {
                ref var tree = ref octree.GetComponent<OctreeComponent>();
                
                Gizmos.color = Color.white;
                if (tree.DynamicRigidbodies.IsValid)
                    tree.DynamicRigidbodies.DrawGizmos();
                
                Gizmos.color = Color.yellow;
                if (tree.StaticRigidbodies.IsValid)
                    tree.StaticRigidbodies.DrawGizmos();
            }
        }
    }
}