using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ElasticConstraint : MonoBehaviour
{
    [Header("弹性设置")]
    [Tooltip("弹性系数 - 值越大，回弹力越强")]
    public float elasticStrength = 50f;
    
    [Tooltip("阻尼系数 - 防止过度振荡")]
    public float damping = 5f;
    
    [Tooltip("最大约束距离 - 超过此距离会施加更强的力")]
    public float maxDistance = 2f;
    
    [Header("参考对象")]
    [Tooltip("目标固定对象(父对象)")]
    public Transform targetParent;

    private Rigidbody rb;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private bool isInitialized = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (targetParent != null)
        {
            InitializeConstraint();
        }
        else
        {
            Debug.LogWarning("目标父对象未分配，请分配目标父对象。");
        }
    }

    public void InitializeConstraint()
    {
        if (targetParent == null) return;
        
        // 记录初始相对位置和旋转
        initialLocalPosition = targetParent.InverseTransformPoint(transform.position);
        initialLocalRotation = Quaternion.Inverse(targetParent.rotation) * transform.rotation;
        
        isInitialized = true;
    }

    void FixedUpdate()
    {
        if (!isInitialized || targetParent == null) return;
        
        // 计算目标位置(父对象的局部空间转换到世界空间)
        Vector3 targetPosition = targetParent.TransformPoint(initialLocalPosition);
        
        // 计算当前位置与目标位置的偏移
        Vector3 positionDelta = targetPosition - transform.position;
        float distance = positionDelta.magnitude;
        
        // 计算弹性力 (胡克定律: F = -k * x)
        Vector3 elasticForce = elasticStrength * positionDelta;
        
        // 计算阻尼力 (与速度相反)
        Vector3 dampingForce = damping * rb.velocity;
        
        // 如果超过最大距离，增加额外的力
        if (distance > maxDistance)
        {
            float excess = distance - maxDistance;
            elasticForce += elasticForce.normalized * excess * elasticStrength * 0.5f;
        }
        
        // 应用合力
        rb.AddForce(elasticForce - dampingForce);
        
        // 可选: 旋转约束
        Quaternion targetRotation = targetParent.rotation * initialLocalRotation;
        Quaternion rotationDelta = targetRotation * Quaternion.Inverse(transform.rotation);
        rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
        
        if (angle > 180f) angle -= 360f;
        
        if (Mathf.Abs(angle) > 0.1f)
        {
            Vector3 angularForce = (0.1f * angle * axis - 0.8f * rb.angularVelocity);
            rb.AddTorque(angularForce, ForceMode.VelocityChange);
        }
    }

    // 在编辑器中设置目标父对象时自动初始化
    void OnValidate()
    {
        if (targetParent != null && !isInitialized)
        {
            InitializeConstraint();
        }
    }
}