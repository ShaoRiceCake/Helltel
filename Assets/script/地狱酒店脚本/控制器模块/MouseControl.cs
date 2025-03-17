
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
    protected bool EnableMouseControl
    {
        get => m_enableMouseControl;
        set => m_enableMouseControl = value;
    }

    protected bool m_enableMouseControl = true;
    private float m_mouseSensitivity = 100;


    // 事件定义
    public UnityEvent onLeftMouseDown;          // 鼠标左键按下
    public UnityEvent onRightMouseDown;         // 鼠标右键按下
    public UnityEvent onMiddleMouseDown;        // 滚轮按下
    public UnityEvent onMouseWheelUp;           // 滚轮向上滚动
    public UnityEvent onMouseWheelDown;         // 滚轮向下滚动
    public UnityEvent onBothMouseButtonsDown;   // 左右键同时按下
    public UnityEvent<Vector2> onMouseMove;     // 相对鼠标移动量

    private Vector2 lastMousePosition;

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
        // 鼠标左键按下
        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            onLeftMouseDown.Invoke();
        }

        // 鼠标右键按下
        if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0))
        {
            onRightMouseDown.Invoke();
        }

        // 滚轮按下
        if (Input.GetMouseButtonDown(2))
        {
            onMiddleMouseDown.Invoke();
        }

        // 左右键同时按下
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            onBothMouseButtonsDown.Invoke();
        }
    }

    void HandleMouseWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

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
        Vector2 currentMousePosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (currentMousePosition != lastMousePosition)
        {
            Vector2 mouseDelta = currentMousePosition * m_mouseSensitivity * Time.deltaTime;
            onMouseMove.Invoke(mouseDelta);
        }

        lastMousePosition = currentMousePosition;
    }
}
