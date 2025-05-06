using UnityEngine;

public class ItemWoodCar : ActiveItem
{

    private void Start()
    {
        OnGrabbed.AddListener(HandleWeaponGrabbed);
        OnReleased.AddListener(HandleWeaponReleased);

    }

    private void HandleWeaponGrabbed()
    {
        enableOrbit = true;
        
    }
    
    protected override void UpdateOrbitRotation()
    {
        // 计算从中心点到物体的方向向量
        _currentOrbitDirection = transform.position - orbitCenter.position;

        // 如果方向向量长度接近0，跳过旋转
        if (_currentOrbitDirection.sqrMagnitude < 0.001f) return;

        // 计算物体局部前向在世界空间中的方向
        Vector3 worldForward = transform.TransformDirection(manualForwardDirection);
        Vector3 flatWorldForward = new Vector3(worldForward.x, 0, worldForward.z).normalized;

        // 计算目标方向在XZ平面上的投影
        Vector3 flatTargetDir = new Vector3(_currentOrbitDirection.x, 0, _currentOrbitDirection.z).normalized;

        if (flatTargetDir == Vector3.zero || flatWorldForward == Vector3.zero) return;

        // 计算需要旋转的角度差
        float angle = Vector3.SignedAngle(flatWorldForward, -flatTargetDir, Vector3.up);

        // 保持当前X和Z轴旋转
        Vector3 currentEuler = transform.rotation.eulerAngles;
    
        // 创建仅Y轴变化的新旋转
        Quaternion targetRotation = Quaternion.Euler(
            currentEuler.x,
            currentEuler.y + angle, // 基于当前旋转增加角度差
            currentEuler.z
        );

        // 平滑旋转
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            orbitRotationSpeed * Time.fixedDeltaTime
        );
    }    
    private void HandleWeaponReleased()
    {
        enableOrbit = false;
    }

    private void OnDestroy()
    {
        OnGrabbed.RemoveListener(HandleWeaponGrabbed);
        OnReleased.RemoveListener(HandleWeaponReleased);
    }
}