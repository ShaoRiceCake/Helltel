using UnityEngine;

public class MovementExperiment : MonoBehaviour
{
    // 移动参数
    [Header("Movement Settings")]
    public float moveSpeed = 5f;        // 匀速移动速度
    public float impulseForce = 10f;   // 冲量力度
    public float rotationSpeed = 5f;   // 旋转速度
    public float smoothTime = 0.1f;    // 运动平滑时间

    // 状态变量
    private Vector3 initialPosition;   // 初始位置
    private Vector3 currentVelocity;   // 当前速度
    private Vector3 smoothDampVelocity; // 平滑速度

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
    }

    void Update()
    {
        // 复位功能
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPosition();
        }

        // 冲量功能
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ApplyImpulse();
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 获取输入
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S
        float elevation = 0f;
        
        if (Input.GetKey(KeyCode.Q)) elevation = -1f;
        if (Input.GetKey(KeyCode.E)) elevation = 1f;

        // 计算目标速度
        Vector3 targetVelocity = new Vector3(horizontal, elevation, vertical).normalized * moveSpeed;
        
        // 平滑过渡到目标速度
        currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref smoothDampVelocity, smoothTime);
        
        // 应用移动
        rb.velocity = currentVelocity;
        

    }

    private void ApplyImpulse()
    {
        Vector3 impulseDirection;
        
        // 确定冲量方向
        if (currentVelocity.magnitude > 0.1f)
        {
            // 有移动时，沿当前移动方向
            impulseDirection = currentVelocity.normalized;
        }
        else
        {
            // 静止时，默认向前(+Z)
            impulseDirection = transform.forward;
        }
        
        // 施加冲量
        rb.AddForce(impulseDirection * impulseForce, ForceMode.Impulse);
    }

    private void ResetPosition()
    {
        // 重置位置和旋转
        transform.position = initialPosition;
        transform.rotation = Quaternion.identity;
        
        // 重置物理状态
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // 重置控制变量
        currentVelocity = Vector3.zero;
        smoothDampVelocity = Vector3.zero;
    }

    // 可选：在Inspector中显示当前速度
    void OnGUI()
    {
        GUILayout.Label($"当前速度: {currentVelocity.magnitude:F2}");
        GUILayout.Label($"冲量力度: {impulseForce:F2}");
    }
}