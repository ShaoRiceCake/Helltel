using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public GameObject pivotObject; // 定点对象
    public GameObject controlObject; // 操控对象
    public float speed = 4.0f; // 速度控制
    public float radius = 5.0f; // 移动半径限制

    private Vector3 initialMousePosition; // 初始鼠标位置
    private bool isRightMouseDown = false; // 右键是否按下

    void Start()
    {
        initialMousePosition = Input.mousePosition;

    }

    void Update()
    {
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

        // 计算操控对象的新位置
        Vector3 newLocalPosition = controlObject.transform.localPosition + localMovement;

        // 限制操控对象的移动范围在半径内
        if (newLocalPosition.magnitude > radius)
        {
            newLocalPosition = newLocalPosition.normalized * radius;
        }

        // 更新操控对象的位置
        controlObject.transform.localPosition = newLocalPosition;

        // 更新初始鼠标位置，以便下一帧计算变化量
        initialMousePosition = currentMousePosition;
    }
}