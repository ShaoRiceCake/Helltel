using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public GameObject pivotObject; // 定点对象
    public float speed = 4.0f; // 速度控制
    public float radius = 5.0f; // 移动半径限制

    private GameObject controlObject; // 操控对象
    private Vector3 initialMousePosition; // 初始鼠标位置
    private bool isRightMouseDown = false; // 右键是否按下

    void Start()
    {
        initialMousePosition = Input.mousePosition;
    }

    void Update()
    {
        if (controlObject == null || pivotObject == null)
        {
            Debug.LogWarning("Control object or pivot object is not set.");
            return;
        }

        // 检测右键按下或松开
        if (Input.GetMouseButtonDown(1)) // 1 表示右键
        {
            isRightMouseDown = true;
            initialMousePosition = Input.mousePosition; // 重置初始鼠标位置
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isRightMouseDown = false;
            initialMousePosition = Input.mousePosition; // 重置初始鼠标位置
        }

        // 获取当前鼠标位置
        Vector3 currentMousePosition = Input.mousePosition;

        // 计算鼠标位置的变化量
        Vector3 mouseDelta = currentMousePosition - initialMousePosition;

        // 根据右键状态选择映射方式
        Vector3 localMovement = Vector3.zero;
        if (isRightMouseDown)
        {
            // 右键按下时，鼠标Y轴映射到操控对象的局部Y轴
            localMovement = new Vector3(mouseDelta.x, mouseDelta.y, 0) * speed * Time.deltaTime;
        }
        else
        {
            // 右键未按下时，鼠标X轴映射到操控对象的局部X轴，鼠标Y轴映射到操控对象的局部Z轴
            localMovement = new Vector3(mouseDelta.x, 0, mouseDelta.y) * speed * Time.deltaTime;
        }

        // 将鼠标移动量转换到 pivotObject 的局部坐标系
        Vector3 localMovementInPivotSpace = pivotObject.transform.InverseTransformDirection(localMovement);

        // 获取控制球在 pivotObject 局部坐标系中的当前位置
        Vector3 currentLocalPosition = pivotObject.transform.InverseTransformPoint(controlObject.transform.position);

        // 计算控制球在 pivotObject 局部坐标系中的新位置
        Vector3 newLocalPosition = currentLocalPosition + localMovementInPivotSpace;

        // 限制控制球在 pivotObject 局部坐标系中的移动范围
        if (newLocalPosition.magnitude > radius)
        {
            newLocalPosition = newLocalPosition.normalized * radius;
        }

        // 将新位置从局部坐标系转换回世界坐标系
        Vector3 newWorldPosition = pivotObject.transform.TransformPoint(newLocalPosition);

        // 更新控制球在世界坐标系中的位置
        controlObject.transform.position = newWorldPosition;

        // 更新初始鼠标位置，以便下一帧计算变化量
        initialMousePosition = currentMousePosition;
    }

    // 设置操控对象
    public void SetControlObject(GameObject controlObj)
    {
        controlObject = controlObj;
    }
}