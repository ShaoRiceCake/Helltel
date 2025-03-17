using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class KeyboardControl : InputControl_Base
{
    // 键盘输入启用与否
    protected bool EnableKeyboardControl
    {
        get => m_enableKeyboardControl;
        set => m_enableKeyboardControl = value;
    }

    protected bool m_enableKeyboardControl = false;

    // 事件定义
    public UnityEvent<Key> onKeyPressed;  // 按键按下
    public UnityEvent<Key> onKeyReleased; // 按键释放

    protected override InputDevice GetDevice()
    {
        return Keyboard.current;
    }

    protected override void EnableDevice()
    {
        // 启用键盘输入
        m_enableKeyboardControl = true;
    }

    protected override void DisableDevice()
    {
        // 禁用键盘输入
        m_enableKeyboardControl = false;
    }

    protected override void InitializeController()
    {
        base.InitializeController(); // 调用基类初始化
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

        // 遍历所有按键
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