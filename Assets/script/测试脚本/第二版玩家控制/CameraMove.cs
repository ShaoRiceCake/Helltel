using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float rotationSpeed = 100f; 
    public float minXAngle = -35f; 
    public float maxXAngle = 35f; 

    private float _currentXRotation = 0f;

    private float _currentZoomVelocity; 

    private void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.parent.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.parent.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }

 
        if (Input.GetKey(KeyCode.W))
        {
            var deltaRotation = -rotationSpeed * Time.deltaTime;
            _currentXRotation += deltaRotation;
            _currentXRotation = Mathf.Clamp(_currentXRotation, minXAngle, maxXAngle); 
            transform.localEulerAngles = new Vector3(_currentXRotation, 0, 0);
        }

        if (!Input.GetKey(KeyCode.S)) return;
        {
            var deltaRotation = rotationSpeed * Time.deltaTime;
            _currentXRotation += deltaRotation;
            _currentXRotation = Mathf.Clamp(_currentXRotation, minXAngle, maxXAngle); 
            transform.localEulerAngles = new Vector3(_currentXRotation, 0, 0);
        }
    }
}