using UnityEngine;
public class CameraAnchorController : MonoBehaviour {
    public Transform headAnchor;
    public Transform fingerTip;
    public float totalDistance = 3.0f;
    
    private void LateUpdate() {
        var headToFinger = fingerTip.position - headAnchor.position;
        var currentDistance = headToFinger.magnitude;
        var cameraDistance = Mathf.Clamp(totalDistance - currentDistance, 0.5f, 2.5f);
        
        transform.position = headAnchor.position - 
                             headToFinger.normalized * cameraDistance;
    }
}