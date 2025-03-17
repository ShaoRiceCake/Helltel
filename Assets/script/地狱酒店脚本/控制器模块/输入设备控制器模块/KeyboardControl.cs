using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class KeyboardControl : InputControl_Base
{
    // ���������������
    protected bool EnableKeyboardControl
    {
        get => m_enableKeyboardControl;
        set => m_enableKeyboardControl = value;
    }

    protected bool m_enableKeyboardControl = false;

    // �¼�����
    public UnityEvent<Key> onKeyPressed;  // ��������
    public UnityEvent<Key> onKeyReleased; // �����ͷ�

    protected override InputDevice GetDevice()
    {
        return Keyboard.current;
    }

    protected override void EnableDevice()
    {
        // ���ü�������
        m_enableKeyboardControl = true;
    }

    protected override void DisableDevice()
    {
        // ���ü�������
        m_enableKeyboardControl = false;
    }

    protected override void InitializeController()
    {
        base.InitializeController(); // ���û����ʼ��
    }

    void Update()
    {
        if (!m_enableKeyboardControl)
            return;

        HandleKeyboardInput();
    }

    void HandleKeyboardInput()
    {
        var keyboard = Keyboard.current;

        // �������а���
        foreach (var key in keyboard.allKeys)
        {
            if (key.wasPressedThisFrame)
            {
                onKeyPressed.Invoke(key.keyCode);
            }
            if (key.wasReleasedThisFrame)
            {
                onKeyReleased.Invoke(key.keyCode);
            }
        }
    }
}