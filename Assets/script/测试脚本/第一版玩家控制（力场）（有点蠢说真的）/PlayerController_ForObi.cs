using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_ForObi : MonoBehaviour
{
    [Header("控制模式设置")]
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
        if (Input.GetMouseButtonDown(2)) // 鼠标中键按下
        {
            isFootControlActive = !isFootControlActive;
            UpdateControllerStates();
        }
    }

    void UpdateControllerStates()
    {
        footController.enabled = isFootControlActive;
        handController.enabled = !isFootControlActive;
        // 可选：切换时重置状态
        if (isFootControlActive)
        {
        }
        else
        {
            handController.mouseObj.transform.position = new Vector3(this.transform.position.x+1.0f, this.transform.position.y, this.transform.position.z);
        }
    }
}
