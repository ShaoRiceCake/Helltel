using UnityEngine;

public class MouseDrivenObjectController : MonoBehaviour
{
    [Header("Bindings")]
    public Transform chestPoint;        // �ؿڲο���
    public Transform leftHand;          // ���ְ󶨶���
    public float moveSpeed = 8f;        // �ƶ��ٶ�
    public float maxRadius = 1.5f;      // ����뾶

    [Header("Circle Rendering")]
    public Material circleMaterial;     // Բ�β���
    public float lineWidth = 0.05f;     // �������

    private Camera mainCamera;
    private Plane ecoPlane;             // ��ǰ�ƽ��
    private Vector3 planeOrigin;        // ƽ��ԭ��
    private Vector3 targetPosition;     // Ŀ��λ��
    private bool isRightMouseDown;      // �Ҽ��Ƿ���
    private bool isHandControl = true; // ��ǰ����ģʽ��Ĭ���ֲ����ƣ�
    private Vector3 startPos;           // ƽ��ԭ��
    private LineRenderer circleRenderer; // ���ڻ���Բ�ε�LineRenderer

    void Start()
    {
        mainCamera = Camera.main;
        InitializeEcoPlane();
        ResetHandPosition();
        startPos = transform.position;

        // ��ʼ��Բ����Ⱦ��
        InitializeCircleRenderer();
    }

    void Update()
    {
        HandleInput();

        if (isHandControl)
        {
            UpdateEcoPlane();
            UpdateHandPosition();
        }

        // ����Բ�ε�λ�ú���״
        UpdateCircleRenderer();
    }

    private void InitializeEcoPlane()
    {
        planeOrigin = chestPoint.position;
        ecoPlane = new Plane(transform.up, planeOrigin);
    }

    private void HandleInput()
    {
        isRightMouseDown = Input.GetMouseButton(1);

        // ����м��л�����ģʽ
        if (Input.GetMouseButtonDown(2))
        {
            isHandControl = !isHandControl;

            if (isHandControl)
            {
                // �л����ֲ�����ʱ������λ
                ResetHandPosition();
            }
        }
    }

    private void UpdateEcoPlane()
    {
        if (isRightMouseDown)
        {
            Vector3 planeNormal = transform.forward;
            planeOrigin = leftHand.position;
            ecoPlane = new Plane(planeNormal, planeOrigin);
        }
        else
        {
            planeOrigin = new Vector3(
                chestPoint.position.x,
                leftHand.position.y,
                chestPoint.position.z
            );
            ecoPlane = new Plane(transform.up, planeOrigin);
        }
    }

    private void UpdateHandPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float enter;

        if (ecoPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 localOffset = transform.InverseTransformPoint(hitPoint) -
                                 transform.InverseTransformPoint(planeOrigin);

            localOffset = Vector3.ClampMagnitude(localOffset, maxRadius);

            targetPosition = transform.TransformPoint(
                transform.InverseTransformPoint(planeOrigin) + localOffset
            );
        }

        leftHand.position = Vector3.Lerp(
            leftHand.position,
            targetPosition,
            Time.deltaTime * moveSpeed
        );
    }

    private void ResetHandPosition()
    {
        leftHand.position = chestPoint.position;
        targetPosition = chestPoint.position;
    }

    private void InitializeCircleRenderer()
    {
        // ����LineRenderer���
        circleRenderer = gameObject.AddComponent<LineRenderer>();

        // ���ò���
        circleRenderer.material = circleMaterial;

        // �����������
        circleRenderer.startWidth = lineWidth;
        circleRenderer.endWidth = lineWidth;

        // ����Բ�ηֶ���
        int segments = 36;
        circleRenderer.positionCount = segments + 1; // �պ�Բ����Ҫ��һ����
    }

    private void UpdateCircleRenderer()
    {
        if (circleRenderer == null) return;

        int segments = circleRenderer.positionCount - 1;
        float anglePerSegment = 360f / segments;

        // ����Բ�ε�λ��
        for (int i = 0; i <= segments; i++)
        {
            float angle = anglePerSegment * i;
            Vector3 point = GetCirclePoint(angle);
            circleRenderer.SetPosition(i, point);
        }
    }

    private Vector3 GetCirclePoint(float angle)
    {
        // ���Ƕ�ת��Ϊ����
        float radian = angle * Mathf.Deg2Rad;

        // ����Բ�ϵĵ�
        float x = Mathf.Cos(radian) * maxRadius;
        float z = Mathf.Sin(radian) * maxRadius;

        // ���ֲ�����ת��Ϊ��������
        return planeOrigin + transform.TransformDirection(new Vector3(x, 0, z));
    }
}