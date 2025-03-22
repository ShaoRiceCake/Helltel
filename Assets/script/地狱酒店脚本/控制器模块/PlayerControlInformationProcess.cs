using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerControlInformationProcess : MonoBehaviour
{
    // ����ģʽö��
    public enum ControlMode
    {
        LegControl, // �Ȳ�����
        HandControl // �ֲ�����
    }

    // ��ǰ����ģʽ
    private ControlMode m_currentControlMode = ControlMode.LegControl;

    // ���������
    private MouseControl m_mouseControl;

    // ��װ����¼�
    public UnityEvent onLiftLeftLeg; // ̧������
    public UnityEvent onLiftRightLeg; // ̧������
    public UnityEvent onReleaseLeftLeg; // �ͷ�����
    public UnityEvent onReleaseRightLeg; // �ͷ�����
    public UnityEvent onCancelLegGrab; // ȡ���Ȳ�ץȡ

    public UnityEvent onLiftLeftHand; // ̧������
    public UnityEvent onLiftRightHand; // ̧������
    public UnityEvent onReleaseLeftHand; // �ͷ�����
    public UnityEvent onReleaseRightHand; // �ͷ�����
    public UnityEvent onCancelHandGrab; // ȡ���ֲ�ץȡ

    public UnityEvent onSwitchControlMode; // �л���-�ȿ���
    public UnityEvent<Vector2> onMouseMove; // ������λ��
    public UnityEvent onDefaultMode; // Ĭ��ģʽ

    void Awake()
    {
        // ��ʼ�������¼�
        if (onLiftLeftLeg == null) onLiftLeftLeg = new UnityEvent();
        if (onLiftRightLeg == null) onLiftRightLeg = new UnityEvent();
        if (onReleaseLeftLeg == null) onReleaseLeftLeg = new UnityEvent();
        if (onReleaseRightLeg == null) onReleaseRightLeg = new UnityEvent();
        if (onCancelLegGrab == null) onCancelLegGrab = new UnityEvent();

        if (onLiftLeftHand == null) onLiftLeftHand = new UnityEvent();
        if (onLiftRightHand == null) onLiftRightHand = new UnityEvent();
        if (onReleaseLeftHand == null) onReleaseLeftHand = new UnityEvent();
        if (onReleaseRightHand == null) onReleaseRightHand = new UnityEvent();
        if (onCancelHandGrab == null) onCancelHandGrab = new UnityEvent();

        if (onSwitchControlMode == null) onSwitchControlMode = new UnityEvent();
        if (onMouseMove == null) onMouseMove = new UnityEvent<Vector2>();
        if (onDefaultMode == null) onDefaultMode = new UnityEvent();

        // �Զ���� MouseControl ���
        m_mouseControl = gameObject.AddComponent<MouseControl>();

        // ����¼�����
        m_mouseControl.onLeftMouseDown.AddListener(OnLeftMouseDown);
        m_mouseControl.onRightMouseDown.AddListener(OnRightMouseDown);
        m_mouseControl.onLeftMouseUp.AddListener(OnLeftMouseUp);
        m_mouseControl.onRightMouseUp.AddListener(OnRightMouseUp);
        m_mouseControl.onBothMouseButtonsDown.AddListener(OnBothMouseButtonsDown);
        m_mouseControl.onMiddleMouseDown.AddListener(OnMiddleMouseDown);
        m_mouseControl.onMouseMove.AddListener(OnMouseMove);
        m_mouseControl.onNoMouseButtonDown.AddListener(OnNoMouseButtonDown);
    }

    void OnDestroy()
    {
        // ȡ�������¼�
        if (m_mouseControl != null)
        {
            m_mouseControl.onLeftMouseDown.RemoveListener(OnLeftMouseDown);
            m_mouseControl.onRightMouseDown.RemoveListener(OnRightMouseDown);
            m_mouseControl.onLeftMouseUp.RemoveListener(OnLeftMouseUp);
            m_mouseControl.onRightMouseUp.RemoveListener(OnRightMouseUp);
            m_mouseControl.onBothMouseButtonsDown.RemoveListener(OnBothMouseButtonsDown);
            m_mouseControl.onMiddleMouseDown.RemoveListener(OnMiddleMouseDown);
            m_mouseControl.onMouseMove.RemoveListener(OnMouseMove);
            m_mouseControl.onNoMouseButtonDown.RemoveListener(OnNoMouseButtonDown);
        }
    }

    // �����������¼�����
    private void OnLeftMouseDown()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onLiftLeftLeg?.Invoke(); // ̧������
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onLiftLeftHand?.Invoke(); // ̧������
        }
    }

    // ����Ҽ������¼�����
    private void OnRightMouseDown()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onLiftRightLeg?.Invoke(); // ̧������
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onLiftRightHand?.Invoke(); // ̧������
        }
    }

    // ������̧���¼�����
    private void OnLeftMouseUp()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onReleaseLeftLeg?.Invoke(); // �ͷ�����
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onReleaseLeftHand?.Invoke(); // �ͷ�����
        }
    }

    // ����Ҽ�̧���¼�����
    private void OnRightMouseUp()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onReleaseRightLeg?.Invoke(); // �ͷ�����
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onReleaseRightHand?.Invoke(); // �ͷ�����
        }
    }

    // ������Ҽ�ͬʱ�����¼�����
    private void OnBothMouseButtonsDown()
    {
        if (m_currentControlMode == ControlMode.LegControl)
        {
            onCancelLegGrab?.Invoke(); // ȡ���Ȳ�ץȡ
        }
        else if (m_currentControlMode == ControlMode.HandControl)
        {
            onCancelHandGrab?.Invoke(); // ȡ���ֲ�ץȡ
        }
    }

    // ����м������¼������л�����ģʽ��
    private void OnMiddleMouseDown()
    {
        // �л�����ģʽ
        m_currentControlMode = (m_currentControlMode == ControlMode.LegControl) ? ControlMode.HandControl : ControlMode.LegControl;
        onSwitchControlMode?.Invoke(); // �����л�����ģʽ�¼�
    }

    // ����ƶ��¼�����
    private void OnMouseMove(Vector2 mouseDelta)
    {
        onMouseMove?.Invoke(mouseDelta); // ��������ƶ���

    }

    // ����ް��������¼�����
    private void OnNoMouseButtonDown()
    {
        onDefaultMode?.Invoke(); // ����Ĭ��ģʽ
    }

    public void SetSensitivity(float newSensitivity)
    {
        if (m_mouseControl)
        {
            m_mouseControl.MouseSensitivity *= newSensitivity;
        }
    }
}