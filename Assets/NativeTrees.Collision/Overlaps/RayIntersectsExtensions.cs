using Unity.Mathematics;

namespace NativeTrees
{
    public static class RayIntersectsExtensions
    {
        public static bool IntersectsRay(this CapsuleCollider capsuleCollider, in PrecomputedRay ray, out float tMin)
        {
            tMin = float.MinValue;
            return false;
        }
        
        public static bool IntersectsRay(this TerrainCollider terrainCollider, in PrecomputedRay ray, out float tMin)
        {
            tMin = float.MinValue;
            
            float maxDist = 1000.0f;
            float stepSize = terrainCollider.ScaleX * 0.5f;
            
            float3 position = ray.origin;
            float prevT = 0.0f;
            float prevY = position.y;
    
            for (float t = 0; t < maxDist; t += stepSize)
            {
                position = ray.origin + ray.dir * t;
                float height = terrainCollider.GetInterpolatedHeight(position.x, position.z);
                
                if (position.y <= height)
                {
                    float3 lastPosition = ray.origin + ray.dir * prevT;
                    float lastHeight = terrainCollider.GetInterpolatedHeight(lastPosition.x, lastPosition.z);
                    float lastDeltaY = prevY - lastHeight;
                    float deltaY = position.y - height;
                    float alpha = lastDeltaY / (lastDeltaY - deltaY);
                    tMin = prevT * (1 - alpha) + t * alpha;
                    
                    return true;
                }
        
                prevT = t;
                prevY = position.y;
            }
    
            return false;
        }
        
        public static bool IntersectsRay(this SphereCollider sphereCollider, in PrecomputedRay ray, out float tMin)
        {
            tMin = float.MinValue;
    
            float3 oc = ray.origin - sphereCollider.Center;
            float a = math.dot(ray.dir, ray.dir);
            float b = 2.0f * math.dot(oc, ray.dir);
            float c = math.dot(oc, oc) - sphereCollider.Radius * sphereCollider.Radius;
    
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return false;
    
            float sqrtD = math.sqrt(discriminant);
            float t0 = (-b - sqrtD) / (2.0f * a);
            float t1 = (-b + sqrtD) / (2.0f * a);
    
            if (t0 > t1)
                (t0, t1) = (t1, t0);
    
            if (t1 < 0)
                return false;
    
            tMin = t0 >= 0 ? t0 : t1;
            return true;
        }
        
        public static bool IntersectsRay(this BoxCollider boxCollider, in PrecomputedRay ray, out float tMin)
        {
            tMin = float.MinValue;
            float tMax = float.MaxValue;

            float3 p = boxCollider.Center - ray.origin;

            for (int i = 0; i < 3; i++)
            {
                float3 axis = i == 0 ? boxCollider.X : (i == 1 ? boxCollider.Y : boxCollider.Z);
                float extent = i == 0 ? boxCollider.Extents.x : (i == 1 ? boxCollider.Extents.y : boxCollider.Extents.z);

                float e = math.dot(axis, p);
                float f = math.dot(axis, ray.dir);

                if (math.abs(f) > 1e-6f)
                {
                    float t1 = (e - extent) / f;
                    float t2 = (e + extent) / f;

                    if (t1 > t2)
                        (t1, t2) = (t2, t1);

                    tMin = math.max(tMin, t1);
                    tMax = math.min(tMax, t2);

                    if (tMin > tMax)
                        return false;
                }
                else
                {
                    if (-extent > e || e > extent)
                        return false;
                }
            }

            return tMin >= 0;
        }
    }
}