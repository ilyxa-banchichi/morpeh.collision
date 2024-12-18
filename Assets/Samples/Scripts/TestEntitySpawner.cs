using Scellecs.Morpeh;
using Scellecs.Morpeh.Collision.Requests;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Scripts
{
    public class TestEntitySpawner : MonoBehaviour
    {
        public int CountInRow = 100;
        public GameObject Prefab;
        
        protected void Awake()
        {
            for (int i = 0; i < CountInRow; i++)
            for (int j = 0; j < CountInRow; j++)
            {
                var e = World.Default.CreateEntity();
                ref var request = ref e.AddComponent<CreateBoxColliderRequest>();
                request.InitPosition = new float3((float)(i + 0.5), 0f, (float)(j + 0.5));
                request.InitRotation = quaternion.identity;
                request.Size = 1f;
            }
        }
    }
}