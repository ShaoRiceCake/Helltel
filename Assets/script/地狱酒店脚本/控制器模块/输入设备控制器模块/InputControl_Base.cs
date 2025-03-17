using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public abstract class InputControl_Base : ControllerBase
{
    // 设备连接事件
    public UnityEvent<InputDevice> onDeviceConnected;
    public UnityEvent<InputDevice> onDeviceDisconnected;

    // 当前启用的设备
    protected InputDevice activeDevice;

    // 子类设备列表
    protected InputControl_Base[] childDevices;

    protected override void InitializeController()
    {
        // 监听设备变化
        InputSystem.onDeviceChange += OnDeviceChange;

        // 初始化时检测当前连接的设备
        CheckConnectedDevices();

        // 获取所有子类设备
        childDevices = GetComponents<InputControl_Base>();

        // 监听输入事件
        InputSystem.onEvent += OnInputEvent;
    }

    protected override void DestroyController()
    {
        // 取消监听设备变化
        InputSystem.onDeviceChange -= OnDeviceChange;

        // 取消监听输入事件
        InputSystem.onEvent -= OnInputEvent;
    }

    // 设备变化回调
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Reconnected:
                OnDeviceConnected(device);
                break;
            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
                OnDeviceDisconnected(device);
                break;
        }
    }

    // 设备连接
    private void OnDeviceConnected(InputDevice device)
    {
        CustomLogger.Log($"设备已连接：{device.name}", LogType.Log);
        onDeviceConnected?.Invoke(device);

        // 如果当前没有启用设备，则启用新连接的设备
        if (activeDevice == null)
        {
            SetActiveDevice(device);
        }
        else
        {
            // 如果已经有启用设备，则启用所有设备，等待玩家选择
            EnableAllDevices();
        }
    }

    // 设备断开
    private void OnDeviceDisconnected(InputDevice device)
    {
        CustomLogger.Log($"设备已断开：{device.name}", LogType.Log);
        onDeviceDisconnected?.Invoke(device);

        // 如果断开的设备是当前启用的设备，则尝试切换到其他设备
        if (activeDevice == device)
        {
            activeDevice = null;
            EnableAllDevices();
        }
    }

    // 检测当前连接的设备
    private void CheckConnectedDevices()
    {
        foreach (var device in InputSystem.devices)
        {
            OnDeviceConnected(device);
        }
    }

    // 输入事件回调
    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // 如果当前没有启用设备，或者输入事件来自其他设备，则切换到该设备
        if (activeDevice == null || activeDevice != device)
        {
            SetActiveDevice(device);
        }
    }

    // 设置当前启用的设备
    private void SetActiveDevice(InputDevice device)
    {
        activeDevice = device;

        // 启用当前设备，禁用其他设备
        foreach (var childDevice in childDevices)
        {
            if (childDevice.GetDevice() == device)
            {
                childDevice.EnableDevice();
            }
            else
            {
                childDevice.DisableDevice();
            }
        }
    }

    // 启用所有设备
    private void EnableAllDevices()
    {
        foreach (var childDevice in childDevices)
        {
            childDevice.EnableDevice();
        }
    }

    // 获取当前设备
    protected abstract InputDevice GetDevice();

    // 启用设备
    protected abstract void EnableDevice();

    // 禁用设备
    protected abstract void DisableDevice();
}