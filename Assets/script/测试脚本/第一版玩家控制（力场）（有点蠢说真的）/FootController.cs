using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootController : MonoBehaviour
{
    [Header("核心控制")]
    private Transform target;         // 被控制的物体（动态设置为激活的对象的Transform）
    public Transform mousePointer;   // 三维空间中的目标点

    [Header("控制参数")]
    public float activationSpeed = 1f;    // 激活跟随的鼠标速度（像素/秒）
    public float rotationForce = 150f;       // 旋转作用力
    public float dampingForce = 3f;         // 旋转阻尼力
    public float baseHeight = 1f;           // 基准高度偏移
    public float mouseSensitivity = 0.01f;  // 鼠标移动灵敏度（降低灵敏度）

    [Header("空间设置")]
    public float followDistance = 5f;       // 默认跟随距离
    public LayerMask collisionMask;         // 碰撞检测层

    [Header("对象设置")]
    public GameObject object1;              // 对象1
    public GameObject object2;              // 对象2

    private Rigidbody rb;
    private Vector3 defaultPosition;
    private Vector3 lastMousePos;
    private bool isDragging;
    private GameObject activeObject; // 当前激活的对象

    void Start()
    {
        InitializeComponents();
        SetupPhysics();
        DeactivateAllObjects(); // 初始时失活所有对象
    }

    void Update()
    {
        defaultPosition = GetDefaultPosition();

        // 动态设置 target 为激活对象的 Transform
        if (activeObject != null)
        {
            target = activeObject.transform;
        }
        else
        {
            target = null; // 没有激活对象时，target 为 null
        }

        HandleInput();
        UpdatePointerPosition();
    }

    void FixedUpdate()
    {
        if (target != null) // 确保 target 不为 null
        {
            ApplyRotationPhysics();
        }
    }

    void InitializeComponents()
    {
        mousePointer.position = defaultPosition;
        lastMousePos = Input.mousePosition;  // 初始化鼠标位置
    }

    void SetupPhysics()
    {
        if (target != null)
        {
            rb = target.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezePosition;
            rb.maxAngularVelocity = 20f;
        }
    }

    void HandleInput()
    {
        // 左键按下时激活对象1
        if (Input.GetMouseButtonDown(0))
        {
            SetActiveObject(object1);
        }
        // 左键松开时失活对象1
        if (Input.GetMouseButtonUp(0))
        {
            if (activeObject == object1)
            {
                DeactivateAllObjects();
            }
        }

        // 右键按下时激活对象2
        if (Input.GetMouseButtonDown(1))
        {
            SetActiveObject(object2);
        }
        // 右键松开时失活对象2
        if (Input.GetMouseButtonUp(1))
        {
            if (activeObject == object2)
            {
                DeactivateAllObjects();
            }
        }

        // 如果同时按下左键和右键，确保只有一个对象激活
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            if (activeObject != object1 && activeObject != object2)
            {
                SetActiveObject(object1); // 默认激活对象1
            }
        }
    }

    void SetActiveObject(GameObject newActiveObject)
    {
        if (activeObject != null && activeObject != newActiveObject)
        {
            activeObject.SetActive(false); // 失活当前对象
        }
        activeObject = newActiveObject;
        activeObject.SetActive(true); // 激活新对象

        // 更新 target 和 Rigidbody
        if (activeObject != null)
        {
            target = activeObject.transform;
            rb = target.GetComponent<Rigidbody>();
            SetupPhysics();
        }
    }

    void DeactivateAllObjects()
    {
        if (object1 != null) object1.SetActive(false);
        if (object2 != null) object2.SetActive(false);
        activeObject = null;
        target = null; // 没有激活对象时，target 为 null
    }

    public void ResetPointer()
    {
        mousePointer.position = defaultPosition;  // 瞬间归位
        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    void UpdatePointerPosition()
    {
        if (target == null) return; // 没有激活对象时跳过

        Vector3 currentMousePos = Input.mousePosition;
        Vector3 mouseDelta = currentMousePos - lastMousePos;  // 计算鼠标的相对位移
        lastMousePos = currentMousePos;

        if (mouseDelta.magnitude > activationSpeed * Time.deltaTime)
        {
            Update3DPointerPosition(mouseDelta);
        }
        else if (!isDragging)
        {
            ResetPointer();  // 非拖动状态下瞬间归位
        }
    }

    void Update3DPointerPosition(Vector3 mouseDelta)
    {
        // 将鼠标的屏幕空间位移转换为世界空间位移
        Vector3 worldDelta = Camera.main.transform.right * mouseDelta.x + Camera.main.transform.up * mouseDelta.y;
        worldDelta *= mouseSensitivity;  // 应用灵敏度

        // 限制Y坐标为挂载对象的Y坐标
        worldDelta.y = 0;

        // 更新鼠标指针的位置
        mousePointer.position += worldDelta;

        // 限制鼠标指针在平面内运动
        mousePointer.position = new Vector3(
            mousePointer.position.x,
            mousePointer.position.y,
            mousePointer.position.z
        );
    }

    void ApplyRotationPhysics()
    {
        if (target == null) return; // 没有激活对象时跳过

        Vector3 targetDirection = mousePointer.position - target.position;
        if (targetDirection == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion rotationDelta = targetRotation * Quaternion.Inverse(target.rotation);

        rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);

        Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad * rotationForce);
        angularVelocity -= rb.angularVelocity * dampingForce;

        rb.AddTorque(angularVelocity, ForceMode.Acceleration);
    }

    Vector3 GetDefaultPosition()
    {
        Vector3 Eco = new Vector3(this.transform.position.x, this.transform.position.y + baseHeight, this.transform.position.z);

        if (target == null) return Eco; // 没有激活对象时返回默认值

        return Eco;
    }
}