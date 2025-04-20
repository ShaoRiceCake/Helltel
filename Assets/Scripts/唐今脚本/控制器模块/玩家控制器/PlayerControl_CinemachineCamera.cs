using UnityEngine;
using Cinemachine;

public class PlayerControl_CinemachineCamera : PlayerControl_BaseControl
{
    [Header("Cinemachine Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float minVerticalAngle = -5.0f;
    [SerializeField] private float maxVerticalAngle = 30.0f;
    [SerializeField] private float smoothTime = 0.2f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5.0f; 
    [SerializeField] private float minDistance = 2.0f;
    [SerializeField] private float maxDistance = 10.0f; 
    [SerializeField] private float zoomSmoothTime = 0.1f; 
    
    [Header("透明度设置")]
    [SerializeField] private Renderer targetRenderer; 
    [SerializeField] private float minAlpha = 0.3f; 
    [SerializeField] private float maxAlpha = 1.0f; 
    [SerializeField] private float fadeStartDistance = 3.0f; 
    [SerializeField] private float fadeEndDistance = 0.5f; 
    [SerializeField] private float fadeSmoothTime = 0.2f; 

    private Material[] _materials; 
    private float[] _originalAlphas; 
    private float _currentAlpha;
    private float _alphaVelocity;
    private float _currentX;
    private float _currentY;
    private float _currentDistance; 
    private float _targetDistance; 
    private float _zoomVelocity; 
    private Vector3 _smoothVelocity = Vector3.zero;
    private bool _isCameraControlActive;
    private CinemachineTransposer _transposer;
    private Vector3 _originalFollowOffset;

    protected override void Start()
    {
        base.Start();
        
        if (controlHandler != null)
        {
            controlHandler.onCameraControl.AddListener(EnableCameraControl);
            controlHandler.onStopCameraControl.AddListener(DisableCameraControl);
        }

        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogError("No CinemachineVirtualCamera found in scene!");
                return;
            }
        }

        // 获取Transposer组件
        _transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (_transposer == null)
        {
            Debug.LogError("Virtual Camera needs a Transposer component!");
            return;
        }

        _originalFollowOffset = _transposer.m_FollowOffset;
        _currentDistance = _originalFollowOffset.magnitude;
        _targetDistance = _currentDistance;

        // 初始化角度
        var angles = virtualCamera.transform.eulerAngles;
        _currentX = angles.y;
        _currentY = angles.x;
        
        // 初始化透明度相关变量
        if (targetRenderer == null) return;
        _materials = targetRenderer.materials;
        _originalAlphas = new float[_materials.Length];
            
        for (var i = 0; i < _materials.Length; i++)
        {
            // 保存原始透明度
            _originalAlphas[i] = _materials[i].color.a;
        }
            
        _currentAlpha = maxAlpha;
    }

    private void Update()
    {
        if (!_isCameraControlActive || _transposer == null) return;
        
        // 处理缩放输入
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            _targetDistance = Mathf.Clamp(_targetDistance - scroll * zoomSpeed, minDistance, maxDistance);
        }
        
        _currentDistance = Mathf.SmoothDamp(_currentDistance, _targetDistance, ref _zoomVelocity, zoomSmoothTime);
    }

    private void LateUpdate()
    {
        if (!virtualCamera || !_transposer) return;
        
        if (_isCameraControlActive)
        {
            // 处理旋转输入
            _currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
            _currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            _currentY = Mathf.Clamp(_currentY, minVerticalAngle, maxVerticalAngle);
        }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        // 计算旋转
        var rotation = Quaternion.Euler(_currentY, _currentX, 0);
    
        // 计算目标偏移
        var targetOffset = rotation * Vector3.back * _currentDistance;
    
        // 让Cinemachine的阻尼系统处理平滑
        _transposer.m_FollowOffset = targetOffset;
    
        // 调整Cinemachine的阻尼设置
        _transposer.m_XDamping = smoothTime;
        _transposer.m_YDamping = smoothTime;
        _transposer.m_ZDamping = smoothTime;
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
        UpdateCameraPosition();
    }

    public void SetVirtualCamera(CinemachineVirtualCamera newCamera)
    {
        virtualCamera = newCamera;
        if (virtualCamera != null)
        {
            _transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (_transposer != null)
            {
                _originalFollowOffset = _transposer.m_FollowOffset;
            }
        }
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