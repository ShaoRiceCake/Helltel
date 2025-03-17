
using UnityEngine;
using UnityEngine.Events;

public class MouseControl : MonoBehaviour
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
                    Debug.LogWarning("mouseSensitivity is zero!");
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

    protected bool m_enableMouseControl = true;
    private float m_mouseSensitivity = 100;


    // �¼�����
    public UnityEvent onLeftMouseDown;          // ����������
    public UnityEvent onRightMouseDown;         // ����Ҽ�����
    public UnityEvent onMiddleMouseDown;        // ���ְ���
    public UnityEvent onMouseWheelUp;           // �������Ϲ���
    public UnityEvent onMouseWheelDown;         // �������¹���
    public UnityEvent onBothMouseButtonsDown;   // ���Ҽ�ͬʱ����
    public UnityEvent<Vector2> onMouseMove;     // �������ƶ���

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
        // ����������
        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            onLeftMouseDown.Invoke();
        }

        // ����Ҽ�����
        if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0))
        {
            onRightMouseDown.Invoke();
        }

        // ���ְ���
        if (Input.GetMouseButtonDown(2))
        {
            onMiddleMouseDown.Invoke();
        }

        // ���Ҽ�ͬʱ����
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
