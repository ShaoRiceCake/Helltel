using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MouseControl : InputControl_Base
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
                    CustomLogger.Log("MouseSensitivity is zero!", LogType.Warning);
                }
            }
        }
    }

    // 鼠标输入启用与否
    protected bool EnableMouseControl
    {
        get => m_enableMouseControl;
        set => m_enableMouseControl = value;
    }

    protected bool m_enableMouseControl = false;
    private float m_mouseSensitivity = 100;


    protected override InputDevice GetDevice()
    {
        return Mouse.current;
    }

    protected override void EnableDevice()
    {
        // 启用鼠标输入
        m_enableMouseControl = true;
    }

    protected override void DisableDevice()
    {
        // 禁用鼠标输入
        m_enableMouseControl = false;
    }

    // 事件定义
    public UnityEvent onLeftMouseDown;          // 鼠标左键按下
    public UnityEvent onRightMouseDown;        // 鼠标右键按下
    public UnityEvent onMiddleMouseDown;       // 滚轮按下
    public UnityEvent onMouseWheelUp;          // 滚轮向上滚动
    public UnityEvent onMouseWheelDown;        // 滚轮向下滚动
    public UnityEvent onBothMouseButtonsDown;  // 左右键同时按下
    public UnityEvent<Vector2> onMouseMove;    // 相对鼠标移动量

    private Vector2 lastMousePosition;

    protected override void InitializeController()
    {
        base.InitializeController(); // 调用基类初始化
    }

    void Update()
    {
        if (!m_enableMouseControl)
            return;

        HandleMouseButtons();
        HandleMouseWheel();
        HandleMouseMovement();
    }

    void HandleMouseButtons()
    {
        var mouse = Mouse.current;

        // 鼠标左键按下
        if (mouse.leftButton.wasPressedThisFrame && !mouse.rightButton.isPressed)
        {
            onLeftMouseDown.Invoke();
        }

        // 鼠标右键按下
        if (mouse.rightButton.wasPressedThisFrame && !mouse.leftButton.isPressed)
        {
            onRightMouseDown.Invoke();
        }

        // 滚轮按下
        if (mouse.middleButton.wasPressedThisFrame)
        {
            onMiddleMouseDown.Invoke();
        }

        // 左右键同时按下
        if (mouse.leftButton.isPressed && mouse.rightButton.isPressed)
        {
            onBothMouseButtonsDown.Invoke();
        }
    }

    void HandleMouseWheel()
    {
        var mouse = Mouse.current;
        float scroll = mouse.scroll.y.ReadValue();

        if (scroll > 0)
        {
            onMouseWheelUp.Invoke();
        }
        else if (scroll < 0)
        {
            onMouseWheelDown.Invoke();
        }
    }

    void HandleMouseMovement()
    {
        var mouse = Mouse.current;
        Vector2 currentMousePosition = mouse.delta.ReadValue();

        if (currentMousePosition != lastMousePosition)
        {
            Vector2 mouseDelta = currentMousePosition * m_mouseSensitivity * Time.deltaTime;
            onMouseMove.Invoke(mouseDelta);
        }

        lastMousePosition = currentMousePosition;
    }
}