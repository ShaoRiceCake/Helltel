using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerControl_Base : MonoBehaviour
{
    public GameObject forwardObject; // ����ȷ��������Ķ���
    public float mouseSensitivity = 1f; // ���������

    protected void HandleMouseMovement(ref Vector3 movement, bool isRightMouseDown, float speed)
    {
        // ʹ���������ƶ���
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (isRightMouseDown)
        {
            // �Ҽ�����ʱ�����X/Yӳ�䵽�ֲ�X/Y��
            movement = new Vector3(mouseX, mouseY, 0) * speed;
        }
        else
        {
            // �Ҽ�δ����ʱ�����Xӳ�䵽�ֲ�X�ᣬ���Yӳ�䵽�ֲ�Z��
            movement = new Vector3(mouseX, 0, mouseY) * speed;
        }
    }

    protected Vector3 GetForwardDirection()
    {
        // ��ȡforwardObject�ĳ��򣬲�ȷ����ƽ����XZƽ��
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // ȷ������ƽ����XZƽ��
        forwardDirection.Normalize();
        return forwardDirection;
    }

    protected Vector3 GetRightDirection(Vector3 forwardDirection)
    {
        // �����ҷ��򣨴�ֱ��forwardDirection��
        return Vector3.Cross(Vector3.up, forwardDirection).normalized;
    }

    protected Vector3 ConvertLocalToWorldMovement(Vector3 localMovement, Vector3 forwardDirection, Vector3 rightDirection)
    {
        // ���ֲ��ƶ�ת��Ϊ��������ϵ�е��ƶ�
        Vector3 worldMovement = forwardDirection * localMovement.z + rightDirection * localMovement.x;
        worldMovement.y = localMovement.y; // ����Y���ƶ�����
        return worldMovement;
    }

    protected void ApplyCylinderConstraint(ref Vector3 position, float cylinderRadius, float cylinderHalfHeight)
    {
        // ����ˮƽ���ƶ���XZƽ�棩
        Vector2 horizontal = new Vector2(position.x, position.z);
        if (horizontal.magnitude > cylinderRadius)
        {
            horizontal = horizontal.normalized * cylinderRadius;
            position.x = horizontal.x;
            position.z = horizontal.y;
        }

        // ���ƴ�ֱ�߶ȣ�Y�ᣩ
        position.y = Mathf.Clamp(position.y, -cylinderHalfHeight, cylinderHalfHeight);
    }
}
