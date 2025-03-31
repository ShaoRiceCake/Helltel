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
    public bool EnableMouseControl
    {
        get => m_enableMouseControl;
        set => m_enableMouseControl = value;
    }

    private bool m_enableMouseControl = true;
    private float m_mouseSensitivity = 1;

    // �¼�����
    public UnityEvent onLeftMouseDown;          // ����������
    public UnityEvent onRightMouseDown;         // ����Ҽ�����
    public UnityEvent onLeftMouseUp;          // ����������
    public UnityEvent onRightMouseUp;         // ����Ҽ�����
    public UnityEvent onMiddleMouseDown;        // ���ְ���
    public UnityEvent onMouseWheelUp;           // �������Ϲ���
    public UnityEvent onMouseWheelDown;         // �������¹���
    public UnityEvent onBothMouseButtonsDown;   // ���Ҽ�ͬʱ����
    public UnityEvent<Vector2> onMouseMoveFixedUpdate; // �̶�ʱ�䲽������˶�
    public UnityEvent<Vector2> onMouseMoveUpdate;     // ÿ֡����˶�
    public UnityEvent onNoMouseButtonDown; // ����ް�������

    private Vector2 lastMousePosition;

    void Awake()
    {
        // ��ʼ�������¼�
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
        HandleMouseMovementUpdate(); // ÿ֡��������ƶ�

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            onNoMouseButtonDown?.Invoke();
        }
    }

    private void FixedUpdate()
    {
        if (!m_enableMouseControl)
            return;

        HandleMouseMovementFixedUpdate(); // �̶�ʱ�䲽����������ƶ�
    }

    void HandleMouseButtons()
    {
        // �������������Ҽ�δ����
        if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
        {
            onLeftMouseDown?.Invoke();
        }

        // ����Ҽ����������δ����
        if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0))
        {
            onRightMouseDown?.Invoke();
        }

        // ������̧�����Ҽ�δ����
        if (Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1))
        {
            onLeftMouseUp?.Invoke();
        }

        // ����Ҽ�̧�������δ����
        if (Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0))
        {
            onRightMouseUp?.Invoke();
        }


        // ���ְ���
        if (Input.GetMouseButtonDown(2))
        {
            onMiddleMouseDown?.Invoke();
        }

        // ���Ҽ�ͬʱ����
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