using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootController : MonoBehaviour
{
    [Header("���Ŀ���")]
    private Transform target;         // �����Ƶ����壨��̬����Ϊ����Ķ����Transform��
    public Transform mousePointer;   // ��ά�ռ��е�Ŀ���

    [Header("���Ʋ���")]
    public float activationSpeed = 1f;    // ������������ٶȣ�����/�룩
    public float rotationForce = 150f;       // ��ת������
    public float dampingForce = 3f;         // ��ת������
    public float baseHeight = 1f;           // ��׼�߶�ƫ��
    public float mouseSensitivity = 0.01f;  // ����ƶ������ȣ����������ȣ�

    [Header("�ռ�����")]
    public float followDistance = 5f;       // Ĭ�ϸ������
    public LayerMask collisionMask;         // ��ײ����

    [Header("��������")]
    public GameObject object1;              // ����1
    public GameObject object2;              // ����2

    private Rigidbody rb;
    private Vector3 defaultPosition;
    private Vector3 lastMousePos;
    private bool isDragging;
    private GameObject activeObject; // ��ǰ����Ķ���

    void Start()
    {
        InitializeComponents();
        SetupPhysics();
        DeactivateAllObjects(); // ��ʼʱʧ�����ж���
    }

    void Update()
    {
        defaultPosition = GetDefaultPosition();

        // ��̬���� target Ϊ�������� Transform
        if (activeObject != null)
        {
            target = activeObject.transform;
        }
        else
        {
            target = null; // û�м������ʱ��target Ϊ null
        }

        HandleInput();
        UpdatePointerPosition();
    }

    void FixedUpdate()
    {
        if (target != null) // ȷ�� target ��Ϊ null
        {
            ApplyRotationPhysics();
        }
    }

    void InitializeComponents()
    {
        mousePointer.position = defaultPosition;
        lastMousePos = Input.mousePosition;  // ��ʼ�����λ��
    }

    void SetupPhysics()
    {
        if (target != null)
        {
            rb = target.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezePosition;
            rb.maxAngularVelocity = 20f;
        }
    }

    void HandleInput()
    {
        // �������ʱ�������1
        if (Input.GetMouseButtonDown(0))
        {
            SetActiveObject(object1);
        }
        // ����ɿ�ʱʧ�����1
        if (Input.GetMouseButtonUp(0))
        {
            if (activeObject == object1)
            {
                DeactivateAllObjects();
            }
        }

        // �Ҽ�����ʱ�������2
        if (Input.GetMouseButtonDown(1))
        {
            SetActiveObject(object2);
        }
        // �Ҽ��ɿ�ʱʧ�����2
        if (Input.GetMouseButtonUp(1))
        {
            if (activeObject == object2)
            {
                DeactivateAllObjects();
            }
        }

        // ���ͬʱ����������Ҽ���ȷ��ֻ��һ�����󼤻�
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            if (activeObject != object1 && activeObject != object2)
            {
                SetActiveObject(object1); // Ĭ�ϼ������1
            }
        }
    }

    void SetActiveObject(GameObject newActiveObject)
    {
        if (activeObject != null && activeObject != newActiveObject)
        {
            activeObject.SetActive(false); // ʧ�ǰ����
        }
        activeObject = newActiveObject;
        activeObject.SetActive(true); // �����¶���

        // ���� target �� Rigidbody
        if (activeObject != null)
        {
            target = activeObject.transform;
            rb = target.GetComponent<Rigidbody>();
            SetupPhysics();
        }
    }

    void DeactivateAllObjects()
    {
        if (object1 != null) object1.SetActive(false);
        if (object2 != null) object2.SetActive(false);
        activeObject = null;
        target = null; // û�м������ʱ��target Ϊ null
    }

    public void ResetPointer()
    {
        mousePointer.position = defaultPosition;  // ˲���λ
        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    void UpdatePointerPosition()
    {
        if (target == null) return; // û�м������ʱ����

        Vector3 currentMousePos = Input.mousePosition;
        Vector3 mouseDelta = currentMousePos - lastMousePos;  // �����������λ��
        lastMousePos = currentMousePos;

        if (mouseDelta.magnitude > activationSpeed * Time.deltaTime)
        {
            Update3DPointerPosition(mouseDelta);
        }
        else if (!isDragging)
        {
            ResetPointer();  // ���϶�״̬��˲���λ
        }
    }

    void Update3DPointerPosition(Vector3 mouseDelta)
    {
        // ��������Ļ�ռ�λ��ת��Ϊ����ռ�λ��
        Vector3 worldDelta = Camera.main.transform.right * mouseDelta.x + Camera.main.transform.up * mouseDelta.y;
        worldDelta *= mouseSensitivity;  // Ӧ��������

        // ����Y����Ϊ���ض����Y����
        worldDelta.y = 0;

        // �������ָ���λ��
        mousePointer.position += worldDelta;

        // �������ָ����ƽ�����˶�
        mousePointer.position = new Vector3(
            mousePointer.position.x,
            mousePointer.position.y,
            mousePointer.position.z
        );
    }

    void ApplyRotationPhysics()
    {
        if (target == null) return; // û�м������ʱ����

        Vector3 targetDirection = mousePointer.position - target.position;
        if (targetDirection == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion rotationDelta = targetRotation * Quaternion.Inverse(target.rotation);

        rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);

        Vector3 angularVelocity = axis * (angle * Mathf.Deg2Rad * rotationForce);
        angularVelocity -= rb.angularVelocity * dampingForce;

        rb.AddTorque(angularVelocity, ForceMode.Acceleration);
    }

    Vector3 GetDefaultPosition()
    {
        Vector3 Eco = new Vector3(this.transform.position.x, this.transform.position.y + baseHeight, this.transform.position.z);

        if (target == null) return Eco; // û�м������ʱ����Ĭ��ֵ

        return Eco;
    }
}