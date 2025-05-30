using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;
public class PlayerControl_CameraControl : PlayerControl_BaseControl
{
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int Surface = Shader.PropertyToID("_Surface");
    private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
    private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

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
    [SerializeField] private float sphereCastRadius = 0.3f;
    [SerializeField] private float heightAdjustmentSpeed = 2.0f;
    [SerializeField] private float minHeightAdjustment = 0.5f;
    [SerializeField] private float maxHeightAdjustment = 2.0f;
    [SerializeField] private float collisionDamping = 0.5f; // 碰撞阻尼系数
    [SerializeField] private float lookAtHeightRatio = 0.5f; 
    [SerializeField] private float lookAtSmoothTime = 0.2f; 
    
    [Header("Follow Mode")]
    [SerializeField] private bool useLazyFollow = false; // 是否使用慵懒跟随模式
    [SerializeField] [Range(0.01f, 1f)] private float lazyFollowFactor = 0.1f; // 慵懒跟随系数
    [SerializeField] private bool scaleWithTime = true; // 是否随时间缩放

    private Material[] _materials; 
    private float[] _originalAlphas; 
    private float _currentAlpha;
    private float _alphaVelocity;
    private float _currentLookAtHeightOffset;
    private float _lookAtHeightVelocity;
    private float _currentX;
    private float _currentY;
    private float _currentDistance; 
    private float _targetDistance; 
    private float _zoomVelocity; 
    private Vector3 _smoothVelocity = Vector3.zero;
    private bool _isCameraControlActive;
    private float _originalHeightOffset;
    private float _currentHeightOffset;
    private float _targetHeightOffset;
    private float _dampedDistance; 
    private float _distanceVelocity; 

    protected override void Start()
    {
        base.Start();
        
        controlHandler.onCameraControl.AddListener(EnableCameraControl);
        controlHandler.onStopCameraControl.AddListener(DisableCameraControl);
        controlHandler.onMouseMoveUpdate.AddListener(MouseMoveUpdate);

        if (controlledCamera == null) controlledCamera = Camera.main;
        if (target == null)
        {
            var player = transform;
            if (player != null) target = player.transform;
        }

        if (controlledCamera == null || target == null) return;
        
        _currentDistance = distance;
        _targetDistance = distance;
        _dampedDistance = distance;
        var angles = controlledCamera.transform.eulerAngles;
        _currentX = angles.y;
        _currentY = angles.x;
        
        _originalHeightOffset = controlledCamera.transform.position.y - target.position.y;
        _currentHeightOffset = _originalHeightOffset;
        _targetHeightOffset = _originalHeightOffset;
        
        _currentLookAtHeightOffset = 0f;
    }

    private void Update()
    {

        if (!_isCameraControlActive) return;
        
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            _targetDistance = Mathf.Clamp(_targetDistance - scroll * zoomSpeed, minDistance, maxDistance);
        }
        
        _currentDistance = Mathf.SmoothDamp(_currentDistance, _targetDistance, ref _zoomVelocity, zoomSmoothTime);
    }

    

    private void MouseMoveUpdate(Vector2 arg0)
    {

        if (!controlledCamera || !target) return;
        
        if (_isCameraControlActive)
        {
            _currentX += arg0.x * mouseSensitivity;
            _currentY -= arg0.y * mouseSensitivity;
            _currentY = Mathf.Clamp(_currentY, minVerticalAngle, maxVerticalAngle);
        }

        UpdateCameraPosition();
        
        _currentHeightOffset = Mathf.Lerp(_currentHeightOffset, _targetHeightOffset, Time.deltaTime * heightAdjustmentSpeed);
    }

    private void UpdateCameraPosition()
    {
        var direction = new Vector3(0, 0, -_dampedDistance);
        var rotation = Quaternion.Euler(_currentY, _currentX, 0);
        var desiredPosition = target.position + rotation * direction;
        desiredPosition.y += _currentHeightOffset;

        if (avoidCollisions)
        {
            var directionToCamera = desiredPosition - target.position;
            var distanceToCamera = directionToCamera.magnitude;
            var ray = new Ray(target.position, directionToCamera.normalized);

            var isColliding = Physics.SphereCast(ray, sphereCastRadius, out var hit, distanceToCamera, collisionLayers);
        
            if (isColliding)
            {
                var collisionPosition = hit.point + hit.normal * collisionOffset;
                var actualDistance = Vector3.Distance(target.position, collisionPosition);
                
                _dampedDistance = Mathf.SmoothDamp(_dampedDistance, actualDistance, ref _distanceVelocity, collisionDamping);
                
                var distanceRatio = Mathf.InverseLerp(minDistance, _targetDistance, _dampedDistance);
                _targetHeightOffset = Mathf.Lerp(maxHeightAdjustment, minHeightAdjustment, distanceRatio);
                
                collisionPosition.y = Mathf.Max(collisionPosition.y, target.position.y + minHeightAdjustment);
                desiredPosition = collisionPosition;
                
                _currentDistance = Mathf.Lerp(_currentDistance, _dampedDistance, 0.1f);
                
                var targetLookAtOffset = (_currentHeightOffset - _originalHeightOffset) * lookAtHeightRatio;
                _currentLookAtHeightOffset = Mathf.SmoothDamp(
                    _currentLookAtHeightOffset, 
                    targetLookAtOffset, 
                    ref _lookAtHeightVelocity, 
                    lookAtSmoothTime);
            }
            else
            {
                _dampedDistance = Mathf.SmoothDamp(_dampedDistance, _targetDistance, ref _distanceVelocity, collisionDamping);
                _targetHeightOffset = _originalHeightOffset;
                
                _currentLookAtHeightOffset = Mathf.SmoothDamp(
                    _currentLookAtHeightOffset, 
                    0f, 
                    ref _lookAtHeightVelocity, 
                    lookAtSmoothTime);
            }
        }
        if (useLazyFollow)
        {
            var deltaFactor = scaleWithTime ? lazyFollowFactor * Time.deltaTime * 60f : lazyFollowFactor;
            deltaFactor = Mathf.Clamp01(deltaFactor);
            
            var currentPos = controlledCamera.transform.position;
            currentPos += (desiredPosition - currentPos) * deltaFactor;
            controlledCamera.transform.position = currentPos;
        }
        else
        {
            controlledCamera.transform.position = Vector3.SmoothDamp(
                controlledCamera.transform.position, 
                desiredPosition, 
                ref _smoothVelocity, 
                smoothTime);
        }

        controlledCamera.transform.position = Vector3.SmoothDamp(
            controlledCamera.transform.position, 
            desiredPosition, 
            ref _smoothVelocity, 
            smoothTime);
        
        var baseLookAtPosition = target.position;
        var adjustedLookAtPosition = baseLookAtPosition + Vector3.up * _currentLookAtHeightOffset;
        controlledCamera.transform.LookAt(adjustedLookAtPosition);
        
        controlledCamera.nearClipPlane = Mathf.Clamp(
            _dampedDistance * 0.1f, 
            0.01f, 
            0.3f 
        );
    }
    
    private void EnableCameraControl()
    {
        _isCameraControlActive = true;
    }

    private void DisableCameraControl()
    {
        _isCameraControlActive = false;
    }

    public void OnDestroy()
    {

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
        _dampedDistance = _currentDistance;
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

