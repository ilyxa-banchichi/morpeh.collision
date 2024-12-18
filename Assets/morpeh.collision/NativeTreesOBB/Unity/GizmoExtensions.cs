using UnityEngine;
using Unity.Mathematics;

namespace NativeTrees.Unity
{
    public static class GizmoExtensions
    {
        public static void DrawAABB(AABB aabb, Color color)
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = color;
            
            Gizmos.DrawWireCube(aabb.Center, aabb.Size);
            
            Gizmos.color = oldColor;
        }
        
        public static void DrawOBB(OBB obb, Color color)
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = color;

            float3[] vertices = new float3[8];
            float3 halfX = obb.X * obb.Extents.x;
            float3 halfY = obb.Y * obb.Extents.y;
            float3 halfZ = obb.Z * obb.Extents.z;

            vertices[0] = obb.Center - halfX - halfY - halfZ; // (-X, -Y, -Z)
            vertices[1] = obb.Center + halfX - halfY - halfZ; // (+X, -Y, -Z)
            vertices[2] = obb.Center + halfX + halfY - halfZ; // (+X, +Y, -Z)
            vertices[3] = obb.Center - halfX + halfY - halfZ; // (-X, +Y, -Z)
            vertices[4] = obb.Center - halfX - halfY + halfZ; // (-X, -Y, +Z)
            vertices[5] = obb.Center + halfX - halfY + halfZ; // (+X, -Y, +Z)
            vertices[6] = obb.Center + halfX + halfY + halfZ; // (+X, +Y, +Z)
            vertices[7] = obb.Center - halfX + halfY + halfZ; // (-X, +Y, +Z)

            DrawLine(vertices[0], vertices[1]);
            DrawLine(vertices[1], vertices[2]);
            DrawLine(vertices[2], vertices[3]);
            DrawLine(vertices[3], vertices[0]);

            DrawLine(vertices[4], vertices[5]);
            DrawLine(vertices[5], vertices[6]);
            DrawLine(vertices[6], vertices[7]);
            DrawLine(vertices[7], vertices[4]);

            DrawLine(vertices[0], vertices[4]);
            DrawLine(vertices[1], vertices[5]);
            DrawLine(vertices[2], vertices[6]);
            DrawLine(vertices[3], vertices[7]);

            Gizmos.color = oldColor;
        }

        private static void DrawLine(float3 start, float3 end)
        {
            Gizmos.DrawLine(start, end);
        }
    }
}