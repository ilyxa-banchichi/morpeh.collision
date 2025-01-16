using TriInspector;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class TestTerrain : MonoBehaviour
{
    [SerializeField]
    private Transform _point;
    
    public TerrainData terrainData;
    
    [ShowInInspector]
    public NativeTrees.TerrainCollider world;

    [Button]
    protected void OnValidate()
    {
        if (terrainData == null)
            terrainData = GetComponent<TerrainCollider>().terrainData;
        
        world = new NativeTrees.TerrainCollider();
        world.Width = terrainData.heightmapResolution;
        world.Height = terrainData.heightmapResolution;
        world.ScaleX = terrainData.size.x / world.Width;
        world.ScaleZ = terrainData.size.z / world.Height;
        world.Translation = transform.position;
    
        if (world.HeightMap.IsCreated)
            world.HeightMap.Dispose();
        world.HeightMap = new NativeArray<float>(world.Width * world.Height, Allocator.Persistent);
            
        world.MinHeight = float.MaxValue;
        world.MaxHeight = float.MinValue;
    
        // Заполняем высотную карту
        for (int z = 0; z < world.Height; z++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                var height = terrainData.GetHeight(x, z);
                world.HeightMap[z * world.Width + x] = height;
                world.MinHeight = math.min(height, world.MinHeight);
                world.MaxHeight = math.max(height, world.MaxHeight);
            }
        }
    }
    
    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        // for (int z = 0; z < world.Height; z++)
        // {
        //     for (int x = 0; x < world.Width; x++)
        //     {
        //         var pos = new float3(x * world.ScaleX, 0f, z * world.ScaleZ) + world.Translation;
        //         pos.y = world.GetHeightAtIndex(x, z);
        //         Gizmos.DrawCube(pos, new float3(0.1f));
        //     }
        // }
    
        var position = _point.position;
        
        // Gizmos.color = Color.red;
        // position.y = world.GetInterpolatedHeight(position.x, position.z);
        // Gizmos.DrawSphere(position, 0.1f);

        float3[] points = new[]
        {
            new float3(_point.position.x + 0.5f, _point.position.y, _point.position.z),
            new float3(_point.position.x, _point.position.y, _point.position.z + 0.5f),
            new float3(_point.position.x, _point.position.y, _point.position.z - 0.5f),
            new float3(_point.position.x - 0.5f, _point.position.y, _point.position.z),
            new float3(_point.position.x, _point.position.y + 0.5f, _point.position.z),
            new float3(_point.position.x, _point.position.y - 0.5f, _point.position.z),
        };
        
        var maxDepth = 0f;
        int sampleCount = 3;  // Можем увеличить количество проверок для точности
        for (int i = 0; i < points.Length; i++)
        {
            // Точка на окружности сферы
            float angle = math.radians(i * (360f / sampleCount));
            // float x = _point.position.x + 0.5f * math.cos(angle);
            // float z = _point.position.z + 0.5f * math.sin(angle);

            float x = points[i].x;
            float z = points[i].z;
            Gizmos.DrawSphere(points[i], 0.1f);

            // Получаем высоту террейна в этой точке
            float height = world.GetInterpolatedHeight(x, z);
            //Gizmos.DrawSphere(new Vector3(x, height, z), 0.1f);

            if (points[i].y < height)
            {
                var depth = height - points[i].y;
                maxDepth = math.max(depth, maxDepth);
            }
        }
        
        _point.position = new Vector3(_point.position.x, _point.position.y + maxDepth, _point.position.z);
    }
}
