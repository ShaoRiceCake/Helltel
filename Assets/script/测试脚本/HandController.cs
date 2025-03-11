using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("��������")]
    public Transform target;          // Ҫ���Ƶ�����
    public float mouseSensitivity = 0.01f; // ���������
    public float maxRadius = 5f;       // ����뾶

    [Header("�ο�������")]
    public Transform centerPoint;     // ����Ĳο���

    private Vector3 referencePosition;
    private Vector3 lastMousePosition;
    private bool isXYMode;            // ��ǰ�Ƿ���XYģʽ

    void Start()
    {
        InitializeReferences();
        UpdateLockPosition();
    }

    void Update()
    {
        UpdateReferences();
        HandleModeSwitch();
        HandleMouseInput();
        ClampToRadius();
        UpdateLockPosition();
    }

    void InitializeReferences()
    {
        referencePosition = centerPoint != null ? centerPoint.position : transform.position;
        lastMousePosition = Input.mousePosition;
    }

    void UpdateReferences()
    {
        if (centerPoint != null)
        {
            referencePosition = centerPoint.position;
        }
    }

    void HandleModeSwitch()
    {
        // �Ҽ�����ʱ�л�ΪXYģʽ���ɿ��ָ�XZģʽ
        isXYMode = Input.GetMouseButton(1);
    }

    void HandleMouseInput()
    {
        Vector3 currentMousePos = Input.mousePosition;
        Vector3 mouseDelta = currentMousePos - lastMousePosition;
        lastMousePosition = currentMousePos;

        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 cameraUp = Camera.main.transform.up;

        // ����ģʽ����ƽ��λ��
        Vector3 worldDelta = (cameraRight * mouseDelta.x + cameraUp * mouseDelta.y) * mouseSensitivity;

        if (isXYMode)
        {
            // XYģʽ������X/Y��λ�ƣ�Z���ɺ�������
            worldDelta.z = 0;
        }
        else
        {
            // XZģʽ������X/Z��λ�ƣ�Y���ɺ�������
            worldDelta.y = 0;
        }

        target.position += worldDelta;
    }

    void ClampToRadius()
    {
        Vector3 offset = target.position - referencePosition;
        float currentDistance = 0f;

        if (isXYMode)
        {
            // ����XYƽ�����
            currentDistance = Mathf.Sqrt(offset.x * offset.x + offset.y * offset.y);
        }
        else
        {
            // ����XZƽ�����
            currentDistance = Mathf.Sqrt(offset.x * offset.x + offset.z * offset.z);
        }

        if (currentDistance > maxRadius)
        {
            if (isXYMode)
            {
                Vector2 clamped = new Vector2(offset.x, offset.y).normalized * maxRadius;
                target.position = new Vector3(
                    referencePosition.x + clamped.x,
                    referencePosition.y + clamped.y,
                    target.position.z
                );
            }
            else
            {
                Vector2 clamped = new Vector2(offset.x, offset.z).normalized * maxRadius;
                target.position = new Vector3(
                    referencePosition.x + clamped.x,
                    target.position.y,
                    referencePosition.z + clamped.y
                );
            }
        }
    }

    void UpdateLockPosition()
    {
        if (isXYMode)
        {
            // XYģʽ������Z�ᵽ�ο���
            target.position = new Vector3(
                target.position.x,
                target.position.y,
                referencePosition.z
            );
        }
        else
        {
            // XZģʽ������Y�ᵽ�ο���
            target.position = new Vector3(
                target.position.x,
                referencePosition.y,
                target.position.z
            );
        }
    }
}