using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl_HandMove : PlayerControl_Base
{
    public GameObject pivotObject;
    public float speed = 4.0f;
    public float cylinderRadius = 9.0f;    // 圆柱体半径
    public float cylinderHalfHeight = 6.0f; // 圆柱体半高
    public float moveToPivotSpeed = 10.0f;
    public float positionTolerance = 0.1f;

    private GameObject controlObject;
    private bool isRightMouseDown = false;
    private bool isControlReady = false;

    void Update()
    {
        if (controlObject == null || pivotObject == null || forwardObject == null)
        {
            Debug.LogWarning("Control object, pivot object, or forward object is not set.");
            return;
        }

        if (!isControlReady)
        {
            MoveControlObjectToPivot();
            return;
        }

        HandleMouseControl();
    }

    private void MoveControlObjectToPivot()
    {
        float distance = Vector3.Distance(controlObject.transform.position, pivotObject.transform.position);

        if (distance <= positionTolerance)
        {
            controlObject.transform.position = pivotObject.transform.position;
            isControlReady = true;
            return;
        }

        controlObject.transform.position = Vector3.MoveTowards(
            controlObject.transform.position,
            pivotObject.transform.position,
            moveToPivotSpeed * Time.deltaTime
        );
    }

    private void HandleMouseControl()
    {
        // 检测右键状态
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseDown = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isRightMouseDown = false;
        }

        // 处理鼠标移动
        Vector3 localMovement = Vector3.zero;
        HandleMouseMovement(ref localMovement, isRightMouseDown, speed);

        // 获取朝向
        Vector3 forwardDirection = GetForwardDirection();
        Vector3 rightDirection = GetRightDirection(forwardDirection);

        // 将局部移动转换为世界坐标系中的移动
        Vector3 worldMovement = ConvertLocalToWorldMovement(localMovement, forwardDirection, rightDirection);

        // 计算新位置
        Vector3 newWorldPosition = controlObject.transform.position + worldMovement * Time.deltaTime;

        // 将新位置转换到pivot的局部坐标系
        Vector3 newLocalPosition = pivotObject.transform.InverseTransformPoint(newWorldPosition);

        // 圆柱体范围限制（在局部坐标系中）
        ApplyCylinderConstraint(ref newLocalPosition, cylinderRadius, cylinderHalfHeight);

        // 将限制后的局部位置转换回世界坐标系
        controlObject.transform.position = pivotObject.transform.TransformPoint(newLocalPosition);
    }

    public void SetControlObject(GameObject controlObj)
    {
        controlObject = controlObj;
        isControlReady = false;
    }
}
