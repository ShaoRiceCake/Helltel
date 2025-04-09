using UnityEngine;

public class PlayerControl_CameraControl : PlayerControl_BaseControl
{
    [Header("Camera Settings")]
    [SerializeField] private Camera controlledCamera;
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5.0f;
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float minVerticalAngle = -20.0f;
    [SerializeField] private float maxVerticalAngle = 80.0f;
    [SerializeField] private float smoothTime = 0.2f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5.0f; 
    [SerializeField] private float minDistance = 2.0f;
    [SerializeField] private float maxDistance = 10.0f; 
    [SerializeField] private float zoomSmoothTime = 0.1f; 

    [Header("Collision Settings")]
    [SerializeField] private bool avoidCollisions = true;
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float collisionOffset = 0.3f;

    private float _currentX;
    private float _currentY;
    private float _currentDistance; 
    private float _targetDistance; 
    private float _zoomVelocity; 
    private Vector3 _smoothVelocity = Vector3.zero;
    private bool _isCameraControlActive;

    protected override void Start()
    {
        base.Start();
        
        if (controlHandler != null)
        {
            controlHandler.onCameraControl.AddListener(EnableCameraControl);
            controlHandler.onStopCameraControl.AddListener(DisableCameraControl);
        }

        if (controlledCamera == null) controlledCamera = Camera.main;
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        if (controlledCamera == null || target == null) return;
        
        _currentDistance = distance;
        _targetDistance = distance;
        var angles = controlledCamera.transform.eulerAngles;
        _currentX = angles.y;
        _currentY = angles.x;
    }

    private void Update()
    {
        if (!_isCameraControlActive) return;
        
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            _targetDistance = Mathf.Clamp(_targetDistance - scroll * zoomSpeed, minDistance, maxDistance);
        }
        
        // 平滑过渡到目标距离
        _currentDistance = Mathf.SmoothDamp(_currentDistance, _targetDistance, ref _zoomVelocity, zoomSmoothTime);
    }

    private void LateUpdate()
    {
        if (!controlledCamera || !target || !_isCameraControlActive) return;
        
        _currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
        _currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _currentY = Mathf.Clamp(_currentY, minVerticalAngle, maxVerticalAngle);

        var direction = new Vector3(0, 0, -_currentDistance); // 使用当前距离
        var rotation = Quaternion.Euler(_currentY, _currentX, 0);
        var desiredPosition = target.position + rotation * direction;

        if (avoidCollisions && Physics.Linecast(target.position, desiredPosition, out var hit, collisionLayers))
        {
            desiredPosition = hit.point + hit.normal * collisionOffset;
            _currentDistance = Vector3.Distance(target.position, desiredPosition);
        }

        controlledCamera.transform.position = Vector3.SmoothDamp(
            controlledCamera.transform.position, 
            desiredPosition, 
            ref _smoothVelocity, 
            smoothTime);
            
        controlledCamera.transform.LookAt(target);
    }

    private void EnableCameraControl()
    {
        _isCameraControlActive = true;
    }

    private void DisableCameraControl()
    {
        _isCameraControlActive = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (controlHandler == null) return;
        controlHandler.onCameraControl.RemoveListener(EnableCameraControl);
        controlHandler.onStopCameraControl.RemoveListener(DisableCameraControl);
    }
    
    public void SetZoomRange(float min, float max)
    {
        minDistance = Mathf.Max(0.1f, min);
        maxDistance = Mathf.Max(minDistance, max);
        _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
    }

    public void SetDistanceImmediate(float newDistance)
    {
        _currentDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        _targetDistance = _currentDistance;
    }

    public void SetControlledCamera(Camera newCamera)
    {
        controlledCamera = newCamera;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }

    public float GetCurrentX()
    {
        return _currentX;
    }

    public float GetCurrentY()
    {
        return _currentY;
    }    
}