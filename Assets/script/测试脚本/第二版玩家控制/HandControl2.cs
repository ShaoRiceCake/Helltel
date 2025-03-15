using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControl2 : MonoBehaviour
{
    public GameObject pivotObject;
    public GameObject forwardObject; // 用于确定正朝向的对象
    public float speed = 4.0f;
    public float cylinderRadius = 9.0f;    // 圆柱体半径
    public float cylinderHalfHeight = 6.0f; // 圆柱体半高
    public float mouseSensitivity = 10f;
    public float heightIncreaseSpeed = 10.0f; // 高度提升速度

    private GameObject leftControlObject;
    private GameObject rightControlObject;
    private GameObject activeControlObject; // 当前活动的控制对象
    private bool isLeftMouseDown = false;
    private bool isRightMouseDown = false;

    void Update()
    {
        if (pivotObject == null || forwardObject == null)
        {
            Debug.LogWarning("Pivot object or forward object is not set.");
            return;
        }

        HandleMouseControl();
        HandleHeightControl();
    }

    private void HandleMouseControl()
    {
        // 检测左键状态
        if (Input.GetMouseButtonDown(0))
        {
            isLeftMouseDown = true;
            isRightMouseDown = false;
            SwitchControlObject(leftControlObject);
        }

        // 检测右键状态
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseDown = true;
            isLeftMouseDown = false;
            SwitchControlObject(rightControlObject);
        }

        if (activeControlObject == null) return;

        // 使用相对鼠标移动量
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 获取forwardObject的朝向，并确保其平行于XZ平面
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // 确保朝向平行于XZ平面
        forwardDirection.Normalize();

        // 计算右方向（垂直于forwardDirection）
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // 将鼠标移动转换为世界坐标系中的移动
        Vector3 worldMovement = forwardDirection * mouseY + rightDirection * mouseX;
        worldMovement *= speed * Time.deltaTime;

        // 计算新位置
        Vector3 newWorldPosition = activeControlObject.transform.position + worldMovement;

        // 将新位置转换到pivot的局部坐标系
        Vector3 newLocalPosition = pivotObject.transform.InverseTransformPoint(newWorldPosition);

        // 圆柱体范围限制（在局部坐标系中）
        ApplyCylinderConstraint(ref newLocalPosition);

        // 将限制后的局部位置转换回世界坐标系
        activeControlObject.transform.position = pivotObject.transform.TransformPoint(newLocalPosition);
    }

    private void HandleHeightControl()
    {
        if (activeControlObject == null) return;

        float heightChange = 0;

        if (isLeftMouseDown || isRightMouseDown)
        {
            heightChange = heightIncreaseSpeed * Time.deltaTime;
        }

        // 计算新高度
        Vector3 newLocalPosition = pivotObject.transform.InverseTransformPoint(activeControlObject.transform.position);
        newLocalPosition.y += heightChange;

        // 圆柱体范围限制（在局部坐标系中）
        ApplyCylinderConstraint(ref newLocalPosition);

        // 将限制后的局部位置转换回世界坐标系
        activeControlObject.transform.position = pivotObject.transform.TransformPoint(newLocalPosition);
    }

    private void ApplyCylinderConstraint(ref Vector3 position)
    {
        // 限制水平面移动（XZ平面）
        Vector2 horizontal = new Vector2(position.x, position.z);
        if (horizontal.magnitude > cylinderRadius)
        {
            horizontal = horizontal.normalized * cylinderRadius;
            position.x = horizontal.x;
            position.z = horizontal.y;
        }

        // 限制垂直高度（Y轴）
        position.y = Mathf.Clamp(position.y, -cylinderHalfHeight, cylinderHalfHeight);
    }

    public void SetControlObjects(GameObject leftControlObj, GameObject rightControlObj)
    {
        leftControlObject = leftControlObj;
        rightControlObject = rightControlObj;

        // 默认控制右手
        SwitchControlObject(rightControlObject);
    }

    private void SwitchControlObject(GameObject newControlObject)
    {
        if (activeControlObject != null && activeControlObject != newControlObject)
        {
            Destroy(activeControlObject); // 销毁当前控制对象
        }

        activeControlObject = newControlObject;
    }
}