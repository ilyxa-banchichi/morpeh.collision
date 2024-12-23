using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;

namespace NativeTrees
{
    public static class OBBOverlapExtensions
    {
        public struct OverlapResult
        {
            public bool IsIntersecting;
            public float3 Axis;
            public float Depth;
        }
        
        public static OverlapResult Overlaps(this OBB obb1, OBB obb2)
        {
            // Получаем оси обеих рамок
            Span<float3> axes1 = stackalloc float3[3];
            axes1[0] = obb1.X;
            axes1[1] = obb1.Y;
            axes1[2] = obb1.Z;
            
            Span<float3> axes2 = stackalloc float3[3];
            axes2[0] = obb2.X;
            axes2[1] = obb2.Y;
            axes2[2] = obb2.Z;

            // Все оси для проверки (3 от obb1, 3 от obb2, 9 крестов между ними)
            Span<float3> testAxes = stackalloc float3[15];
            int index = 0;

            // Добавляем оси OBB1 и OBB2
            for (int i = 0; i < 3; i++) testAxes[index++] = axes1[i];
            for (int i = 0; i < 3; i++) testAxes[index++] = axes2[i];

            // Добавляем оси, полученные векторным произведением (cross-product)
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    testAxes[index++] = cross(axes1[i], axes2[j]);
                }
            }
            
            float minOverlapDepth = float.MaxValue;
            float3 overlapAxis = float3.zero;

            // Проверяем все оси
            foreach (float3 axis in testAxes)
            {
                // Пропускаем нулевые оси
                if (lengthsq(axis) < 1e-6f)
                    continue;

                // Нормализуем ось
                float3 normalizedAxis = normalize(axis);

                // Проецируем оба OBB на текущую ось
                if (!OverlapOnAxis(obb1, obb2, normalizedAxis, out var overlapDepth))
                    return new OverlapResult() { IsIntersecting = false };
                
                if (overlapDepth < minOverlapDepth)
                {
                    minOverlapDepth = overlapDepth;
                    overlapAxis = normalizedAxis;
                }
            }

            return new OverlapResult()
            {
                IsIntersecting = true,
                Axis = overlapAxis,
                Depth = minOverlapDepth
            };
        }
        
        private static bool OverlapOnAxis(OBB obb1, OBB obb2, float3 axis, out float overlapDepth)
        {
            float projection1 = math.dot(obb1.Center, axis);
            float projection2 = math.dot(obb2.Center, axis);

            float radius1 = ProjectRadius(obb1, axis);
            float radius2 = ProjectRadius(obb2, axis);

            float distance = abs(projection1 - projection2);
            float overlap = (radius1 + radius2) - distance;

            overlapDepth = overlap;

            return overlap > 0;
        }
        
        private static float ProjectRadius(OBB obb, float3 axis)
        {
            // Радиус OBB на данной оси — это сумма проекций локальных осей, умноженных на размеры
            return
                math.abs(dot(obb.X, axis)) * obb.Extents.x +
                math.abs(dot(obb.Y, axis)) * obb.Extents.y +
                math.abs(dot(obb.Z, axis)) * obb.Extents.z;
        }
        
        public static bool IntersectsRay(this OBB obb, in PrecomputedRay ray, out float tMin)
        {
            tMin = float.MinValue;
            float tMax = float.MaxValue;

            // Вектор от начала луча до центра OBB
            float3 p = obb.Center - ray.origin;

            // Проверяем пересечение по каждой из осей OBB
            for (int i = 0; i < 3; i++)
            {
                // Выбираем текущую ось и размер
                float3 axis = i == 0 ? obb.X : (i == 1 ? obb.Y : obb.Z);
                float extent = i == 0 ? obb.Extents.x : (i == 1 ? obb.Extents.y : obb.Extents.z);

                // Проецируем луч и центр OBB на текущую ось
                float e = math.dot(axis, p);             // Проекция центра OBB на ось
                float f = math.dot(axis, ray.dir); // Проекция направления луча на ось

                if (math.abs(f) > 1e-6f) // Луч не параллелен плоскости
                {
                    // Рассчитываем t для пересечения с минимальной и максимальной гранью
                    float t1 = (e - extent) / f;
                    float t2 = (e + extent) / f;

                    // Убедимся, что t1 <= t2
                    if (t1 > t2)
                    {
                        float temp = t1;
                        t1 = t2;
                        t2 = temp;
                    }

                    // Обновляем tMin и tMax
                    tMin = math.max(tMin, t1);
                    tMax = math.min(tMax, t2);

                    // Если интервал пуст, пересечения нет
                    if (tMin > tMax)
                        return false;
                }
                else // Луч параллелен плоскости
                {
                    // Проверяем, находится ли луч внутри граней
                    if (-extent > e || e > extent)
                        return false;
                }
            }

            // Если пересечение есть, tMin содержит расстояние до точки входа
            return tMin >= 0;
        }
    }
}