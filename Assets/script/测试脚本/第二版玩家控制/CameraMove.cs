using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float rotationSpeed = 100f; // ��ת�ٶ�
    public float minXAngle = -35f; // ��X����ת����С�Ƕ�
    public float maxXAngle = 35f;  // ��X����ת�����Ƕ�

    public float zoomSpeed = 10f; // �ӽ�������Զ���ٶ�
    public float minZoomDistance = 2f; // ����ӽǾ���
    public float maxZoomDistance = 10f; // ��Զ�ӽǾ���
    public float zoomSmoothTime = 0.2f; // �ӽ����ŵ�ƽ��ʱ��

    private float currentXRotation = 0f; // ��ǰ��X�����ת�Ƕ�
    private Vector3 initialLocalPosition; // �������ʼ�ľֲ�λ��
    private float targetZoomDistance; // Ŀ���ӽǾ���
    private float currentZoomVelocity; // ��ǰ�ӽ����ŵĲ�ֵ�ٶ�

    void Start()
    {
        // ��¼�������ʼ�ľֲ�λ��
        initialLocalPosition = transform.localPosition;
        // ��ʼ��Ŀ���ӽǾ���Ϊ��ǰ����
        targetZoomDistance = -transform.localPosition.z;
    }

    void Update()
    {
        // ����A������Y����ʱ����ת
        if (Input.GetKey(KeyCode.A))
        {
            transform.parent.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
        }

        // ����D������Y��˳ʱ����ת
        if (Input.GetKey(KeyCode.D))
        {
            transform.parent.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }

        // ����W������X����ʱ����ת
        if (Input.GetKey(KeyCode.W))
        {
            float deltaRotation = -rotationSpeed * Time.deltaTime;
            currentXRotation += deltaRotation;
            currentXRotation = Mathf.Clamp(currentXRotation, minXAngle, maxXAngle); // ���ƽǶ�
            transform.localEulerAngles = new Vector3(currentXRotation, 0, 0);
        }

        // ����S������X��˳ʱ����ת
        if (Input.GetKey(KeyCode.S))
        {
            float deltaRotation = rotationSpeed * Time.deltaTime;
            currentXRotation += deltaRotation;
            currentXRotation = Mathf.Clamp(currentXRotation, minXAngle, maxXAngle); // ���ƽǶ�
            transform.localEulerAngles = new Vector3(currentXRotation, 0, 0);
        }

        //// �������������Զ�ӽ�
        //HandleZoom();
    }

    void HandleZoom()
    {
        // ��ȡ�����ֵ�����
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            // ����Ŀ��Z��λ��
            targetZoomDistance += -scrollInput * zoomSpeed;
            // ����Ŀ��Z��λ�����������Զ�ӽ�֮��
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoomDistance, maxZoomDistance);
        }

        // ʹ�ò�ֵƽ���ص����������λ��
        float newZ = Mathf.SmoothDamp(-transform.localPosition.z, targetZoomDistance, ref currentZoomVelocity, zoomSmoothTime);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -newZ);
    }
}