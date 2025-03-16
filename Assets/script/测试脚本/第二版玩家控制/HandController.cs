using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public GameObject pivotObject;
    public GameObject forwardObject; // 新增：用于确定正朝向的对象
    public float speed = 4.0f;
    public float cylinderRadius = 9.0f;    // 圆柱体半径
    public float cylinderHalfHeight = 6.0f; // 圆柱体半高
    public float moveToPivotSpeed = 10.0f;
    public float positionTolerance = 0.1f;
    public float mouseSensitivity = 10f;

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

        // 使用相对鼠标移动量
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        Vector3 localMovement = Vector3.zero;

        if (isRightMouseDown)
        {
            // 右键按下时：鼠标X/Y映射到局部X/Y轴
            localMovement = new Vector3(mouseX, mouseY, 0) * speed;
        }
        else
        {
            // 右键未按下时：鼠标X映射到局部X轴，鼠标Y映射到局部Z轴
            localMovement = new Vector3(mouseX, 0, mouseY) * speed;
        }

        // 获取forwardObject的朝向，并确保其平行于XZ平面
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // 确保朝向平行于XZ平面
        forwardDirection.Normalize();

        // 计算右方向（垂直于forwardDirection）
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // 将局部移动转换为世界坐标系中的移动
        Vector3 worldMovement = forwardDirection * localMovement.z + rightDirection * localMovement.x;
        worldMovement.y = localMovement.y; // 保持Y轴移动不变

        // 计算新位置
        Vector3 newWorldPosition = controlObject.transform.position + worldMovement * Time.deltaTime;

        // 将新位置转换到pivot的局部坐标系
        Vector3 newLocalPosition = pivotObject.transform.InverseTransformPoint(newWorldPosition);

        // 圆柱体范围限制（在局部坐标系中）
        ApplyCylinderConstraint(ref newLocalPosition);

        // 将限制后的局部位置转换回世界坐标系
        controlObject.transform.position = pivotObject.transform.TransformPoint(newLocalPosition);
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

    public void SetControlObject(GameObject controlObj)
    {
        controlObject = controlObj;
        isControlReady = false;
    }
}