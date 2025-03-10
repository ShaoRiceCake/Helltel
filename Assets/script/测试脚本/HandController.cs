using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("��������")]
    public Transform target;          // Ҫ���Ƶ�����
    public float mouseSensitivity = 0.01f; // ���������
    public float maxRadius = 5f;       // ����뾶

    [Header("�ο�������")]
    public Transform centerPoint;     // ����Ĳο��㣨Ĭ��Ϊ����λ�ã�

    private Vector3 referencePosition; // ��׼λ��
    private Vector3 lastMousePosition;

    void Start()
    {
        InitializeReferences();
        LockYPosition();
    }

    void Update()
    {
        UpdateReferencePosition();
        HandleMouseInput();
        ClampToRadius();
        LockYPosition();
    }

    void InitializeReferences()
    {
        // ��ʼ����׼λ��
        referencePosition = centerPoint != null ? centerPoint.position : transform.position;
        lastMousePosition = Input.mousePosition;
    }

    void UpdateReferencePosition()
    {
        // ��̬���²ο���λ��
        if (centerPoint != null)
        {
            referencePosition = centerPoint.position;
        }
    }

    void HandleMouseInput()
    {
        Vector3 currentMousePos = Input.mousePosition;
        Vector3 mouseDelta = currentMousePos - lastMousePosition;
        lastMousePosition = currentMousePos;

        // ������ƶ�ת��Ϊ����ռ�λ��
        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 cameraUp = Camera.main.transform.up;

        // ����XZƽ��λ������
        Vector3 worldDelta = new Vector3(
            mouseDelta.x * cameraRight.x + mouseDelta.y * cameraUp.x,
            0,
            mouseDelta.x * cameraRight.z + mouseDelta.y * cameraUp.z
        ) * mouseSensitivity;

        target.position += worldDelta;
    }

    void ClampToRadius()
    {
        // ������ο����ˮƽ����
        Vector3 horizontalOffset = target.position - referencePosition;
        horizontalOffset.y = 0;

        if (horizontalOffset.magnitude > maxRadius)
        {
            target.position = referencePosition + horizontalOffset.normalized * maxRadius;
        }
    }

    void LockYPosition()
    {
        // ���ֳ�ʼY���겻��
        target.position = new Vector3(
            target.position.x,
            referencePosition.y,
            target.position.z
        );
    }
}