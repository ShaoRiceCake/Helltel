using UnityEngine;

public class YAxisRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f; // 默认每秒旋转90度
    [SerializeField] private bool rotateEnabled = true;
    private Rigidbody rb;

    void Start()
    {
        // 获取Rigidbody组件并设置为运动学
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        else
        {
            Debug.LogWarning("No Rigidbody component found on this object. Adding one.");
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    void FixedUpdate()
    {
        if (rotateEnabled)
        {
            // 计算旋转角度（基于固定时间步长）
            float rotationAmount = rotationSpeed * Time.fixedDeltaTime;
            
            // 围绕Y轴旋转
            Quaternion deltaRotation = Quaternion.Euler(0f, rotationAmount, 0f);
            
            // 应用旋转（使用Rigidbody的MoveRotation保持物理系统的正确性）
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }

    // 可选：提供方法来控制旋转
    public void SetRotationEnabled(bool enabled)
    {
        rotateEnabled = enabled;
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
}