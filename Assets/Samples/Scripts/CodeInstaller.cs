using System;
using NativeTrees;
using NativeTrees.Unity;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Addons.Feature;
using Scellecs.Morpeh.Addons.Feature.Unity;
using Scellecs.Morpeh.Collision;
using Scellecs.Morpeh.Collision.Components;
using UnityEngine;

namespace Samples.Scripts
{
    public unsafe class CodeInstaller : BaseFeaturesInstaller
    {
        protected override void InitializeShared() { }

        protected override UpdateFeature[] InitializeUpdateFeatures()
        {
            return new UpdateFeature[]
            {
                new SampleFeature()
            };
            return Array.Empty<UpdateFeature>();
        }

        protected override FixedUpdateFeature[] InitializeFixedUpdateFeatures()
        {
            return Array.Empty<FixedUpdateFeature>();
        }

        protected override LateUpdateFeature[] InitializeLateUpdateFeatures()
        {
            return new LateUpdateFeature[]
            {
                new CollisionFeature(),
                new HierarchyFeature(),
                new TransformFeature(),
                new SampleLateFeature(),
            };
        }
        
        protected void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            foreach (var octree in defaultWorld.Filter.With<OctreeComponent>().Build())
            {
                ref var tree = ref octree.GetComponent<OctreeComponent>();
                
                Gizmos.color = Color.white;
                // if (tree.DynamicColliders.IsValid)
                    tree.DynamicColliders.DrawGizmos();
                
                Gizmos.color = Color.yellow;
                // if (tree.StaticColliders.IsValid)
                    tree.StaticColliders.DrawGizmos();
                
                // Gizmos.color = Color.magenta;
                // if (tree.DynamicRigidbodies.IsValid)
                //     tree.DynamicTriggers.DrawGizmos();
                //
                // Gizmos.color = Color.cyan;
                // if (tree.StaticRigidbodies.IsValid)
                //     tree.StaticTriggers.DrawGizmos();
            }
            
            foreach (var entity in defaultWorld.Filter.With<ColliderComponent>().Build())
            {
                ref var collider = ref entity.GetComponent<ColliderComponent>();
                if (collider.WorldBounds.Type == ColliderType.Box)
                {
                    var obb = ColliderCastUtils.ToBoxColliderRef(collider.WorldBounds);
                    GizmoExtensions.DrawOBB(obb, Color.blue);
                }
                else if (collider.WorldBounds.Type == ColliderType.Sphere)
                {
                    var sphere = ColliderCastUtils.ToSphereColliderRef(collider.WorldBounds);
                    GizmoExtensions.DrawSphere(sphere, Color.blue);
                }
                else if (collider.WorldBounds.Type == ColliderType.Terrain)
                {
                    var terrain = ColliderCastUtils.ToTerrainColliderRef(collider.WorldBounds);
                    // GizmoExtensions.DrawTerrain(*terrain, Color.blue);
                }
            }
        }
    }
}