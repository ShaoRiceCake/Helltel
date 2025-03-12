using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public GameObject pivotObject;
    public float speed = 4.0f;
    public float radius = 5.0f;
    public float moveToPivotSpeed = 10.0f;
    public float positionTolerance = 0.1f;
    public float mouseSensitivity = 0.1f; // 新增鼠标灵敏度系数

    private GameObject controlObject;
    private bool isRightMouseDown = false;
    private bool isControlReady = false;

    void Update()
    {
        if (controlObject == null || pivotObject == null)
        {
            Debug.LogWarning("Control object or pivot object is not set.");
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

        // 转换到pivot的局部坐标系
        Vector3 localMovementInPivotSpace = pivotObject.transform.InverseTransformDirection(localMovement);

        // 计算新位置
        Vector3 currentLocalPosition = pivotObject.transform.InverseTransformPoint(controlObject.transform.position);
        Vector3 newLocalPosition = currentLocalPosition + localMovementInPivotSpace * Time.deltaTime;

        // 限制移动半径
        if (newLocalPosition.magnitude > radius)
        {
            newLocalPosition = newLocalPosition.normalized * radius;
        }

        // 应用新位置
        controlObject.transform.position = pivotObject.transform.TransformPoint(newLocalPosition);
    }

    public void SetControlObject(GameObject controlObj)
    {
        controlObject = controlObj;
        isControlReady = false;
    }
}