using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("跟随设置")]
    public Transform target;          // 要控制的物体
    public float mouseSensitivity = 0.01f; // 鼠标灵敏度
    public float maxRadius = 5f;       // 最大活动半径

    [Header("参考点设置")]
    public Transform centerPoint;     // 活动中心参考点（默认为自身位置）

    private Vector3 referencePosition; // 基准位置
    private Vector3 lastMousePosition;

    void Start()
    {
        InitializeReferences();
        LockYPosition();
    }

    void Update()
    {
        UpdateReferencePosition();
        HandleMouseInput();
        ClampToRadius();
        LockYPosition();
    }

    void InitializeReferences()
    {
        // 初始化基准位置
        referencePosition = centerPoint != null ? centerPoint.position : transform.position;
        lastMousePosition = Input.mousePosition;
    }

    void UpdateReferencePosition()
    {
        // 动态更新参考点位置
        if (centerPoint != null)
        {
            referencePosition = centerPoint.position;
        }
    }

    void HandleMouseInput()
    {
        Vector3 currentMousePos = Input.mousePosition;
        Vector3 mouseDelta = currentMousePos - lastMousePosition;
        lastMousePosition = currentMousePos;

        // 将鼠标移动转换为世界空间位移
        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 cameraUp = Camera.main.transform.up;

        // 创建XZ平面位移向量
        Vector3 worldDelta = new Vector3(
            mouseDelta.x * cameraRight.x + mouseDelta.y * cameraUp.x,
            0,
            mouseDelta.x * cameraRight.z + mouseDelta.y * cameraUp.z
        ) * mouseSensitivity;

        target.position += worldDelta;
    }

    void ClampToRadius()
    {
        // 计算与参考点的水平距离
        Vector3 horizontalOffset = target.position - referencePosition;
        horizontalOffset.y = 0;

        if (horizontalOffset.magnitude > maxRadius)
        {
            target.position = referencePosition + horizontalOffset.normalized * maxRadius;
        }
    }

    void LockYPosition()
    {
        // 保持初始Y坐标不变
        target.position = new Vector3(
            target.position.x,
            referencePosition.y,
            target.position.z
        );
    }
}