using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public GameObject pivotObject;
    public GameObject forwardObject; // ����������ȷ��������Ķ���
    public float speed = 4.0f;
    public float cylinderRadius = 9.0f;    // Բ����뾶
    public float cylinderHalfHeight = 6.0f; // Բ������
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

        // ��ȡforwardObject�ĳ��򣬲�ȷ����ƽ����XZƽ��
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // ȷ������ƽ����XZƽ��
        forwardDirection.Normalize();

        // �����ҷ��򣨴�ֱ��forwardDirection��
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // ���ֲ��ƶ�ת��Ϊ��������ϵ�е��ƶ�
        Vector3 worldMovement = forwardDirection * localMovement.z + rightDirection * localMovement.x;
        worldMovement.y = localMovement.y; // ����Y���ƶ�����

        // ������λ��
        Vector3 newWorldPosition = controlObject.transform.position + worldMovement * Time.deltaTime;

        // ����λ��ת����pivot�ľֲ�����ϵ
        Vector3 newLocalPosition = pivotObject.transform.InverseTransformPoint(newWorldPosition);

        // Բ���巶Χ���ƣ��ھֲ�����ϵ�У�
        ApplyCylinderConstraint(ref newLocalPosition);

        // �����ƺ�ľֲ�λ��ת������������ϵ
        controlObject.transform.position = pivotObject.transform.TransformPoint(newLocalPosition);
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

    public void SetControlObject(GameObject controlObj)
    {
        controlObject = controlObj;
        isControlReady = false;
    }
}