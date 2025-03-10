using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("跟随设置")]
    public Transform target;          // 要控制的物体
    public float mouseSensitivity = 0.01f; // 鼠标灵敏度
    public float maxRadius = 5f;       // 最大活动半径

    [Header("参考点设置")]
    public Transform centerPoint;     // 活动中心参考点

    private Vector3 referencePosition;
    private Vector3 lastMousePosition;
    private bool isXYMode;            // 当前是否处于XY模式

    void Start()
    {
        InitializeReferences();
        UpdateLockPosition();
    }

    void Update()
    {
        UpdateReferences();
        HandleModeSwitch();
        HandleMouseInput();
        ClampToRadius();
        UpdateLockPosition();
    }

    void InitializeReferences()
    {
        referencePosition = centerPoint != null ? centerPoint.position : transform.position;
        lastMousePosition = Input.mousePosition;
    }

    void UpdateReferences()
    {
        if (centerPoint != null)
        {
            referencePosition = centerPoint.position;
        }
    }

    void HandleModeSwitch()
    {
        // 右键按下时切换为XY模式，松开恢复XZ模式
        isXYMode = Input.GetMouseButton(1);
    }

    void HandleMouseInput()
    {
        Vector3 currentMousePos = Input.mousePosition;
        Vector3 mouseDelta = currentMousePos - lastMousePosition;
        lastMousePosition = currentMousePos;

        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 cameraUp = Camera.main.transform.up;

        // 根据模式计算平面位移
        Vector3 worldDelta = (cameraRight * mouseDelta.x + cameraUp * mouseDelta.y) * mouseSensitivity;

        if (isXYMode)
        {
            // XY模式：保留X/Y轴位移，Z轴由后续锁定
            worldDelta.z = 0;
        }
        else
        {
            // XZ模式：保留X/Z轴位移，Y轴由后续锁定
            worldDelta.y = 0;
        }

        target.position += worldDelta;
    }

    void ClampToRadius()
    {
        Vector3 offset = target.position - referencePosition;
        float currentDistance = 0f;

        if (isXYMode)
        {
            // 计算XY平面距离
            currentDistance = Mathf.Sqrt(offset.x * offset.x + offset.y * offset.y);
        }
        else
        {
            // 计算XZ平面距离
            currentDistance = Mathf.Sqrt(offset.x * offset.x + offset.z * offset.z);
        }

        if (currentDistance > maxRadius)
        {
            if (isXYMode)
            {
                Vector2 clamped = new Vector2(offset.x, offset.y).normalized * maxRadius;
                target.position = new Vector3(
                    referencePosition.x + clamped.x,
                    referencePosition.y + clamped.y,
                    target.position.z
                );
            }
            else
            {
                Vector2 clamped = new Vector2(offset.x, offset.z).normalized * maxRadius;
                target.position = new Vector3(
                    referencePosition.x + clamped.x,
                    target.position.y,
                    referencePosition.z + clamped.y
                );
            }
        }
    }

    void UpdateLockPosition()
    {
        if (isXYMode)
        {
            // XY模式：锁定Z轴到参考点
            target.position = new Vector3(
                target.position.x,
                target.position.y,
                referencePosition.z
            );
        }
        else
        {
            // XZ模式：锁定Y轴到参考点
            target.position = new Vector3(
                target.position.x,
                referencePosition.y,
                target.position.z
            );
        }
    }
}