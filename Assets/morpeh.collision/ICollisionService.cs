using NativeTrees;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Collider = NativeTrees.Collider;

namespace Scellecs.Morpeh.Collision
{
    public struct RaycastHit
    {
        public Entity Entity;
        public int Layer;
        public Collider Collider;
        public float3 Point;
        public float Distance;
        // ToDo: Normal
        // public float3 Normal;
    }
    
    public interface ICollisionService
    {
        void Initialize(World world);
        bool TryRaycastFromScreenPoint(Camera camera, Vector2 position, LayerMask mask, out RaycastHit hit, float distance = 1000f);
        bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = ~0);
        int OverlapSphereNonAlloc(float3 position, float range, NativeArray<EntityHolder<Entity>> colliders, int layerMask = ~0);

        int OverlapBoxNonAlloc(
            float3 position, float3 extents, quaternion orientation,
            NativeArray<EntityHolder<Entity>> colliders, int layerMask = ~0);
    }
}