using UnityEngine;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(Rigidbody))]
public class ItemLantern : PassiveItem
{
    [Header("Light Settings")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private float minIntensity = 0.1f;
    [SerializeField] private float maxIntensity = 3f;
    [SerializeField] private float smoothDampTime = 0.5f; // 更长的平滑时间
    [SerializeField] private float activationSpeedThreshold = 3f; // 更高的激活阈值
    [SerializeField] private float deactivationSpeedThreshold = 1f; // 低于此值开始变暗
    [SerializeField] private float responseCurvePower = 2f; // 响应曲线指数

    private Rigidbody _rigidbody;
    private float _currentVelocity;
    private float _targetIntensity;
    private float _currentSpeed;

    protected override void Awake()
    {
        base.Awake();
        
        _rigidbody = GetComponent<Rigidbody>();
        lanternLight = GetComponent<Light>();
        lanternLight.enabled = false;
        
        OnGrabbed.AddListener(OnLanternGrabbed);
        OnReleased.AddListener(OnLanternReleased);
    }

    private void OnLanternGrabbed()
    {
        lanternLight.enabled = true;
        _targetIntensity = minIntensity;
    }

    private void OnLanternReleased()
    {
        lanternLight.enabled = false;
    }

    private void Update()
    {
        base.Update();
        
        if (!IsGrabbed || !lanternLight.enabled) return;
        
        _currentSpeed = _rigidbody.velocity.magnitude;
        
        // 响应曲线算法
        if (_currentSpeed > activationSpeedThreshold)
        {
            // 使用指数曲线使高速时才明显变亮
            float normalizedSpeed = Mathf.InverseLerp(activationSpeedThreshold, activationSpeedThreshold * 2, _currentSpeed);
            _targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.Pow(normalizedSpeed, responseCurvePower));
        }
        else if (_currentSpeed < deactivationSpeedThreshold)
        {
            // 速度低于阈值时缓慢衰减
            _targetIntensity = Mathf.Lerp(_targetIntensity, minIntensity, Time.deltaTime * 0.5f);
        }
        
        // 更平滑的过渡
        lanternLight.intensity = Mathf.SmoothDamp(
            lanternLight.intensity,
            _targetIntensity,
            ref _currentVelocity,
            smoothDampTime);
    }


}