using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

// https://bartvandesande.nl
// https://github.com/bartofzo

namespace NativeTrees
{
    public partial struct NativeOctree<T> : INativeDisposable
        where T : unmanaged 
    {
        /// <summary>
        /// Perform a raycast against the octree
        /// </summary>
        /// <param name="ray">Input ray</param>
        /// <param name="hit">The resulting hit</param>
        /// <param name="intersecter">Delegate to compute ray intersections against the objects or AABB's</param>
        /// <param name="maxDistance">Max distance from the ray's origin a hit may occur</param>
        /// <typeparam name="U">Type of intersecter</typeparam>
        /// <returns>True when a hit has occured</returns>
        public bool Raycast<U>(Ray ray, out OctreeRaycastHit<T> hit, U intersecter = default, float maxDistance = float.PositiveInfinity) where U : struct, IOctreeRayIntersecter<T>
        {
            var computedRay = new PrecomputedRay(ray);

            // check if ray even hits the boundary, and if so, we use the intersectin point to transpose our ray
            if (!bounds.IntersectsRay(computedRay.origin, computedRay.invDir, out float tMin) || tMin > maxDistance)
            {
                hit = default;
                return false;
            }
            
            maxDistance -= tMin;
            var rayPos = computedRay.origin + computedRay.dir * tMin;
            
            // Note: transpose computed ray to boundary and go
            return RaycastNext(
                ray: new PrecomputedRay(computedRay, rayPos), 
                nodeId: 1, 
                extentsBounds: new ExtentsBounds(boundsCenter, boundsExtents), 
                hit: out hit, 
                intersecter: ref intersecter, 
                maxDistance: maxDistance,
            parentDepth: 0);
        }
        
        bool RaycastNext<U>(
            in PrecomputedRay ray,
            uint nodeId, in ExtentsBounds extentsBounds,
            out OctreeRaycastHit<T> hit, 
            ref U intersecter, 
            int parentDepth, float maxDistance) 
            where U : struct, IOctreeRayIntersecter<T>
        {
            parentDepth++;
            
            // Reference for the method used to determine the order of octants to visit
            // https://daeken.svbtle.com/a-stupidly-simple-fast-octree-traversal-for-ray-intersection
            
            // Compute the bounds of the parent node we're in, we use it to check if a plane intersection is valid
            // var parentBounds = ExtentsBounds.GetBounds(extentsBounds);
            
            // compute the plane intersections of YZ, XZ and XY
            float3 planeHits = PlaneHits(ray, extentsBounds.nodeCenter);
            
            // for our first (closest) octant, it must be the position the ray entered the parent node
            int octantIndex = PointToOctantIndex(ray.origin, extentsBounds.nodeCenter);
            float3 octantRayIntersection = ray.origin;
            float octantDistance = 0;

            for (int i = 0; i < 4; i++)
            {
                uint octantId = GetOctantId(nodeId, octantIndex);
                if (nodes.TryGetValue(octantId, out int objectCount) && 
                    Raycast(
                        ray: new PrecomputedRay(ray, octantRayIntersection), 
                        nodeId: octantId, 
                        extentsBounds: ExtentsBounds.GetOctant(extentsBounds, octantIndex), 
                        objectCount: objectCount,
                        hit: out hit,
                        intersecter: ref intersecter, 
                        maxDistance: maxDistance - octantDistance,
                        depth: parentDepth))
                {
                    return true;
                }

                // find next octant to test:
                float closestDistance = maxDistance; //float.PositiveInfinity;
                int closestPlaneIndex = -1;
                
                for (int j = 0; j < 3; j++)
                {
                    float t = planeHits[j];
                    if (t > closestDistance || t < 0) continue; // negative t is backwards
                    
                    float3 planeRayIntersection = ray.origin + t * ray.dir;
                    //if (parentBounds.Contains(planeRayIntersection))
                    if (extentsBounds.Contains(planeRayIntersection))
                    {
                        octantRayIntersection = planeRayIntersection;
                        closestPlaneIndex = j;
                        closestDistance = octantDistance = t;
                    }
                }

                // No valid octant intersections left, bail
                if (closestPlaneIndex == -1)
                    break;
                
                // get next octant from plane index
                octantIndex ^= 1 << closestPlaneIndex;
                planeHits[closestPlaneIndex] = float.PositiveInfinity;
            }
            
            hit = default;
            return false;
        }
        
        bool Raycast<U>(in PrecomputedRay ray, 
            uint nodeId, 
            in ExtentsBounds extentsBounds, 
            int objectCount,
            out OctreeRaycastHit<T> hit,
            ref U intersecter, float maxDistance,
            int depth) where U : struct, IOctreeRayIntersecter<T>
        {
            // Are we in a leaf node?
            if (objectCount <= objectsPerNode || depth == maxDepth)
            {
                hit = default;
                float closest = maxDistance;
                bool didHit = false;

                if (objects.TryGetFirstValue(nodeId, out var wrappedObj, out var it))
                {
                    do
                    {
                        if (intersecter.IntersectRay(ray, wrappedObj.obj, wrappedObj.bounds, out float t) && t < closest)
                        {
                            closest = t;
                            hit.obj = wrappedObj.obj;
                            didHit = true;
                        }
                    } while (objects.TryGetNextValue(out wrappedObj, ref it));
                }
                
                if (didHit)
                {
                    hit.point = ray.origin + ray.dir * closest;
                    return extentsBounds.Contains(hit.point);
                }
                
                return false;
            }

            return RaycastNext(
                ray: ray, 
                nodeId: nodeId, 
                extentsBounds: extentsBounds, 
                hit: out hit, 
                intersecter: ref intersecter, 
                maxDistance: maxDistance,
                parentDepth: depth);
        }
        
        /// <summary>
        /// Computes ray plane intersections compute of YZ, XZ and XY respectively stored in xyz
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 PlaneHits(in PrecomputedRay ray, float3 nodeCenter) => (nodeCenter - ray.origin) * ray.invDir;
    }
}