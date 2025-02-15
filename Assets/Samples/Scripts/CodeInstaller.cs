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
        public static ICollisionService CollisionService;

        protected override void InitializeShared()
        {
            CollisionService = new CollisionService();
        }

        protected override UpdateFeature[] InitializeUpdateFeatures()
        {
            return new UpdateFeature[]
            {
                new SampleFeature(CollisionService)
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
                new CollisionFeature(CollisionService),
                new HierarchyFeature(),
                new TransformFeature(),
                new SampleLateFeature(),
            };
            return Array.Empty<LateUpdateFeature>();
        }
        
        protected void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            foreach (var octree in defaultWorld.Filter.With<OctreeComponent>().Build())
            {
                ref var tree = ref octree.GetComponent<OctreeComponent>();
                
                Gizmos.color = Color.black;
                if (tree.DynamicColliders.IsValid())
                    tree.DynamicColliders.DrawGizmos();
                
                Gizmos.color = Color.blue;
                if (tree.StaticColliders.IsValid())
                    tree.StaticColliders.DrawGizmos();
            }
            
            foreach (var entity in defaultWorld.Filter.With<ColliderComponent>().Build())
            {
                ref var collider = ref entity.GetComponent<ColliderComponent>();
                if (collider.WorldBounds.Type == ColliderType.Box)
                {
                    var obb = ColliderCastUtils.ToBoxColliderRef(collider.WorldBounds);
                    GizmoExtensions.DrawOBB(obb, Color.green);
                }
                else if (collider.WorldBounds.Type == ColliderType.Sphere)
                {
                    var sphere = ColliderCastUtils.ToSphereColliderRef(collider.WorldBounds);
                    GizmoExtensions.DrawSphere(sphere, Color.green);
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