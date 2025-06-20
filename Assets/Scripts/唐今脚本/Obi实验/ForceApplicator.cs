using UnityEngine;

/// <summary>
/// 对刚体施加可控的持续牵引力和瞬时冲力，并可以重置物体状态。
/// </summary>
public class ApplyForceController : MonoBehaviour
{
    [Header("持续牵引力设置")]
    [Tooltip("牵引力的基础大小，可以在运行时动态调整")]
    public float continuousForceMagnitude = 10.0f;

    [Tooltip("牵引力的方向（相对于物体自身坐标）")]
    public Vector3 continuousForceDirection = Vector3.forward;
    
    [Space(10)] // 在检视面板中增加一些间距，让界面更清晰

    [Header("瞬时冲力设置")]
    [Tooltip("单次猛推的冲力大小")]
    public float impulseMagnitude = 25.0f;

    [Tooltip("瞬时冲力的方向（相对于物体自身坐标）")]
    public Vector3 impulseDirection = Vector3.up; // 默认设置为向上，产生一个跳跃的效果

    [Header("控制按键设置")]
    [Tooltip("按下此按键来开启或关闭持续牵引力")]
    public KeyCode toggleContinuousForceKey = KeyCode.Space;

    [Tooltip("按下此按键来施加一次瞬时冲力")]
    public KeyCode applyImpulseKey = KeyCode.K;

    [Tooltip("按下此按key来重置物体位置和状态")]
    public KeyCode resetKey = KeyCode.R;

    // 私有变量
    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isContinuousForceActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("在游戏对象 '" + gameObject.name + "' 上没有找到 Rigidbody 组件！请添加一个。", this);
            enabled = false;
            return;
        }

        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // --- 输入控制 ---

        // 切换持续牵引力
        if (Input.GetKeyDown(toggleContinuousForceKey))
        {
            isContinuousForceActive = !isContinuousForceActive;
            if (!isContinuousForceActive && rb.velocity != Vector3.zero)
            {
                Debug.Log("持续牵引力已关闭。");
            }
            else if(isContinuousForceActive)
            {
                Debug.Log("持续牵引力已开启！");
            }
        }

        // 施加瞬时冲力
        if (Input.GetKeyDown(applyImpulseKey))
        {
            ApplyImpulse();
        }

        // 重置物体状态
        if (Input.GetKeyDown(resetKey))
        {
            ResetObjectState();
        }
    }

    void FixedUpdate()
    {
        // 如果持续力处于激活状态，则在物理更新中施加
        if (isContinuousForceActive && rb != null)
        {
            ApplyContinuousForce();
        }
    }

    /// <summary>
    /// 施加持续的力
    /// </summary>
    private void ApplyContinuousForce()
    {
        Vector3 worldSpaceDir = transform.TransformDirection(continuousForceDirection.normalized);
        rb.AddForce(worldSpaceDir * continuousForceMagnitude, ForceMode.Force);
    }

    /// <summary>
    /// 施加一次性的瞬时冲力
    /// </summary>
    private void ApplyImpulse()
    {
        if (rb == null) return;

        Vector3 worldSpaceImpulseDir = transform.TransformDirection(impulseDirection.normalized);
        rb.AddForce(worldSpaceImpulseDir * impulseMagnitude, ForceMode.Impulse);
        Debug.Log("对 '" + gameObject.name + "' 施加了瞬时冲力！大小: " + impulseMagnitude);
    }

    /// <summary>
    /// 重置物体状态
    /// </summary>
    private void ResetObjectState()
    {
        isContinuousForceActive = false;
        
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        Debug.Log("物体 '" + gameObject.name + "' 已重置。");
    }
}