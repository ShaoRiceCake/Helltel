using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MouseControl : InputControl_Base
{
    // ���������
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

    // ��������������
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
        // �����������
        m_enableMouseControl = true;
    }

    protected override void DisableDevice()
    {
        // �����������
        m_enableMouseControl = false;
    }

    // �¼�����
    public UnityEvent onLeftMouseDown;          // ����������
    public UnityEvent onRightMouseDown;        // ����Ҽ�����
    public UnityEvent onMiddleMouseDown;       // ���ְ���
    public UnityEvent onMouseWheelUp;          // �������Ϲ���
    public UnityEvent onMouseWheelDown;        // �������¹���
    public UnityEvent onBothMouseButtonsDown;  // ���Ҽ�ͬʱ����
    public UnityEvent<Vector2> onMouseMove;    // �������ƶ���

    private Vector2 lastMousePosition;

    protected override void InitializeController()
    {
        base.InitializeController(); // ���û����ʼ��
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

        // ����������
        if (mouse.leftButton.wasPressedThisFrame && !mouse.rightButton.isPressed)
        {
            onLeftMouseDown.Invoke();
        }

        // ����Ҽ�����
        if (mouse.rightButton.wasPressedThisFrame && !mouse.leftButton.isPressed)
        {
            onRightMouseDown.Invoke();
        }

        // ���ְ���
        if (mouse.middleButton.wasPressedThisFrame)
        {
            onMiddleMouseDown.Invoke();
        }

        // ���Ҽ�ͬʱ����
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