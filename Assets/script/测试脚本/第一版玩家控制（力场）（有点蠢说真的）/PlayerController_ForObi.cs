using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_ForObi : MonoBehaviour
{
    [Header("����ģʽ����")]
    public HandController handController;
    public FootController footController;
    public bool startWithFootControl = true;

    private bool isFootControlActive;

    void Start()
    {
        InitializeControlMode();
    }

    void Update()
    {
        HandleControlSwitch();
    }

    void InitializeControlMode()
    {
        isFootControlActive = startWithFootControl;
        UpdateControllerStates();
    }

    void HandleControlSwitch()
    {
        if (Input.GetMouseButtonDown(2)) // ����м�����
        {
            isFootControlActive = !isFootControlActive;
            UpdateControllerStates();
        }
    }

    void UpdateControllerStates()
    {
        footController.enabled = isFootControlActive;
        handController.enabled = !isFootControlActive;
        // ��ѡ���л�ʱ����״̬
        if (isFootControlActive)
        {
        }
        else
        {
            handController.mouseObj.transform.position = new Vector3(this.transform.position.x+1.0f, this.transform.position.y, this.transform.position.z);
        }
    }
}
