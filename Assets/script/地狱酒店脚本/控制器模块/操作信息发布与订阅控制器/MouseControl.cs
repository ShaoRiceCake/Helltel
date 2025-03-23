using UnityEngine;
using UnityEngine.Events;

public class MouseControl : MonoBehaviour
{
    // 鼠标灵敏度
    public float MouseSensitivity
    {
        get => m_mouseSensitivity;
        set
        {
            if (value != m_mouseSensitivity)
            {
                m_mouseSensitivity = value;
                if (m_mouseSensitivity == 0)
                {
                    Debug.LogWarning("mouseSensitivity is zero!");
                }
            }
        }
    }

    // 鼠标输入启用与否
    public bool EnableMouseControl
    {
        get => m_enableMouseControl;
        set => m_enableMouseControl = value;
    }

    private bool m_enableMouseControl = true;
    private float m_mouseSensitivity = 1;

    // 事件定义
    public UnityEvent onLeftMouseDown;          // 鼠标左键按下
    public UnityEvent onRightMouseDown;         // 鼠标右键按下
    public UnityEvent onLeftMouseUp;          // 鼠标左键按下
    public UnityEvent onRightMouseUp;         // 鼠标右键按下
    public UnityEvent onMiddleMouseDown;        // 滚轮按下
    public UnityEvent onMouseWheelUp;           // 滚轮向上滚动
    public UnityEvent onMouseWheelDown;         // 滚轮向下滚动
    public UnityEvent onBothMouseButtonsDown;   // 左右键同时按下
    public UnityEvent<Vector2> onMouseMoveFixedUpdate; // 固定时间步长相对运动
    public UnityEvent<Vector2> onMouseMoveUpdate;     // 每帧相对运动
    public UnityEvent onNoMouseButtonDown; // 鼠标无按键操作

    private Vector2 lastMousePosition;

    void Awake()
    {
        // 初始化所有事件
        if (onLeftMouseDown == null) onLeftMouseDown = new UnityEvent();
        if (onRightMouseDown == null) onRightMouseDown = new UnityEvent();
        if (onLeftMouseUp == null) onLeftMouseUp = new UnityEvent();
        if (onRightMouseUp == null) onRightMouseUp = new UnityEvent();
        if (onMiddleMouseDown == null) onMiddleMouseDown = new UnityEvent();
        if (onMouseWheelUp == null) onMouseWheelUp = new UnityEvent();
        if (onMouseWheelDown == null) onMouseWheelDown = new UnityEvent();
        if (onBothMouseButtonsDown == null) onBothMouseButtonsDown = new UnityEvent();
        if (onNoMouseButtonDown == null) onNoMouseButtonDown = new UnityEvent();
        if (onMouseMoveFixedUpdate == null) onMouseMoveFixedUpdate = new UnityEvent<Vector2>();
        if (onMouseMoveUpdate == null) onMouseMoveUpdate = new UnityEvent<Vector2>();
    }

    void Update()
    {
        if (!m_enableMouseControl)
            return;

        HandleMouseButtons();
        HandleMouseWheel();
        HandleMouseMovementUpdate(); // 每帧处理鼠标移动

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            onNoMouseButtonDown?.Invoke();
        }
    }

    private void FixedUpdate()
    {
        if (!m_enableMouseControl)
            return;

        HandleMouseMovementFixedUpdate(); // 固定时间步长处理鼠标移动
    }

    void HandleMouseButtons()
    {
        // 鼠标左键按下且右键未按下
        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            onLeftMouseDown?.Invoke();
        }

        // 鼠标右键按下且左键未按下
        if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0))
        {
            onRightMouseDown?.Invoke();
        }

        // 鼠标左键抬起且右键未按下
        if (Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1))
        {
            onLeftMouseUp?.Invoke();
        }

        // 鼠标右键抬起且左键未按下
        if (Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0))
        {
            onRightMouseUp?.Invoke();
        }


        // 滚轮按下
        if (Input.GetMouseButtonDown(2))
        {
            onMiddleMouseDown?.Invoke();
        }

        // 左右键同时按下
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            onBothMouseButtonsDown?.Invoke();
        }
    }

    void HandleMouseWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0)
        {
            onMouseWheelUp?.Invoke();
        }
        else if (scroll < 0)
        {
            onMouseWheelDown?.Invoke();
        }
    }

    void HandleMouseMovementFixedUpdate()
    {
        Vector2 currentMousePosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (currentMousePosition != lastMousePosition)
        {
            Vector2 mouseDelta = currentMousePosition * m_mouseSensitivity * Time.fixedDeltaTime;
            onMouseMoveFixedUpdate?.Invoke(mouseDelta);
        }
        lastMousePosition = currentMousePosition;
    }

    void HandleMouseMovementUpdate()
    {
        Vector2 currentMousePosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (currentMousePosition != lastMousePosition)
        {
            Vector2 mouseDelta = currentMousePosition * m_mouseSensitivity;
            onMouseMoveUpdate?.Invoke(mouseDelta);
        }
        lastMousePosition = currentMousePosition;
    }
}