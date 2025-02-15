using UnityEngine;

public class TestUnityListener : MonoBehaviour
{
    protected void Update()
    {
        Debug.Log("Update");
    }
    
    protected void FixedUpdate()
    {
        Debug.Log("FixedUpdate");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerStay");
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerStay");
    }
    
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerStay");
    }
}
