using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl_HandMove : PlayerControl_Base
{
    public GameObject pivotObject;
    public float speed = 4.0f;
    public float cylinderRadius = 9.0f;    // Բ����뾶
    public float cylinderHalfHeight = 6.0f; // Բ������
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
        // ����Ҽ�״̬
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseDown = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isRightMouseDown = false;
        }

        // ��������ƶ�
        Vector3 localMovement = Vector3.zero;
        HandleMouseMovement(ref localMovement, isRightMouseDown, speed);

        // ��ȡ����
        Vector3 forwardDirection = GetForwardDirection();
        Vector3 rightDirection = GetRightDirection(forwardDirection);

        // ���ֲ��ƶ�ת��Ϊ��������ϵ�е��ƶ�
        Vector3 worldMovement = ConvertLocalToWorldMovement(localMovement, forwardDirection, rightDirection);

        // ������λ��
        Vector3 newWorldPosition = controlObject.transform.position + worldMovement * Time.deltaTime;

        // ����λ��ת����pivot�ľֲ�����ϵ
        Vector3 newLocalPosition = pivotObject.transform.InverseTransformPoint(newWorldPosition);

        // Բ���巶Χ���ƣ��ھֲ�����ϵ�У�
        ApplyCylinderConstraint(ref newLocalPosition, cylinderRadius, cylinderHalfHeight);

        // �����ƺ�ľֲ�λ��ת������������ϵ
        controlObject.transform.position = pivotObject.transform.TransformPoint(newLocalPosition);
    }

    public void SetControlObject(GameObject controlObj)
    {
        controlObject = controlObj;
        isControlReady = false;
    }
}
