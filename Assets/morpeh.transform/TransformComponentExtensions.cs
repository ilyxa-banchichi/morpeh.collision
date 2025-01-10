using Scellecs.Morpeh.Transform.Components;
using Unity.Mathematics;

namespace Scellecs.Morpeh
{
    public static class TransformComponentExtensions
    {
        public static float4x4 LocalTRS(in this TransformComponent component)
        {
            return float4x4.TRS(component.LocalPosition, component.LocalRotation, component.LocalScale);
        }
        
        public static float3 Position(in this TransformComponent component) => Position(component.LocalToWorld);
        public static float3 Position(float4x4 localToWorld) => localToWorld.c3.xyz;

        public static void Translate(ref this TransformComponent component, float3 translation)
        {
            component.LocalPosition += translation;
            component.LocalToWorld.c3.xyz += translation;
        }
        
        public static void SetPosition(ref this TransformComponent component, float3 worldPosition)
        {
            component.LocalPosition += (worldPosition - component.LocalToWorld.c3.xyz);
            component.LocalToWorld.c3.xyz = worldPosition;
        }

        public static quaternion Rotation(in this TransformComponent component) => Rotation(component.LocalToWorld);
        public static quaternion Rotation(float4x4 localToWorld)
        {
            return new quaternion(math.orthonormalize(new float3x3(localToWorld)));
        }

        public static void Rotate(ref this TransformComponent component, quaternion rotation)
        {
            component.LocalRotation = math.mul(component.LocalRotation, rotation);
            float4x4 deltaMatrix = new float4x4(rotation, float3.zero);
            component.LocalToWorld = math.mul(component.LocalToWorld, deltaMatrix);
        }
        
        public static void SetRotation(ref this TransformComponent component, quaternion worldRotation)
        {
            var delta = math.mul(worldRotation, math.conjugate(component.Rotation()));
            component.Rotate(delta);
        }
        
        public static float3 Scale(in this TransformComponent component) => Scale(component.LocalToWorld);
        public static float3 Scale(float4x4 localToWorld)
        {
            return new float3(
                math.length(localToWorld.c0.xyz),
                math.length(localToWorld.c1.xyz),
                math.length(localToWorld.c2.xyz));
        }
    }
}
