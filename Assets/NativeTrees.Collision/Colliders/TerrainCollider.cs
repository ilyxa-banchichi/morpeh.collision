using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace NativeTrees
{
    public unsafe struct TerrainCollider : IEquatable<TerrainCollider>, IDisposable
    {
        public float* HeightMapPtr;
        public NativeArray<float> HeightMap;  // Массив высот
        public int Width;  // Ширина террейна (по оси X)
        public int Height;  // Высота террейна (по оси Z)
        public float ScaleX;  // Масштаб по оси X
        public float ScaleZ;  // Масштаб по оси Z
        public float MinHeight;  // Минимальная высота террейна
        public float MaxHeight;  // Максимальная высота террейна
        public float3 Translation;

        public void Dispose()
        {
            if (HeightMap.IsCreated)
            {
                HeightMap.Dispose();
                HeightMapPtr = null;
            }
        }

        public bool Equals(TerrainCollider other)
        {
            return HeightMap.Equals(other.HeightMap) && Width == other.Width && Height == other.Height && ScaleX.Equals(other.ScaleX) && ScaleZ.Equals(other.ScaleZ) && MinHeight.Equals(other.MinHeight) && MaxHeight.Equals(other.MaxHeight);
        }

        public override bool Equals(object obj)
        {
            return obj is TerrainCollider other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(HeightMap, Width, Height, ScaleX, ScaleZ, MinHeight, MaxHeight);
        }
    }

    public static unsafe class TerrainColliderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetHeightAtIndex(this TerrainCollider collider, int ix, int iz)
        {
            ix = math.clamp(ix, 0, collider.Width - 1);
            iz = math.clamp(iz, 0, collider.Height - 1);
            return collider.HeightMap[iz * collider.Width + ix] + collider.Translation.y;
        }
        
        public static float GetInterpolatedHeight(this TerrainCollider collider, float x, float z)
        {
            // Преобразование мировых координат в локальные
            float localX = (x - collider.Translation.x) / (collider.ScaleX * collider.Width);
            float localZ = (z - collider.Translation.z) / (collider.ScaleZ * collider.Height);

            // Преобразование в координаты сетки (индексы)
            float gridX = localX * (collider.Width - 1);
            float gridZ = localZ * (collider.Height - 1);

            // Ограничиваем координаты в пределах массива
            gridX = math.clamp(gridX, 0, collider.Width - 1);
            gridZ = math.clamp(gridZ, 0, collider.Height - 1);

            // Индексы соседних точек
            int x0 = (int)math.floor(gridX);
            int z0 = (int)math.floor(gridZ);
            int x1 = math.min(x0 + 1, collider.Width - 1);
            int z1 = math.min(z0 + 1, collider.Height - 1);

            // Фракции для интерполяции
            float tx = gridX - x0;
            float tz = gridZ - z0;

            // Значения высот в четырех углах
            float h00 = collider.HeightMapPtr[z0 * collider.Width + x0];
            float h10 = collider.HeightMapPtr[z0 * collider.Width + x1];
            float h01 = collider.HeightMapPtr[z1 * collider.Width + x0];
            float h11 = collider.HeightMapPtr[z1 * collider.Width + x1];

            // Интерполяция по X
            float hx0 = math.lerp(h00, h10, tx);
            float hx1 = math.lerp(h01, h11, tx);

            // Интерполяция по Z
            float h = math.lerp(hx0, hx1, tz);

            return h + collider.Translation.y;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetHeightAt(this TerrainCollider collider, float x, float z)
        {
            x -=  collider.Translation.x;
            z -=  collider.Translation.z;
            int ix = (int)math.round(x / collider.ScaleX);
            int iz = (int)math.round(z / collider.ScaleZ);

            return collider.GetHeightAtIndex(ix, iz);
        }
    }
}