using UnityEngine;

public class LimbController : MonoBehaviour
{
    [Header("Bindings")]
    public Transform leftHand;          // 左手绑定对象
    public Transform detectionPlane;    // 检测平面参考对象
    public float moveSpeed = 8f;        // 移动速度
    public float rayDistance = 10f;    // 射线检测距离

    [Header("States")]
    public bool isHandControl = true;   // 当前是否手部控制模式
    private bool useLocalPlane;         // 是否使用局部平面
    private Camera mainCamera;         // 主摄像机引用

    private Vector3 targetPosition;    // 目标位置
    private Plane currentPlane;        // 当前检测平面

    void Start()
    {
        mainCamera = Camera.main;
        InitializeDefaultPlane();
    }

    void Update()
    {
        HandleInput();
        UpdateControlState();
    }

    void LateUpdate()
    {
        if (isHandControl)
        {
            UpdateHandPosition();
        }
    }

    private void InitializeDefaultPlane()
    {
        // 初始化默认XZ平面（使用检测平面的Y坐标）
        currentPlane = new Plane(Vector3.up, detectionPlane.position);
    }

    private void HandleInput()
    {
        // 切换控制模式（中键单击）
        if (Input.GetMouseButtonDown(2))
        {
            isHandControl = !isHandControl;
            // 这里可以添加后续的腿部控制逻辑
        }

        // 右键按下时切换局部平面
        useLocalPlane = Input.GetMouseButton(1);
    }

    private void UpdateControlState()
    {
        if (useLocalPlane)
        {
            // 创建基于角色面朝方向的平面（Y轴方向）
            Vector3 planeNormal = mainCamera.transform.forward;
            Vector3 planePosition = leftHand.position;
            currentPlane = new Plane(planeNormal, planePosition);
        }
        else
        {
            InitializeDefaultPlane();
        }
    }

    private void UpdateHandPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float enter;

        if (currentPlane.Raycast(ray, out enter))
        {
            // 限制射线检测距离
            enter = Mathf.Min(enter, rayDistance);
            targetPosition = ray.GetPoint(enter);
        }

        // 平滑移动手部位置
        leftHand.position = Vector3.Lerp(
            leftHand.position,
            targetPosition,
            Time.deltaTime * moveSpeed
        );
    }

    // 后续可添加的腿部控制方法
    // private void UpdateLegPosition() {...}
}