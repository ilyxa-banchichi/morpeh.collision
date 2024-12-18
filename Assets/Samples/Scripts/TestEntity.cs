using System;
using NativeTrees;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collision.Components;
using Scellecs.Morpeh.Collision.Requests;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Scripts
{
    public class TestEntity : MonoBehaviour
    {
        public float3 Center;
        public float3 Size;
        public bool IsStatic;

        private Entity _entity;

        protected void Awake()
        {
            _entity = World.Default.CreateEntity();
            ref var c = ref _entity.AddComponent<CreateBoxColliderRequest>();
            
            c.OnValidate(gameObject);

            c.Center = Center;
            c.Size = Size;
            c.Weight = 1;
            c.FreezePosition = false;
        }

        protected void Update()
        {
            if (_entity == default) return;
            if (!_entity.Has<BoxColliderComponent>()) return;
            
            ref var c = ref _entity.GetComponent<BoxColliderComponent>();
            float3 position = transform.position;
            quaternion rotation = transform.rotation;
            c.WorldBounds = new OBB(aabb: c.OriginalBounds, position: position, rotation: rotation);
        }

        protected void OnDestroy()
        {
            World.Default.RemoveEntity(_entity);
            _entity = default;
        }
    }
}