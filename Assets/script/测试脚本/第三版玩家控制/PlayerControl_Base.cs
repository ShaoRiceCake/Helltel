using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerControl_Base : MonoBehaviour
{
    public GameObject forwardObject; // 用于确定正朝向的对象
    public float mouseSensitivity = 1f; // 鼠标灵敏度

    protected void HandleMouseMovement(ref Vector3 movement, bool isRightMouseDown, float speed)
    {
        // 使用相对鼠标移动量
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (isRightMouseDown)
        {
            // 右键按下时：鼠标X/Y映射到局部X/Y轴
            movement = new Vector3(mouseX, mouseY, 0) * speed;
        }
        else
        {
            // 右键未按下时：鼠标X映射到局部X轴，鼠标Y映射到局部Z轴
            movement = new Vector3(mouseX, 0, mouseY) * speed;
        }
    }

    protected Vector3 GetForwardDirection()
    {
        // 获取forwardObject的朝向，并确保其平行于XZ平面
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // 确保朝向平行于XZ平面
        forwardDirection.Normalize();
        return forwardDirection;
    }

    protected Vector3 GetRightDirection(Vector3 forwardDirection)
    {
        // 计算右方向（垂直于forwardDirection）
        return Vector3.Cross(Vector3.up, forwardDirection).normalized;
    }

    protected Vector3 ConvertLocalToWorldMovement(Vector3 localMovement, Vector3 forwardDirection, Vector3 rightDirection)
    {
        // 将局部移动转换为世界坐标系中的移动
        Vector3 worldMovement = forwardDirection * localMovement.z + rightDirection * localMovement.x;
        worldMovement.y = localMovement.y; // 保持Y轴移动不变
        return worldMovement;
    }

    protected void ApplyCylinderConstraint(ref Vector3 position, float cylinderRadius, float cylinderHalfHeight)
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
}
