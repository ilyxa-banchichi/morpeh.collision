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
        
        public static void DrawSphere(SphereCollider sphere, Color color)
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = color;
            
            Gizmos.DrawWireSphere(sphere.Center, sphere.Radius);
            
            Gizmos.color = oldColor;
        }
        
        public static void DrawTerrain(TerrainCollider terrain, Color color)
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = color;

            for (int z = 0; z < terrain.Height; z++)
            {
                for (int x = 0; x < terrain.Width; x++)
                {
                    var pos = new float3(x * terrain.ScaleX, 0f, z * terrain.ScaleZ) + terrain.Translation;
                    pos.y = terrain.GetHeightAtIndex(x, z);
                    Gizmos.DrawCube(pos, new float3(0.1f));
                }
            }
            
            Gizmos.color = oldColor;
        }
        
        public static void DrawOBB(BoxCollider boxCollider, Color color)
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = color;

            float3[] vertices = new float3[8];
            float3 halfX = boxCollider.X * boxCollider.Extents.x;
            float3 halfY = boxCollider.Y * boxCollider.Extents.y;
            float3 halfZ = boxCollider.Z * boxCollider.Extents.z;

            vertices[0] = boxCollider.Center - halfX - halfY - halfZ; // (-X, -Y, -Z)
            vertices[1] = boxCollider.Center + halfX - halfY - halfZ; // (+X, -Y, -Z)
            vertices[2] = boxCollider.Center + halfX + halfY - halfZ; // (+X, +Y, -Z)
            vertices[3] = boxCollider.Center - halfX + halfY - halfZ; // (-X, +Y, -Z)
            vertices[4] = boxCollider.Center - halfX - halfY + halfZ; // (-X, -Y, +Z)
            vertices[5] = boxCollider.Center + halfX - halfY + halfZ; // (+X, -Y, +Z)
            vertices[6] = boxCollider.Center + halfX + halfY + halfZ; // (+X, +Y, +Z)
            vertices[7] = boxCollider.Center - halfX + halfY + halfZ; // (-X, +Y, +Z)

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