using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace NativeTrees
{
    public struct TerrainCollider : IEquatable<TerrainCollider>
    {
        public NativeArray<float> HeightMap;  // Массив высот
        public int Width;  // Ширина террейна (по оси X)
        public int Height;  // Высота террейна (по оси Z)
        public float ScaleX;  // Масштаб по оси X
        public float ScaleZ;  // Масштаб по оси Z
        public float MinHeight;  // Минимальная высота террейна
        public float MaxHeight;  // Максимальная высота террейна
        public float3 Translation;
        
        public float GetHeightAtIndex(int ix, int iz)
        {
            ix = math.clamp(ix, 0, Width - 1);
            iz = math.clamp(iz, 0, Height - 1);
            return HeightMap[iz * Width + ix] + Translation.y;
        }
        
        public float GetInterpolatedHeight(float x, float z)
        {
            // Преобразование мировых координат в локальные
            float localX = (x - Translation.x) / (ScaleX * Width);
            float localZ = (z - Translation.z) / (ScaleZ * Height);

            // Преобразование в координаты сетки (индексы)
            float gridX = localX * (Width - 1);
            float gridZ = localZ * (Height - 1);

            // Ограничиваем координаты в пределах массива
            gridX = math.clamp(gridX, 0, Width - 1);
            gridZ = math.clamp(gridZ, 0, Height - 1);

            // Индексы соседних точек
            int x0 = (int)math.floor(gridX);
            int z0 = (int)math.floor(gridZ);
            int x1 = math.min(x0 + 1, Width - 1);
            int z1 = math.min(z0 + 1, Height - 1);

            // Фракции для интерполяции
            float tx = gridX - x0;
            float tz = gridZ - z0;

            // Значения высот в четырех углах
            float h00 = HeightMap[z0 * Width + x0];
            float h10 = HeightMap[z0 * Width + x1];
            float h01 = HeightMap[z1 * Width + x0];
            float h11 = HeightMap[z1 * Width + x1];

            // Интерполяция по X
            float hx0 = math.lerp(h00, h10, tx);
            float hx1 = math.lerp(h01, h11, tx);

            // Интерполяция по Z
            float h = math.lerp(hx0, hx1, tz);

            return h;
        }
        
        public float GetHeightAt(float x, float z)
        {
            x -=  Translation.x;
            z -=  Translation.z;
            int ix = (int)math.round(x / ScaleX);
            int iz = (int)math.round(z / ScaleZ);

            return GetHeightAtIndex(ix, iz);
        }
        
        public static explicit operator AABB(TerrainCollider terrainCollider)
        {
            // Минимальные и максимальные значения по оси X и Z
            float3 min = new float3(0, terrainCollider.MinHeight, 0);
            float3 max = new float3(terrainCollider.ScaleX * (terrainCollider.Width - 1), terrainCollider.MaxHeight, terrainCollider.ScaleZ * (terrainCollider.Height - 1));

            // Возвращаем AABB для всего террейна
            return new AABB(min + terrainCollider.Translation, max + terrainCollider.Translation);
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
}