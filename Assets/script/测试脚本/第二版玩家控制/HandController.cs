using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public GameObject pivotObject;
    public float speed = 4.0f;
    public float radius = 5.0f;
    public float moveToPivotSpeed = 10.0f;
    public float positionTolerance = 0.1f;
    public float mouseSensitivity = 0.1f; // �������������ϵ��

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
        // ����Ҽ�״̬
        if (Input.GetMouseButtonDown(1))
        {
            isRightMouseDown = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isRightMouseDown = false;
        }

        // ʹ���������ƶ���
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        Vector3 localMovement = Vector3.zero;

        if (isRightMouseDown)
        {
            // �Ҽ�����ʱ�����X/Yӳ�䵽�ֲ�X/Y��
            localMovement = new Vector3(mouseX, mouseY, 0) * speed;
        }
        else
        {
            // �Ҽ�δ����ʱ�����Xӳ�䵽�ֲ�X�ᣬ���Yӳ�䵽�ֲ�Z��
            localMovement = new Vector3(mouseX, 0, mouseY) * speed;
        }

        // ת����pivot�ľֲ�����ϵ
        Vector3 localMovementInPivotSpace = pivotObject.transform.InverseTransformDirection(localMovement);

        // ������λ��
        Vector3 currentLocalPosition = pivotObject.transform.InverseTransformPoint(controlObject.transform.position);
        Vector3 newLocalPosition = currentLocalPosition + localMovementInPivotSpace * Time.deltaTime;

        // �����ƶ��뾶
        if (newLocalPosition.magnitude > radius)
        {
            newLocalPosition = newLocalPosition.normalized * radius;
        }

        // Ӧ����λ��
        controlObject.transform.position = pivotObject.transform.TransformPoint(newLocalPosition);
    }

    public void SetControlObject(GameObject controlObj)
    {
        controlObject = controlObj;
        isControlReady = false;
    }
}