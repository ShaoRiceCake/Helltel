using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public abstract class InputControl_Base : ControllerBase
{
    // �豸�����¼�
    public UnityEvent<InputDevice> onDeviceConnected;
    public UnityEvent<InputDevice> onDeviceDisconnected;

    // ��ǰ���õ��豸
    protected InputDevice activeDevice;

    // �����豸�б�
    protected InputControl_Base[] childDevices;

    protected override void InitializeController()
    {
        // �����豸�仯
        InputSystem.onDeviceChange += OnDeviceChange;

        // ��ʼ��ʱ��⵱ǰ���ӵ��豸
        CheckConnectedDevices();

        // ��ȡ���������豸
        childDevices = GetComponents<InputControl_Base>();

        // ���������¼�
        InputSystem.onEvent += OnInputEvent;
    }

    protected override void DestroyController()
    {
        // ȡ�������豸�仯
        InputSystem.onDeviceChange -= OnDeviceChange;

        // ȡ�����������¼�
        InputSystem.onEvent -= OnInputEvent;
    }

    // �豸�仯�ص�
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

    // �豸����
    private void OnDeviceConnected(InputDevice device)
    {
        CustomLogger.Log($"�豸�����ӣ�{device.name}", LogType.Log);
        onDeviceConnected?.Invoke(device);

        // �����ǰû�������豸�������������ӵ��豸
        if (activeDevice == null)
        {
            SetActiveDevice(device);
        }
        else
        {
            // ����Ѿ��������豸�������������豸���ȴ����ѡ��
            EnableAllDevices();
        }
    }

    // �豸�Ͽ�
    private void OnDeviceDisconnected(InputDevice device)
    {
        CustomLogger.Log($"�豸�ѶϿ���{device.name}", LogType.Log);
        onDeviceDisconnected?.Invoke(device);

        // ����Ͽ����豸�ǵ�ǰ���õ��豸�������л��������豸
        if (activeDevice == device)
        {
            activeDevice = null;
            EnableAllDevices();
        }
    }

    // ��⵱ǰ���ӵ��豸
    private void CheckConnectedDevices()
    {
        foreach (var device in InputSystem.devices)
        {
            OnDeviceConnected(device);
        }
    }

    // �����¼��ص�
    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // �����ǰû�������豸�����������¼����������豸�����л������豸
        if (activeDevice == null || activeDevice != device)
        {
            SetActiveDevice(device);
        }
    }

    // ���õ�ǰ���õ��豸
    private void SetActiveDevice(InputDevice device)
    {
        activeDevice = device;

        // ���õ�ǰ�豸�����������豸
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

    // ���������豸
    private void EnableAllDevices()
    {
        foreach (var childDevice in childDevices)
        {
            childDevice.EnableDevice();
        }
    }

    // ��ȡ��ǰ�豸
    protected abstract InputDevice GetDevice();

    // �����豸
    protected abstract void EnableDevice();

    // �����豸
    protected abstract void DisableDevice();
}