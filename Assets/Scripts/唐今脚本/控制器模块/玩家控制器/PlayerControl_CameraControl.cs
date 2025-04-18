using UnityEngine;
using UnityEngine.Rendering; 

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
    [SerializeField] private float sphereCastRadius = 0.3f;
    [SerializeField] private float heightAdjustmentSpeed = 2.0f;
    [SerializeField] private float minHeightAdjustment = 0.5f;
    [SerializeField] private float maxHeightAdjustment = 2.0f;
    [SerializeField] private float collisionDamping = 0.5f; // 碰撞阻尼系数
    [SerializeField] private float lookAtHeightRatio = 0.5f; 
    [SerializeField] private float lookAtSmoothTime = 0.2f; 

    [Header("透明度设置")]
    [SerializeField] private Renderer targetRenderer; // 要调整透明度的人物模型Renderer
    [SerializeField] private float minAlpha = 0.3f; // 最小透明度值
    [SerializeField] private float maxAlpha = 1.0f; // 最大透明度值
    [SerializeField] private float fadeStartDistance = 3.0f; // 开始淡出的距离
    [SerializeField] private float fadeEndDistance = 0.5f; // 完全淡出的距离
    [SerializeField] private float fadeSmoothTime = 0.2f; // 淡出平滑时间

    private Material[] _materials; // 存储所有材质
    private float[] _originalAlphas; // 存储原始透明度
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
        _dampedDistance = distance; // 初始化阻尼距离
        var angles = controlledCamera.transform.eulerAngles;
        _currentX = angles.y;
        _currentY = angles.x;
        
        _originalHeightOffset = controlledCamera.transform.position.y - target.position.y;
        _currentHeightOffset = _originalHeightOffset;
        _targetHeightOffset = _originalHeightOffset;
        
        _currentLookAtHeightOffset = 0f;

        if (targetRenderer == null) return;
        _materials = targetRenderer.materials;
        _originalAlphas = new float[_materials.Length];
            
        for (int i = 0; i < _materials.Length; i++)
        {
            // URP中获取原始透明度的方式
            _originalAlphas[i] = _materials[i].GetColor("_BaseColor").a;
                
            // 确保材质初始状态正确
            SetupURPMaterial(_materials[i], _originalAlphas[i]);
        }
            
        _currentAlpha = maxAlpha;
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
        
        // 更新透明度
        UpdateTransparency();
    }

    private void UpdateTransparency()
    {
        if (targetRenderer == null || _materials == null) return;
        
        float distanceRatio = Mathf.InverseLerp(fadeEndDistance, fadeStartDistance, _currentDistance);
        float targetAlpha = Mathf.Lerp(minAlpha, maxAlpha, distanceRatio);
        
        _currentAlpha = Mathf.SmoothDamp(_currentAlpha, targetAlpha, ref _alphaVelocity, fadeSmoothTime);
        
        for (int i = 0; i < _materials.Length; i++)
        {
            // URP Lit材质需要特殊处理
            SetupURPMaterial(_materials[i], _currentAlpha * _originalAlphas[i]);
        }
    }

    private void SetupURPMaterial(Material material, float targetAlpha)
    {
        // 1. 更改渲染模式（如果当前不是透明模式）
        if (material.GetFloat("_Surface") != 1) // 1表示透明模式
        {
            material.SetFloat("_Surface", 1); // 设置为透明模式
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        // 2. 设置基础颜色和透明度
        Color baseColor = material.GetColor("_BaseColor");
        baseColor.a = targetAlpha;
        material.SetColor("_BaseColor", baseColor);
    }

    private void LateUpdate()
    {
        if (!controlledCamera || !target) return;
        
        if (_isCameraControlActive)
        {
            _currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
            _currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
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
                
                // Calculate look-at height offset based on current height adjustment
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
                
                // Smoothly return look-at height to original position
                _currentLookAtHeightOffset = Mathf.SmoothDamp(
                    _currentLookAtHeightOffset, 
                    0f, 
                    ref _lookAtHeightVelocity, 
                    lookAtSmoothTime);
            }
        }
        else
        {
            _dampedDistance = _currentDistance;
            _currentLookAtHeightOffset = 0f;
        }

        controlledCamera.transform.position = Vector3.SmoothDamp(
            controlledCamera.transform.position, 
            desiredPosition, 
            ref _smoothVelocity, 
            smoothTime);
        
        // Improved LookAt with height adjustment
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
    
    // 新增方法：设置目标Renderer
    public void SetTargetRenderer(Renderer newRenderer)
    {
        targetRenderer = newRenderer;
        
        // 重新初始化材质
        if (targetRenderer == null) return;
        _materials = targetRenderer.materials;
        _originalAlphas = new float[_materials.Length];
            
        for (var i = 0; i < _materials.Length; i++)
        {
            _originalAlphas[i] = _materials[i].color.a;
        }
    }

    // 新增方法：设置淡出距离范围
    public void SetFadeDistance(float start, float end)
    {
        fadeStartDistance = Mathf.Max(start, end);
        fadeEndDistance = Mathf.Min(start, end);
    }

    // 新增方法：设置透明度范围
    public void SetAlphaRange(float min, float max)
    {
        minAlpha = Mathf.Clamp01(min);
        maxAlpha = Mathf.Clamp01(max);
    }
}