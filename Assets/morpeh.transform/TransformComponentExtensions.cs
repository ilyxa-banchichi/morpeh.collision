using Scellecs.Morpeh.Transform.Systems;
using Unity.Mathematics;

namespace Scellecs.Morpeh
{
    public static class TransformComponentExtensions
    {
        public static float3 Position(in this TransformComponent component) => Position(component.LocalToWorld);
        public static float3 Position(float4x4 localToWorld) => localToWorld.c3.xyz;

        public static quaternion Rotation(in this TransformComponent component) => Rotation(component.LocalToWorld);
        public static quaternion Rotation(float4x4 localToWorld)
        {
            return new quaternion(math.orthonormalize(new float3x3(localToWorld)));
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
