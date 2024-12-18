using NativeTrees;
using NativeTrees.Unity;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Scripts
{
    public class DrawBBs : MonoBehaviour
    {
        public float3 Center;
        public float3 Size;

        protected void OnDrawGizmos()
        {
            var aabb = new AABB(
                Center + (float3)transform.position - Size * .5f,
                Center + (float3)transform.position + Size * .5f);

            var obb = new OBB(aabb, transform.rotation);
            
            GizmoExtensions.DrawAABB(aabb, Color.green);
            GizmoExtensions.DrawOBB(obb, Color.blue);
            GizmoExtensions.DrawAABB((AABB)obb, Color.red);
        }
    }
}