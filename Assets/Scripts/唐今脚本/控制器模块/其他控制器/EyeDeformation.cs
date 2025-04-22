using UnityEngine;

public class EyeDeformation : MonoBehaviour
{
    [Header("References")]
    public Transform eyeballMesh; 
    public Transform pupilAnchor; 
    public Transform pupilMesh;  

    [Header("Deformation Settings")]
    public float maxDeformation = 0.5f;
    public float deformationSharpness = 2f;
    public float rotationSpeed = 10f;

    [Header("Movement Settings")]
    public float maxPupilOffset = 0.1f;
    public float pupilPlaneOffset = 0.05f;

    private Vector3 _originalEyeballScale;
    private Vector3 _originalPupilScale;
    private readonly Vector3 _pupilPlaneNormal = -Vector3.forward;

    private void Start()
    {
        if (!eyeballMesh || !pupilAnchor || !pupilMesh)
        {
            Debug.LogError("References not set properly!");
            enabled = false;
            return;
        }

        _originalEyeballScale = eyeballMesh.localScale;
        _originalPupilScale = pupilMesh.localScale;
        pupilAnchor.localPosition = _pupilPlaneNormal * pupilPlaneOffset;
        pupilMesh.localPosition = Vector3.zero;
    }

    private void Update()
    {
        RestrictPupilMovement();
        UpdateEyeDeformation();
    }

    private void RestrictPupilMovement()
    {
        var localPos = pupilAnchor.localPosition;
        localPos.z = -pupilPlaneOffset;
        
        var circlePos = new Vector2(localPos.x, localPos.y);
        if (circlePos.magnitude > maxPupilOffset)
        {
            circlePos = circlePos.normalized * maxPupilOffset;
            localPos = new Vector3(circlePos.x, circlePos.y, -pupilPlaneOffset);
        }
        
        pupilAnchor.localPosition = localPos;
        pupilMesh.localScale = _originalPupilScale;
    }

    private void UpdateEyeDeformation()
    {
        var pupilLocalPos = pupilAnchor.localPosition;
        var pupilOffset = new Vector2(pupilLocalPos.x, pupilLocalPos.y);
        var offsetDistance = pupilOffset.magnitude;
        
        if (offsetDistance <= 0.001f)
        {
            eyeballMesh.localRotation = Quaternion.Slerp(
                eyeballMesh.localRotation, 
                Quaternion.identity, 
                Time.deltaTime * rotationSpeed
            );
            
            eyeballMesh.localScale = Vector3.Lerp(
                eyeballMesh.localScale, 
                _originalEyeballScale, 
                Time.deltaTime * 10f
            );
            return;
        }

        var offsetDirection = pupilOffset.normalized;
        
        var targetAngle = Mathf.Atan2(offsetDirection.y, offsetDirection.x) * Mathf.Rad2Deg;
        var targetRotation = Quaternion.Euler(0, 0, targetAngle);
        
        eyeballMesh.localRotation = Quaternion.Slerp(
            eyeballMesh.localRotation, 
            targetRotation, 
            Time.deltaTime * rotationSpeed
        );
        
        var deformationAmount = Mathf.Clamp01(offsetDistance / maxPupilOffset);
        var deformationFactor = Mathf.Pow(deformationAmount, deformationSharpness) * maxDeformation;
        

        var newScale = _originalEyeballScale;
        var stretch = 1f + deformationFactor;
        var squeeze = 1f - deformationFactor * 0.5f;
        
        newScale.x *= stretch;  
        newScale.y *= squeeze;  
        newScale.z *= squeeze;  
        
        eyeballMesh.localScale = Vector3.Lerp(
            eyeballMesh.localScale, 
            newScale, 
            Time.deltaTime * 10f
        );
    }
}