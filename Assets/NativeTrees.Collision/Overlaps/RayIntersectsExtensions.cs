using Unity.Mathematics;

namespace NativeTrees
{
    public static class RayIntersectsExtensions
    {
        public static bool IntersectsRay(this BoxCollider boxCollider, in PrecomputedRay ray, out float tMin)
        {
            tMin = float.MinValue;
            float tMax = float.MaxValue;

            // Вектор от начала луча до центра OBB
            float3 p = boxCollider.Center - ray.origin;

            // Проверяем пересечение по каждой из осей OBB
            for (int i = 0; i < 3; i++)
            {
                // Выбираем текущую ось и размер
                float3 axis = i == 0 ? boxCollider.X : (i == 1 ? boxCollider.Y : boxCollider.Z);
                float extent = i == 0 ? boxCollider.Extents.x : (i == 1 ? boxCollider.Extents.y : boxCollider.Extents.z);

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