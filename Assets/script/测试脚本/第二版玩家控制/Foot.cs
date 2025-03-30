using UnityEngine;

public class Foot : MonoBehaviour
{
    public GameObject targetObject; // 目标点对象
    public GameObject movingObjectLeft; // 左键控制的运动对象
    public GameObject movingObjectRight; // 右键控制的运动对象
    public GameObject forwardObject; // 新增：用于确定正朝向的对象
    public float springForce = 10f; // 弹簧力度
    public float damping = 0.6f; // 阻尼系数
    public float moveSpeed = 15f; // 移动到碰撞落点的速度
    public float raycastDistance = 10f; // 射线检测的最大距离
    public float mouseDetectionTimeInterval = 0.015f; // 鼠标移动检测单位时间间隔（秒）
    public float impulseCoefficient = 0.3f; // 冲量系数
    public float landingThreshold = 0.5f; // 落地判定阈值
    public float mouseSensitivity = 1f; // 鼠标灵敏度
    public LayerMask raycastMask; // 射线检测遮罩

    private bool isSpringActiveLeft = false; // 左键是否按下
    private bool isSpringActiveRight = false; // 右键是否按下
    private bool isObjectFixedLeft = false; // 左键控制的运动对象是否被固定
    private bool isObjectFixedRight = false; // 右键控制的运动对象是否被固定
    private Vector3 targetPositionLeft; // 左键控制的运动对象的射线检测目标落点
    private Vector3 targetPositionRight; // 右键控制的运动对象的射线检测目标落点
    private float timeCounter = 0f; // 时间计数器
    private Rigidbody movingRbLeft; // 左键控制的运动对象的刚体组件
    private Rigidbody movingRbRight; // 右键控制的运动对象的刚体组件

    void Start()
    {
        // 获取运动对象的刚体组件
        movingRbLeft = movingObjectLeft.GetComponent<Rigidbody>();
        movingRbRight = movingObjectRight.GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 检测鼠标左右键是否同时按下
        if (Input.GetMouseButtonDown(0) && Input.GetMouseButtonDown(1))
        {
            // 优先处理左键逻辑
            isSpringActiveLeft = true;
            UnfixObject(movingRbLeft, ref isObjectFixedLeft); // 解除左键控制的运动对象的固定
        }
        else
        {
            // 如果右键单独按下，并且左键没有被按下
            if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0))
            {
                isSpringActiveRight = true;
                UnfixObject(movingRbRight, ref isObjectFixedRight); // 解除右键控制的运动对象的固定
            }

            // 如果左键单独按下，并且右键没有被按下
            if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
            {
                isSpringActiveLeft = true;
                UnfixObject(movingRbLeft, ref isObjectFixedLeft); // 解除左键控制的运动对象的固定
            }

            // 如果右键释放
            if (Input.GetMouseButtonUp(1))
            {
                isSpringActiveRight = false;
                MoveToTargetPosition(movingRbRight, targetPositionRight); // 右键松开时移动到碰撞落点
            }

            // 如果左键释放
            if (Input.GetMouseButtonUp(0))
            {
                isSpringActiveLeft = false;
                MoveToTargetPosition(movingRbLeft, targetPositionLeft); // 左键松开时移动到碰撞落点
            }
        }

        // 持续检测脚下的碰撞点
        DetectGround(movingObjectLeft, ref targetPositionLeft); // 检测左键控制的运动对象的落点
        DetectGround(movingObjectRight, ref targetPositionRight); // 检测右键控制的运动对象的落点

        // 鼠标移动检测单位时间逻辑
        if (isSpringActiveLeft || isSpringActiveRight)
        {
            timeCounter += Time.deltaTime;
            if (timeCounter >= mouseDetectionTimeInterval)
            {
                timeCounter = 0f; // 重置时间计数器
                DetectMouseMovement(); // 检测鼠标位移并应用冲量
            }
        }

        // 检测是否落地并固定对象
        if (!isSpringActiveLeft && IsObjectLanded(movingObjectLeft, targetPositionLeft))
        {
            FixObject(movingRbLeft, ref isObjectFixedLeft); // 固定左键控制的运动对象
        }
        if (!isSpringActiveRight && IsObjectLanded(movingObjectRight, targetPositionRight))
        {
            FixObject(movingRbRight, ref isObjectFixedRight); // 固定右键控制的运动对象
        }
    }

    void FixedUpdate()
    {
        // 左键控制的运动对象的弹簧约束
        if (isSpringActiveLeft && targetObject != null && movingObjectLeft != null)
        {
            ApplySpringForce(movingRbLeft, targetObject.transform.position);
        }

        // 右键控制的运动对象的弹簧约束
        if (isSpringActiveRight && targetObject != null && movingObjectRight != null)
        {
            ApplySpringForce(movingRbRight, targetObject.transform.position);
        }
    }

    void DetectGround(GameObject movingObject, ref Vector3 targetPosition)
    {
        // 从运动对象向下发射射线
        Ray ray = new Ray(movingObject.transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, raycastMask))
        {
            // 如果射线检测到碰撞，则记录碰撞点
            targetPosition = hit.point;
            Debug.DrawLine(ray.origin, hit.point, Color.green); // 可视化射线
        }
        else
        {
            // 如果未检测到碰撞，重置目标位置
            targetPosition = movingObject.transform.position;
        }
    }

    void MoveToTargetPosition(Rigidbody movingRb, Vector3 targetPosition)
    {
        // 计算目标位置与当前位置的差值
        Vector3 direction = targetPosition - movingRb.position;
        float distance = direction.magnitude;

        // 如果距离大于一个很小的阈值，则开始移动
        if (distance > landingThreshold)
        {
            movingRb.velocity = direction.normalized * moveSpeed; // 设置速度
        }
    }

    void DetectMouseMovement()
    {
        // 使用相对鼠标移动量
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 获取forwardObject的朝向，并确保其平行于XZ平面
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // 确保朝向平行于XZ平面
        forwardDirection.Normalize();

        // 计算右方向（垂直于forwardDirection）
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // 将鼠标位移向量转换为基于forwardObject朝向的冲量方向
        Vector3 impulseDirection = (rightDirection * mouseX + forwardDirection * mouseY).normalized;

        // 根据左键或右键状态应用冲量
        if (isSpringActiveLeft)
        {
            movingRbLeft.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
        }
        if (isSpringActiveRight)
        {
            movingRbRight.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
        }
    }

    bool IsObjectLanded(GameObject movingObject, Vector3 targetPosition)
    {
        // 检测运动对象是否接近目标位置
        float distance = Vector3.Distance(movingObject.transform.position, targetPosition);
        return distance <= landingThreshold;
    }

    void FixObject(Rigidbody movingRb, ref bool isObjectFixed)
    {
        if (!isObjectFixed)
        {
            // 清除速度和受力
            movingRb.velocity = Vector3.zero;
            movingRb.angularVelocity = Vector3.zero;
            movingRb.isKinematic = true; // 设置为运动学刚体，固定位置

            isObjectFixed = true;
        }
    }

    void UnfixObject(Rigidbody movingRb, ref bool isObjectFixed)
    {
        if (isObjectFixed)
        {
            // 解除固定
            movingRb.isKinematic = false; // 设置为动力学刚体
            isObjectFixed = false;
        }
    }

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