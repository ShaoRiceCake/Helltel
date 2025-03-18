using UnityEngine;

public class PlayerControl_FootControl : MonoBehaviour
{
    public GameObject targetObject; // 目标点对象
    public GameObject movingObjectLeft; // 左键控制的运动对象
    public GameObject movingObjectRight; // 右键控制的运动对象
    public GameObject forwardObject; // 用于确定正朝向的对象
    public float springForce = 10f; // 弹簧力度
    public float damping = 0.6f; // 阻尼系数
    public float moveSpeed = 15f; // 下落速度
    public float fixedHeight = 2f; // 固定下落高度
    public float impulseCoefficient = 0.3f; // 冲量系数
    public float mouseSensitivity = 0.1f; // 鼠标灵敏度
    public float downForce = 1f; // 下坠冲力


    private bool isLeftFootUp = false; // 左腿是否抬起
    private bool isRightFootUp = false; // 右腿是否抬起

    private Rigidbody movingRbLeft; // 左键控制的运动对象的刚体组件
    private Rigidbody movingRbRight; // 右键控制的运动对象的刚体组件
    private CatchControl catchControlLeft; // 左键控制的抓取控制组件
    private CatchControl catchControlRight; // 右键控制的抓取控制组件

    private PlayerControlInformationProcess controlHandler; // 控制事件处理器

    void Start()
    {
        // 获取运动对象的刚体组件
        movingRbLeft = movingObjectLeft.GetComponent<Rigidbody>();
        movingRbRight = movingObjectRight.GetComponent<Rigidbody>();

        // 获取抓取控制组件
        catchControlLeft = movingObjectLeft.GetComponent<CatchControl>();
        catchControlRight = movingObjectRight.GetComponent<CatchControl>();

        // 获取 PlayerControlInformationProcess 组件
        controlHandler = GetComponent<PlayerControlInformationProcess>();

        if (!controlHandler || !movingObjectLeft || !movingObjectRight)
        {
            Debug.LogError("Some of the PlayerControl_FootControl components are missing!");
            return;
        }

        // 订阅事件
        controlHandler.onLiftLeftLeg.AddListener(OnLiftLeftLeg);
        controlHandler.onLiftRightLeg.AddListener(OnLiftRightLeg);
        controlHandler.onReleaseLeftLeg.AddListener(OnReleaseLeftLeg);
        controlHandler.onReleaseRightLeg.AddListener(OnReleaseRightLeg);
        controlHandler.onCancelLegGrab.AddListener(OnCancelLegGrab);
        controlHandler.onDefaultMode.AddListener(OnDefaultMode);
        controlHandler.onMouseMove.AddListener(OnMouseMove);
    }

    void OnDestroy()
    {
        // 取消订阅事件
        if (controlHandler != null)
        {
            controlHandler.onLiftLeftLeg.RemoveListener(OnLiftLeftLeg);
            controlHandler.onLiftRightLeg.RemoveListener(OnLiftRightLeg);
            controlHandler.onReleaseLeftLeg.RemoveListener(OnReleaseLeftLeg);
            controlHandler.onReleaseRightLeg.RemoveListener(OnReleaseRightLeg);
            controlHandler.onCancelLegGrab.RemoveListener(OnCancelLegGrab);
            controlHandler.onDefaultMode.RemoveListener(OnDefaultMode);
            controlHandler.onMouseMove.RemoveListener(OnMouseMove);
        }
    }

    void FixedUpdate()
    {
        // 施加抬腿时的弹簧约束-左
        if (isLeftFootUp && targetObject != null && movingObjectLeft != null)
        {
            ApplySpringForce(movingRbLeft, targetObject.transform.position);
        }

        // 施加抬腿时的弹簧约束-右
        if (isRightFootUp && targetObject != null && movingObjectRight != null)
        {
            ApplySpringForce(movingRbRight, targetObject.transform.position);
        }
    }

    // 事件处理：抬左腿
    private void OnLiftLeftLeg()
    {
        isLeftFootUp = true;
        catchControlLeft.isCacthing = false;
    }

    // 事件处理：抬右腿
    private void OnLiftRightLeg()
    {
        isRightFootUp = true;
        catchControlRight.isCacthing = false;
    }

    // 事件处理：放左腿
    private void OnReleaseLeftLeg()
    {
        isLeftFootUp = false;
        catchControlLeft.isCacthing = true;
        Vector3 force = new Vector3(0,-downForce,0);
        movingRbLeft.AddForce(force);
    }

    // 事件处理：放右腿
    private void OnReleaseRightLeg()
    {
        catchControlRight.isCacthing = true;
        isRightFootUp = false;
        Vector3 force = new Vector3(0, -downForce, 0);
        movingRbRight.AddForce(force);

    }

    // 事件处理：取消腿部抓取
    private void OnCancelLegGrab()
    {
    }

    // 事件处理：无任何按键事件
    private void OnDefaultMode()
    {
    }

    // 事件处理：鼠标移动
    private void OnMouseMove(Vector2 mouseDelta)
    {
        // 获取forwardObject的朝向，并确保其平行于XZ平面
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // 确保朝向平行于XZ平面
        forwardDirection.Normalize();

        // 计算右方向（垂直于forwardDirection）
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // 将鼠标位移向量转换为基于forwardObject朝向的冲量方向
        Vector3 impulseDirection = (rightDirection * mouseDelta.x * mouseSensitivity + forwardDirection * mouseDelta.y * mouseSensitivity).normalized;

        // 根据左键或右键状态应用冲量
        if (isLeftFootUp)
        {
            movingRbLeft.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
        }
        if (isRightFootUp)
        {
            movingRbRight.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
        }
    }

    // 施加弹簧力
    void ApplySpringForce(Rigidbody movingRb, Vector3 targetPosition)
    {
        // 计算目标点和运动对象之间的距离向量
        Vector3 direction = targetPosition - movingRb.position;
        float distance = direction.magnitude;

        // 计算弹簧力
        Vector3 springForceVector = direction.normalized * distance * springForce;

        // 计算阻尼力
        Vector3 dampingForce = -movingRb.velocity * damping;

        // 施加合力
        movingRb.AddForce(springForceVector + dampingForce);
    }
}