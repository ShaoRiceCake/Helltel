using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandControl2 : MonoBehaviour
{
    public GameObject pivotObject;
    public GameObject forwardObject; // ����ȷ��������Ķ���
    public float speed = 4.0f;
    public float cylinderRadius = 9.0f;    // Բ����뾶
    public float cylinderHalfHeight = 6.0f; // Բ������
    public float mouseSensitivity = 10f;
    public float heightIncreaseSpeed = 10.0f; // �߶������ٶ�

    private GameObject leftControlObject;
    private GameObject rightControlObject;
    private GameObject activeControlObject; // ��ǰ��Ŀ��ƶ���
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
        // ������״̬
        if (Input.GetMouseButtonDown(0))
        {
            isLeftMouseDown = true;
            isRightMouseDown = false;
            SwitchControlObject(leftControlObject);
        }

        // ����Ҽ�״̬
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseDown = true;
            isLeftMouseDown = false;
            SwitchControlObject(rightControlObject);
        }

        if (activeControlObject == null) return;

        // ʹ���������ƶ���
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ��ȡforwardObject�ĳ��򣬲�ȷ����ƽ����XZƽ��
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // ȷ������ƽ����XZƽ��
        forwardDirection.Normalize();

        // �����ҷ��򣨴�ֱ��forwardDirection��
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // ������ƶ�ת��Ϊ��������ϵ�е��ƶ�
        Vector3 worldMovement = forwardDirection * mouseY + rightDirection * mouseX;
        worldMovement *= speed * Time.deltaTime;

        // ������λ��
        Vector3 newWorldPosition = activeControlObject.transform.position + worldMovement;

        // ����λ��ת����pivot�ľֲ�����ϵ
        Vector3 newLocalPosition = pivotObject.transform.InverseTransformPoint(newWorldPosition);

        // Բ���巶Χ���ƣ��ھֲ�����ϵ�У�
        ApplyCylinderConstraint(ref newLocalPosition);

        // �����ƺ�ľֲ�λ��ת������������ϵ
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

        // �����¸߶�
        Vector3 newLocalPosition = pivotObject.transform.InverseTransformPoint(activeControlObject.transform.position);
        newLocalPosition.y += heightChange;

        // Բ���巶Χ���ƣ��ھֲ�����ϵ�У�
        ApplyCylinderConstraint(ref newLocalPosition);

        // �����ƺ�ľֲ�λ��ת������������ϵ
        activeControlObject.transform.position = pivotObject.transform.TransformPoint(newLocalPosition);
    }

    private void ApplyCylinderConstraint(ref Vector3 position)
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

    public void SetControlObjects(GameObject leftControlObj, GameObject rightControlObj)
    {
        leftControlObject = leftControlObj;
        rightControlObject = rightControlObj;

        // Ĭ�Ͽ�������
        SwitchControlObject(rightControlObject);
    }

    private void SwitchControlObject(GameObject newControlObject)
    {
        if (activeControlObject != null && activeControlObject != newControlObject)
        {
            Destroy(activeControlObject); // ���ٵ�ǰ���ƶ���
        }

        activeControlObject = newControlObject;
    }
}