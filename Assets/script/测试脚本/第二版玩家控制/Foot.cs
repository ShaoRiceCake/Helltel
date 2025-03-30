using UnityEngine;

public class Foot : MonoBehaviour
{
    public GameObject targetObject; // Ŀ������
    public GameObject movingObjectLeft; // ������Ƶ��˶�����
    public GameObject movingObjectRight; // �Ҽ����Ƶ��˶�����
    public GameObject forwardObject; // ����������ȷ��������Ķ���
    public float springForce = 10f; // ��������
    public float damping = 0.6f; // ����ϵ��
    public float moveSpeed = 15f; // �ƶ�����ײ�����ٶ�
    public float raycastDistance = 10f; // ���߼���������
    public float mouseDetectionTimeInterval = 0.015f; // ����ƶ���ⵥλʱ�������룩
    public float impulseCoefficient = 0.3f; // ����ϵ��
    public float landingThreshold = 0.5f; // ����ж���ֵ
    public float mouseSensitivity = 1f; // ���������
    public LayerMask raycastMask; // ���߼������

    private bool isSpringActiveLeft = false; // ����Ƿ���
    private bool isSpringActiveRight = false; // �Ҽ��Ƿ���
    private bool isObjectFixedLeft = false; // ������Ƶ��˶������Ƿ񱻹̶�
    private bool isObjectFixedRight = false; // �Ҽ����Ƶ��˶������Ƿ񱻹̶�
    private Vector3 targetPositionLeft; // ������Ƶ��˶���������߼��Ŀ�����
    private Vector3 targetPositionRight; // �Ҽ����Ƶ��˶���������߼��Ŀ�����
    private float timeCounter = 0f; // ʱ�������
    private Rigidbody movingRbLeft; // ������Ƶ��˶�����ĸ������
    private Rigidbody movingRbRight; // �Ҽ����Ƶ��˶�����ĸ������

    void Start()
    {
        // ��ȡ�˶�����ĸ������
        movingRbLeft = movingObjectLeft.GetComponent<Rigidbody>();
        movingRbRight = movingObjectRight.GetComponent<Rigidbody>();
    }

    void Update()
    {
        // ���������Ҽ��Ƿ�ͬʱ����
        if (Input.GetMouseButtonDown(0) && Input.GetMouseButtonDown(1))
        {
            // ���ȴ�������߼�
            isSpringActiveLeft = true;
            UnfixObject(movingRbLeft, ref isObjectFixedLeft); // ���������Ƶ��˶�����Ĺ̶�
        }
        else
        {
            // ����Ҽ��������£��������û�б�����
            if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0))
            {
                isSpringActiveRight = true;
                UnfixObject(movingRbRight, ref isObjectFixedRight); // ����Ҽ����Ƶ��˶�����Ĺ̶�
            }

            // �������������£������Ҽ�û�б�����
            if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
            {
                isSpringActiveLeft = true;
                UnfixObject(movingRbLeft, ref isObjectFixedLeft); // ���������Ƶ��˶�����Ĺ̶�
            }

            // ����Ҽ��ͷ�
            if (Input.GetMouseButtonUp(1))
            {
                isSpringActiveRight = false;
                MoveToTargetPosition(movingRbRight, targetPositionRight); // �Ҽ��ɿ�ʱ�ƶ�����ײ���
            }

            // �������ͷ�
            if (Input.GetMouseButtonUp(0))
            {
                isSpringActiveLeft = false;
                MoveToTargetPosition(movingRbLeft, targetPositionLeft); // ����ɿ�ʱ�ƶ�����ײ���
            }
        }

        // ���������µ���ײ��
        DetectGround(movingObjectLeft, ref targetPositionLeft); // ���������Ƶ��˶���������
        DetectGround(movingObjectRight, ref targetPositionRight); // ����Ҽ����Ƶ��˶���������

        // ����ƶ���ⵥλʱ���߼�
        if (isSpringActiveLeft || isSpringActiveRight)
        {
            timeCounter += Time.deltaTime;
            if (timeCounter >= mouseDetectionTimeInterval)
            {
                timeCounter = 0f; // ����ʱ�������
                DetectMouseMovement(); // ������λ�Ʋ�Ӧ�ó���
            }
        }

        // ����Ƿ���ز��̶�����
        if (!isSpringActiveLeft && IsObjectLanded(movingObjectLeft, targetPositionLeft))
        {
            FixObject(movingRbLeft, ref isObjectFixedLeft); // �̶�������Ƶ��˶�����
        }
        if (!isSpringActiveRight && IsObjectLanded(movingObjectRight, targetPositionRight))
        {
            FixObject(movingRbRight, ref isObjectFixedRight); // �̶��Ҽ����Ƶ��˶�����
        }
    }

    void FixedUpdate()
    {
        // ������Ƶ��˶�����ĵ���Լ��
        if (isSpringActiveLeft && targetObject != null && movingObjectLeft != null)
        {
            ApplySpringForce(movingRbLeft, targetObject.transform.position);
        }

        // �Ҽ����Ƶ��˶�����ĵ���Լ��
        if (isSpringActiveRight && targetObject != null && movingObjectRight != null)
        {
            ApplySpringForce(movingRbRight, targetObject.transform.position);
        }
    }

    void DetectGround(GameObject movingObject, ref Vector3 targetPosition)
    {
        // ���˶��������·�������
        Ray ray = new Ray(movingObject.transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, raycastMask))
        {
            // ������߼�⵽��ײ�����¼��ײ��
            targetPosition = hit.point;
            Debug.DrawLine(ray.origin, hit.point, Color.green); // ���ӻ�����
        }
        else
        {
            // ���δ��⵽��ײ������Ŀ��λ��
            targetPosition = movingObject.transform.position;
        }
    }

    void MoveToTargetPosition(Rigidbody movingRb, Vector3 targetPosition)
    {
        // ����Ŀ��λ���뵱ǰλ�õĲ�ֵ
        Vector3 direction = targetPosition - movingRb.position;
        float distance = direction.magnitude;

        // ����������һ����С����ֵ����ʼ�ƶ�
        if (distance > landingThreshold)
        {
            movingRb.velocity = direction.normalized * moveSpeed; // �����ٶ�
        }
    }

    void DetectMouseMovement()
    {
        // ʹ���������ƶ���
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ��ȡforwardObject�ĳ��򣬲�ȷ����ƽ����XZƽ��
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // ȷ������ƽ����XZƽ��
        forwardDirection.Normalize();

        // �����ҷ��򣨴�ֱ��forwardDirection��
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // �����λ������ת��Ϊ����forwardObject����ĳ�������
        Vector3 impulseDirection = (rightDirection * mouseX + forwardDirection * mouseY).normalized;

        // ����������Ҽ�״̬Ӧ�ó���
        if (isSpringActiveLeft)
        {
            movingRbLeft.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
        }
        if (isSpringActiveRight)
        {
            movingRbRight.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
        }
    }

    bool IsObjectLanded(GameObject movingObject, Vector3 targetPosition)
    {
        // ����˶������Ƿ�ӽ�Ŀ��λ��
        float distance = Vector3.Distance(movingObject.transform.position, targetPosition);
        return distance <= landingThreshold;
    }

    void FixObject(Rigidbody movingRb, ref bool isObjectFixed)
    {
        if (!isObjectFixed)
        {
            // ����ٶȺ�����
            movingRb.velocity = Vector3.zero;
            movingRb.angularVelocity = Vector3.zero;
            movingRb.isKinematic = true; // ����Ϊ�˶�ѧ���壬�̶�λ��

            isObjectFixed = true;
        }
    }

    void UnfixObject(Rigidbody movingRb, ref bool isObjectFixed)
    {
        if (isObjectFixed)
        {
            // ����̶�
            movingRb.isKinematic = false; // ����Ϊ����ѧ����
            isObjectFixed = false;
        }
    }

    void ApplySpringForce(Rigidbody movingRb, Vector3 targetPosition)
    {
        // ����Ŀ�����˶�����֮��ľ�������
        Vector3 direction = targetPosition - movingRb.position;
        float distance = direction.magnitude;

        // ���㵯����
        Vector3 springForceVector = direction.normalized * distance * springForce;

        // ����������
        Vector3 dampingForce = -movingRb.velocity * damping;

        // ʩ�Ӻ���
        movingRb.AddForce(springForceVector + dampingForce);
    }
}