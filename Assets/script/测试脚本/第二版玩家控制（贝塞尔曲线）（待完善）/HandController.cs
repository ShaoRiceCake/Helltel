using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public GameObject pivotObject; // �������
    public float speed = 4.0f; // �ٶȿ���
    public float radius = 5.0f; // �ƶ��뾶����

    private GameObject controlObject; // �ٿض���
    private Vector3 initialMousePosition; // ��ʼ���λ��
    private bool isRightMouseDown = false; // �Ҽ��Ƿ���

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

        // ����Ҽ����»��ɿ�
        if (Input.GetMouseButtonDown(1)) // 1 ��ʾ�Ҽ�
        {
            isRightMouseDown = true;
            initialMousePosition = Input.mousePosition; // ���ó�ʼ���λ��
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isRightMouseDown = false;
            initialMousePosition = Input.mousePosition; // ���ó�ʼ���λ��
        }

        // ��ȡ��ǰ���λ��
        Vector3 currentMousePosition = Input.mousePosition;

        // �������λ�õı仯��
        Vector3 mouseDelta = currentMousePosition - initialMousePosition;

        // �����Ҽ�״̬ѡ��ӳ�䷽ʽ
        Vector3 localMovement = Vector3.zero;
        if (isRightMouseDown)
        {
            // �Ҽ�����ʱ�����Y��ӳ�䵽�ٿض���ľֲ�Y��
            localMovement = new Vector3(mouseDelta.x, mouseDelta.y, 0) * speed * Time.deltaTime;
        }
        else
        {
            // �Ҽ�δ����ʱ�����X��ӳ�䵽�ٿض���ľֲ�X�ᣬ���Y��ӳ�䵽�ٿض���ľֲ�Z��
            localMovement = new Vector3(mouseDelta.x, 0, mouseDelta.y) * speed * Time.deltaTime;
        }

        // ������ƶ���ת���� pivotObject �ľֲ�����ϵ
        Vector3 localMovementInPivotSpace = pivotObject.transform.InverseTransformDirection(localMovement);

        // ��ȡ�������� pivotObject �ֲ�����ϵ�еĵ�ǰλ��
        Vector3 currentLocalPosition = pivotObject.transform.InverseTransformPoint(controlObject.transform.position);

        // ����������� pivotObject �ֲ�����ϵ�е���λ��
        Vector3 newLocalPosition = currentLocalPosition + localMovementInPivotSpace;

        // ���ƿ������� pivotObject �ֲ�����ϵ�е��ƶ���Χ
        if (newLocalPosition.magnitude > radius)
        {
            newLocalPosition = newLocalPosition.normalized * radius;
        }

        // ����λ�ôӾֲ�����ϵת������������ϵ
        Vector3 newWorldPosition = pivotObject.transform.TransformPoint(newLocalPosition);

        // ���¿���������������ϵ�е�λ��
        controlObject.transform.position = newWorldPosition;

        // ���³�ʼ���λ�ã��Ա���һ֡����仯��
        initialMousePosition = currentMousePosition;
    }

    // ���òٿض���
    public void SetControlObject(GameObject controlObj)
    {
        controlObject = controlObj;
    }
}