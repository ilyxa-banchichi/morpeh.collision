using Samples.Scripts;
using Unity.Mathematics;
using UnityEngine;

public class TriggerDemo : MonoBehaviour
{
    public Camera Camera;

    private float3 _lastHitPoint;

    protected void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ЛКМ
        {
            if (CodeInstaller.CollisionService.TryRaycastFromScreenPoint(Camera, Input.mousePosition, 1 << 6,  out var raycastHit))
            {
                _lastHitPoint = raycastHit.Point;
                Debug.Log("Native Hit: " + raycastHit.Entity);
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
    }
}
