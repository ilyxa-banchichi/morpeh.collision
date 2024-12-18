using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace NativeTrees
{
    public static class OBBOverlapExtensions
    {
        public static bool Overlaps(this OBB obb1, OBB obb2)
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

            // Проверяем все оси
            foreach (float3 axis in testAxes)
            {
                // Пропускаем нулевые оси
                if (lengthsq(axis) < 1e-6f)
                    continue;

                // Нормализуем ось
                float3 normalizedAxis = normalize(axis);

                // Проецируем оба OBB на текущую ось
                if (!OverlapOnAxis(obb1, obb2, normalizedAxis))
                    return false; // Если нет перекрытия на этой оси, OBB не пересекаются
            }

            return true; // Если перекрытие есть на всех осях, OBB пересекаются
        }
        
        private static bool OverlapOnAxis(OBB obb1, OBB obb2, float3 axis)
        {
            // Проекция центра первого и второго OBB на ось
            float projection1 = dot(obb1.Center, axis);
            float projection2 = dot(obb2.Center, axis);

            // Проекция половины размеров (радиус OBB) на ось
            float radius1 = ProjectRadius(obb1, axis);
            float radius2 = ProjectRadius(obb2, axis);

            // Проверяем расстояние между центрами против суммы радиусов
            return math.abs(projection1 - projection2) <= (radius1 + radius2);
        }
        
        private static float ProjectRadius(OBB obb, float3 axis)
        {
            // Радиус OBB на данной оси — это сумма проекций локальных осей, умноженных на размеры
            return
                math.abs(dot(obb.X, axis)) * obb.Extents.x +
                math.abs(dot(obb.Y, axis)) * obb.Extents.y +
                math.abs(dot(obb.Z, axis)) * obb.Extents.z;
        }
    }
}