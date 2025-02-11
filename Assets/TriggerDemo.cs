using Scellecs.Morpeh;
using Scellecs.Morpeh.Collision.Components;
using Unity.Mathematics;
using UnityEngine;

public class TriggerDemo : MonoBehaviour
{
    public Camera Camera;

    private Filter _octree;
    private float3 _lastHitPoint;
    private Ray _lastRay;

    protected void Start()
    {
        _octree = World.Default.Filter.With<OctreeComponent>().Build();
    }

    protected void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ЛКМ
        {
            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                Debug.Log("Hit: " + hit.collider.name);
            }

            foreach (var entity in _octree)
            {
                ref var octree = ref entity.GetComponent<OctreeComponent>();
                if (octree.Raycast(ray, out var raycastHit))
                {
                    _lastHitPoint = raycastHit.Point;
                    _lastRay = ray;
                    Debug.Log("Native Hit: " + raycastHit.EntityHolder.Entity);
                }
            }
        }
    }
    
    protected void OnValidate()
    {
        if (Camera == null)
            Camera = GetComponent<Camera>();
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_lastHitPoint, 0.1f);
        Gizmos.color = Color.gray;
        Gizmos.DrawLine(_lastRay.origin, _lastRay.origin + _lastRay.direction * 1000f);
    }
}
