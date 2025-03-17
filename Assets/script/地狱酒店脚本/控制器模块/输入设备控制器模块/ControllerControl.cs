using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ControllerControl : InputControl_Base
{
    public UnityEvent onButtonSouthPressed;       // A (Xbox) / Cross (PS5)
    public UnityEvent onButtonEastPressed;        // B (Xbox) / Circle (PS5)
    public UnityEvent onButtonWestPressed;        // X (Xbox) / Square (PS5)
    public UnityEvent onButtonNorthPressed;       // Y (Xbox) / Triangle (PS5)
    public UnityEvent onLeftShoulderPressed;      // LB (Xbox) / L1 (PS5)
    public UnityEvent onRightShoulderPressed;     // RB (Xbox) / R1 (PS5)
    public UnityEvent onLeftTriggerPressed;       // LT (Xbox) / L2 (PS5)
    public UnityEvent onRightTriggerPressed;      // RT (Xbox) / R2 (PS5)
    public UnityEvent onStartButtonPressed;      // Start (Xbox) / Options (PS5)
    public UnityEvent onSelectButtonPressed;      // Select (Xbox) / Share (PS5)
    public UnityEvent onLeftStickPressed;         // Left Stick Press (Xbox/PS5)
    public UnityEvent onRightStickPressed;        // Right Stick Press (Xbox/PS5)
    public UnityEvent<Vector2> onLeftStickMove;   // Left Stick Movement
    public UnityEvent<Vector2> onRightStickMove;  // Right Stick Movement
    public UnityEvent<Vector2> onDpadMove;        // D-pad Movement

    private Gamepad gamepad;

    protected override InputDevice GetDevice()
    {
        return gamepad;
    }

    protected override void EnableDevice()
    {
        // 启用控制器输入
        enabled = true;
    }

    protected override void DisableDevice()
    {
        // 禁用控制器输入
        enabled = false;
    }

    protected override void InitializeController()
    {
        gamepad = Gamepad.current;
        if (gamepad == null)
        {
            CustomLogger.Log("No gamepad connected.",LogType.Warning);
        }
    }

    protected override void DestroyController()
    {
        // 清理资源
        gamepad = null;
    }

    void Update()
    {
        if (gamepad == null)
            return;

        HandleButtons();
        HandleSticks();
        HandleDpad();
    }

    void HandleButtons()
    {
        // A (Xbox) / Cross (PS5)
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            onButtonSouthPressed.Invoke();
        }

        // B (Xbox) / Circle (PS5)
        if (gamepad.buttonEast.wasPressedThisFrame)
        {
            onButtonEastPressed.Invoke();
        }

        // X (Xbox) / Square (PS5)
        if (gamepad.buttonWest.wasPressedThisFrame)
        {
            onButtonWestPressed.Invoke();
        }

        // Y (Xbox) / Triangle (PS5)
        if (gamepad.buttonNorth.wasPressedThisFrame)
        {
            onButtonNorthPressed.Invoke();
        }

        // LB (Xbox) / L1 (PS5)
        if (gamepad.leftShoulder.wasPressedThisFrame)
        {
            onLeftShoulderPressed.Invoke();
        }

        // RB (Xbox) / R1 (PS5)
        if (gamepad.rightShoulder.wasPressedThisFrame)
        {
            onRightShoulderPressed.Invoke();
        }

        // LT (Xbox) / L2 (PS5)
        if (gamepad.leftTrigger.wasPressedThisFrame)
        {
            onLeftTriggerPressed.Invoke();
        }

        // RT (Xbox) / R2 (PS5)
        if (gamepad.rightTrigger.wasPressedThisFrame)
        {
            onRightTriggerPressed.Invoke();
        }

        // Start (Xbox) / Options (PS5)
        if (gamepad.startButton.wasPressedThisFrame)
        {
            onStartButtonPressed.Invoke();
        }

        // Select (Xbox) / Share (PS5)
        if (gamepad.selectButton.wasPressedThisFrame)
        {
            onSelectButtonPressed.Invoke();
        }

        // Left Stick Press (Xbox/PS5)
        if (gamepad.leftStickButton.wasPressedThisFrame)
        {
            onLeftStickPressed.Invoke();
        }

        // Right Stick Press (Xbox/PS5)
        if (gamepad.rightStickButton.wasPressedThisFrame)
        {
            onRightStickPressed.Invoke();
        }
    }

    void HandleSticks()
    {
        // Left Stick Movement
        Vector2 leftStickValue = gamepad.leftStick.ReadValue();
        if (leftStickValue.magnitude > 0.1f) // 防止微小移动触发事件
        {
            onLeftStickMove.Invoke(leftStickValue);
        }

        // Right Stick Movement
        Vector2 rightStickValue = gamepad.rightStick.ReadValue();
        if (rightStickValue.magnitude > 0.1f) // 防止微小移动触发事件
        {
            onRightStickMove.Invoke(rightStickValue);
        }
    }

    void HandleDpad()
    {
        // D-pad Movement
        Vector2 dpadValue = gamepad.dpad.ReadValue();
        if (dpadValue.magnitude > 0.1f) // 防止微小移动触发事件
        {
            onDpadMove.Invoke(dpadValue);
        }
    }
}