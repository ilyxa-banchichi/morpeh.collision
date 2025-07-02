using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace NativeTrees
{
    public static class LayerUtils
    {
        [BurstDiscard]
        public static int[] LayerCollisionMasks { get; } = new int[32];

        static LayerUtils()
        {
            for (int layer = 0; layer < 32; layer++)
                LayerCollisionMasks[layer] = GetCollisionMaskForLayer(layer);
        }
        
        public static NativeArray<int> GetMasksNative(Allocator allocator)
        {
            return new NativeArray<int>(LayerCollisionMasks, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ShouldCollide(int layer, int mask)
        {
            return (mask & (1 << layer)) != 0;
        }
        
        private static int GetCollisionMaskForLayer(int layer)
        {
            int mask = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(layer, i))
                    mask |= (1 << i);
            }
            
            return mask;
        }
    }
}