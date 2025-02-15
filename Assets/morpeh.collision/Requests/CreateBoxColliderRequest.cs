using System;
using NativeTrees;
using TriInspector;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using BoxCollider = UnityEngine.BoxCollider;
using CapsuleCollider = UnityEngine.CapsuleCollider;
using SphereCollider = UnityEngine.SphereCollider;
using TerrainCollider = UnityEngine.TerrainCollider;

namespace Scellecs.Morpeh.Collision.Requests
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [System.Serializable]
    public struct CreateBoxColliderRequest : IComponent, IValidatableWithGameObject
    {
        public bool ManualSetupUnityCollider;
        
        [ShowIf(nameof(ManualSetupUnityCollider), true)]
        public UnityEngine.Collider ColliderObject;
        
        [ReadOnly, InlineProperty, HideLabel]
        public NewColliderData Data;
        
        [ReadOnly]
        public int Layer;
        
        [ReadOnly]
        public bool IsStatic;
        
        [ReadOnly]
        public bool IsTrigger;

        private bool IsTriggerOrStatic => IsStatic || IsTrigger;
        
        [Title("Rigidbody")]
        [ShowIf(nameof(IsTriggerOrStatic), false)]
        public int Weight;
        
        [ShowIf(nameof(IsTriggerOrStatic), false)]
        public bool3 FreezePosition;

        public void OnValidate(GameObject gameObject)
        {
            if (!ManualSetupUnityCollider)
                ColliderObject = gameObject.GetComponent<UnityEngine.Collider>();
            
            Layer = ColliderObject.gameObject.layer;
            IsStatic = ColliderObject.gameObject.isStatic;
            IsTrigger = ColliderObject.isTrigger;
            Weight = math.clamp(Weight, 1, int.MaxValue);
            Data = default;
            
            if (ColliderObject is BoxCollider boxCollider)
                Data = NewColliderData.CreateBoxData(boxCollider.center,boxCollider.size);
            else if (ColliderObject is SphereCollider sphereCollider)
                Data = NewColliderData.CreateSphereData(sphereCollider.center, sphereCollider.radius);
            else if (ColliderObject is CapsuleCollider capsuleCollider)
                Data = NewColliderData.CreateCapsuleData(capsuleCollider.center, capsuleCollider.radius, capsuleCollider.height);
            else if (ColliderObject is TerrainCollider terrainCollider)
                Data = NewColliderData.CreateTerrainData(terrainCollider.terrainData);
        }
    }

    [Serializable]
    public struct NewColliderData
    {
        [field: SerializeField]
        public ColliderType Type { get; private set; }

        [field: InlineProperty]
        [field: SerializeField]
        public float3 Center { get; private set; }
        
        [field: InlineProperty]
        [field: SerializeField]
        public float3 Size { get; private set; }
        
        [field: SerializeField]
        public float Radius { get; private set; }
        
        [field: SerializeField]
        public float Height { get; private set; }
        
        [field: SerializeField]
        public TerrainData TerrainData { get; private set; }

        private NewColliderData(
            ColliderType type,
            float3 center = default, 
            float3 size = default, 
            float radius = default, 
            float height = default, 
            TerrainData terrainData = default)
        {
            Type = type;
            Center = center;
            Size = size;
            Radius = radius;
            Height = height;
            TerrainData = terrainData;
        }

        public static NewColliderData CreateBoxData(float3 center, float3 size)
        {
            return new NewColliderData(ColliderType.Box, center: center, size: size);
        }

        public static NewColliderData CreateSphereData(float3 center, float radius)
        {
            return new NewColliderData(ColliderType.Sphere, center: center, radius: radius);
        }

        public static NewColliderData CreateCapsuleData(float3 center, float radius, float height)
        {
            return new NewColliderData(ColliderType.Capsule, center: center, radius: radius, height: height);
        }
        
        public static NewColliderData CreateTerrainData(TerrainData terrainData)
        {
            return new NewColliderData(ColliderType.Terrain, terrainData: terrainData);
        }
    }
}