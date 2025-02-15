using System.Runtime.CompilerServices;
using Scellecs.Morpeh.Transform.Components;
using Unity.Mathematics;

namespace Scellecs.Morpeh
{
    public static class TransformComponentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 LocalTRS(in this TransformComponent component)
        {
            return float4x4.TRS(component.LocalPosition, component.LocalRotation, component.LocalScale);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Position(in this TransformComponent component) => Position(component.LocalToWorld);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Position(float4x4 localToWorld) => localToWorld.c3.xyz;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Translate(ref this TransformComponent component, float3 translation)
        {
            component.LocalPosition += translation;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPosition(ref this TransformComponent component, float3 worldPosition)
        {
            component.LocalPosition += (worldPosition - component.LocalToWorld.c3.xyz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion Rotation(in this TransformComponent component) => Rotation(component.LocalToWorld);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion Rotation(float4x4 localToWorld)
        {
            float3x3 rotMatrix = new float3x3(localToWorld);

            float3 scale = new float3(
                math.length(rotMatrix.c0), 
                math.length(rotMatrix.c1), 
                math.length(rotMatrix.c2)
            );

            rotMatrix.c0 /= scale.x;
            rotMatrix.c1 /= scale.y;
            rotMatrix.c2 /= scale.z;
            
            return new quaternion(rotMatrix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rotate(ref this TransformComponent component, quaternion rotation)
        {
            component.LocalRotation = math.mul(component.LocalRotation, rotation);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRotation(ref this TransformComponent component, quaternion worldRotation)
        {
            var delta = math.mul(worldRotation, math.conjugate(component.Rotation()));
            component.Rotate(delta);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Scale(in this TransformComponent component) => Scale(component.LocalToWorld);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Scale(float4x4 localToWorld)
        {
            return new float3(
                math.length(localToWorld.c0.xyz),
                math.length(localToWorld.c1.xyz),
                math.length(localToWorld.c2.xyz));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateLocalToWorld(ref this TransformComponent component)
        {
            var localTRS = component.LocalTRS();
            component.LocalToWorld = math.mul(component.ParentLocalToWorld, localTRS);
        }
    }
}
