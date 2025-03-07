using UnityEngine;

public class LimbController : MonoBehaviour
{
    [Header("Bindings")]
    public Transform leftHand;          // ���ְ󶨶���
    public Transform detectionPlane;    // ���ƽ��ο�����
    public float moveSpeed = 8f;        // �ƶ��ٶ�
    public float rayDistance = 10f;    // ���߼�����

    [Header("States")]
    public bool isHandControl = true;   // ��ǰ�Ƿ��ֲ�����ģʽ
    private bool useLocalPlane;         // �Ƿ�ʹ�þֲ�ƽ��
    private Camera mainCamera;         // �����������

    private Vector3 targetPosition;    // Ŀ��λ��
    private Plane currentPlane;        // ��ǰ���ƽ��

    void Start()
    {
        mainCamera = Camera.main;
        InitializeDefaultPlane();
    }

    void Update()
    {
        HandleInput();
        UpdateControlState();
    }

    void LateUpdate()
    {
        if (isHandControl)
        {
            UpdateHandPosition();
        }
    }

    private void InitializeDefaultPlane()
    {
        // ��ʼ��Ĭ��XZƽ�棨ʹ�ü��ƽ���Y���꣩
        currentPlane = new Plane(Vector3.up, detectionPlane.position);
    }

    private void HandleInput()
    {
        // �л�����ģʽ���м�������
        if (Input.GetMouseButtonDown(2))
        {
            isHandControl = !isHandControl;
            // ���������Ӻ������Ȳ������߼�
        }

        // �Ҽ�����ʱ�л��ֲ�ƽ��
        useLocalPlane = Input.GetMouseButton(1);
    }

    private void UpdateControlState()
    {
        if (useLocalPlane)
        {
            // �������ڽ�ɫ�泯�����ƽ�棨Y�᷽��
            Vector3 planeNormal = mainCamera.transform.forward;
            Vector3 planePosition = leftHand.position;
            currentPlane = new Plane(planeNormal, planePosition);
        }
        else
        {
            InitializeDefaultPlane();
        }
    }

    private void UpdateHandPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float enter;

        if (currentPlane.Raycast(ray, out enter))
        {
            // �������߼�����
            enter = Mathf.Min(enter, rayDistance);
            targetPosition = ray.GetPoint(enter);
        }

        // ƽ���ƶ��ֲ�λ��
        leftHand.position = Vector3.Lerp(
            leftHand.position,
            targetPosition,
            Time.deltaTime * moveSpeed
        );
    }

    // ��������ӵ��Ȳ����Ʒ���
    // private void UpdateLegPosition() {...}
}