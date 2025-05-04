using UnityEngine;

public class VelocityMonitor : MonoBehaviour
{
    [Tooltip("是否在控制台输出速度信息")]
    public bool logVelocity = true;
    
    [Tooltip("日志输出间隔时间(秒)")]
    public float logInterval = 0.5f;
    
    private Rigidbody rb;
    private float timeSinceLastLog = 0f;
    private Vector3 lastPosition;
    private Vector3 calculatedVelocity;
    
    void Start()
    {
        // 尝试获取Rigidbody组件
        rb = GetComponent<Rigidbody>();
        
        // 如果没有Rigidbody，我们会使用位置变化计算速度
        lastPosition = transform.position;
    }
    
    void Update()
    {
        // 如果没有Rigidbody，计算基于位置变化的速度
        if (rb == null)
        {
            Vector3 currentPosition = transform.position;
            calculatedVelocity = (currentPosition - lastPosition) / Time.deltaTime;
            lastPosition = currentPosition;
        }
        
        // 日志输出
        if (logVelocity)
        {
            timeSinceLastLog += Time.deltaTime;
            if (timeSinceLastLog >= logInterval)
            {
                LogCurrentVelocity();
                timeSinceLastLog = 0f;
            }
        }
    }
    
    void LogCurrentVelocity()
    {
        Vector3 velocity = rb != null ? rb.velocity : calculatedVelocity;
        float speed = velocity.magnitude;
        
        Debug.Log($"{gameObject.name} 速度信息:\n" +
                  $"当前速度: {speed:F2} 单位/秒\n" +
                  $"速度向量: {velocity}\n" +
                  $"X轴速度: {velocity.x:F2}\n" +
                  $"Y轴速度: {velocity.y:F2}\n" +
                  $"Z轴速度: {velocity.z:F2}");
    }
    
    // 也可以从外部获取当前速度
    public Vector3 GetCurrentVelocity()
    {
        return rb != null ? rb.velocity : calculatedVelocity;
    }
    
    // 获取当前速度大小
    public float GetCurrentSpeed()
    {
        return (rb != null ? rb.velocity : calculatedVelocity).magnitude;
    }
}