using NativeTrees;
using NativeTrees.Unity;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Collision.Requests;
using TriInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Scripts
{
    public class TestEntity : MonoBehaviour
    {
        [InlineProperty, HideLabel]
        public CreateBoxColliderRequest Request;

        protected Entity entity;

        protected void Awake()
        {
            entity = World.Default.CreateEntity();
            entity.SetComponent(Request);
        }

        protected void Update()
        {
            if (entity == default) return;
            if (!entity.Has<BoxColliderComponent>()) return;

            if (entity.Has<RigidbodyComponent>())
            {
                ref var rigidbody = ref entity.GetComponent<RigidbodyComponent>();
                transform.position += (Vector3)rigidbody.Delta;
            }
            
            ref var c = ref entity.GetComponent<BoxColliderComponent>();
            float3 position = transform.position;
            quaternion rotation = transform.rotation;
            c.WorldBounds = new OBB(aabb: c.OriginalBounds, position: position, rotation: rotation);
        }

        protected void OnDestroy()
        {
            World.Default.RemoveEntity(entity);
            entity = default;
        }
        
        protected void OnValidate()
        {
            Request.OnValidate(gameObject);
        }
        
        protected void OnDrawGizmos()
        {
            if (Application.isPlaying) return;
            
            var aabb = new AABB(
                Request.Center + (float3)transform.position - Request.Size * .5f,
                Request.Center + (float3)transform.position + Request.Size * .5f);

            var obb = new OBB(aabb, transform.rotation);
            
            // GizmoExtensions.DrawAABB(aabb, Color.green);
            GizmoExtensions.DrawOBB(obb, Color.blue);
            GizmoExtensions.DrawAABB((AABB)obb, Color.red);
        }
    }
}