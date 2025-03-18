using UnityEngine;

public class PlayerControl_FootControl : MonoBehaviour
{
    public GameObject targetObject; // Ŀ������
    public GameObject movingObjectLeft; // ������Ƶ��˶�����
    public GameObject movingObjectRight; // �Ҽ����Ƶ��˶�����
    public GameObject forwardObject; // ����ȷ��������Ķ���
    public float springForce = 10f; // ��������
    public float damping = 0.6f; // ����ϵ��
    public float moveSpeed = 15f; // �����ٶ�
    public float fixedHeight = 2f; // �̶�����߶�
    public float impulseCoefficient = 0.3f; // ����ϵ��
    public float mouseSensitivity = 0.1f; // ���������

    private bool isLeftFootUp = false; // �����Ƿ�̧��
    private bool isRightFootUp = false; // �����Ƿ�̧��
    private bool isFreeFoot = false; // �Ȳ����䵽ָ��λ��

    private Rigidbody movingRbLeft; // ������Ƶ��˶�����ĸ������
    private Rigidbody movingRbRight; // �Ҽ����Ƶ��˶�����ĸ������
    private CatchControl catchControlLeft; // ������Ƶ�ץȡ�������
    private CatchControl catchControlRight; // �Ҽ����Ƶ�ץȡ�������

    private PlayerControlInformationProcess controlHandler; // �����¼�������

    void Start()
    {
        // ��ȡ�˶�����ĸ������
        movingRbLeft = movingObjectLeft.GetComponent<Rigidbody>();
        movingRbRight = movingObjectRight.GetComponent<Rigidbody>();

        // ��ȡץȡ�������
        catchControlLeft = movingObjectLeft.GetComponent<CatchControl>();
        catchControlRight = movingObjectRight.GetComponent<CatchControl>();

        // ��ȡ PlayerControlInformationProcess ���
        controlHandler = GetComponent<PlayerControlInformationProcess>();

        if (!controlHandler || !movingObjectLeft || !movingObjectRight)
        {
            Debug.LogError("Some of the PlayerControl_FootControl components are missing!");
            return;
        }

        // �����¼�
        controlHandler.onLiftLeftLeg.AddListener(OnLiftLeftLeg);
        controlHandler.onLiftRightLeg.AddListener(OnLiftRightLeg);
        controlHandler.onReleaseLeftLeg.AddListener(OnReleaseLeftLeg);
        controlHandler.onReleaseRightLeg.AddListener(OnReleaseRightLeg);
        controlHandler.onCancelLegGrab.AddListener(OnCancelLegGrab);
        controlHandler.onDefaultMode.AddListener(OnDefaultMode);
        controlHandler.onMouseMove.AddListener(OnMouseMove);
    }

    void OnDestroy()
    {
        // ȡ�������¼�
        if (controlHandler != null)
        {
            controlHandler.onLiftLeftLeg.RemoveListener(OnLiftLeftLeg);
            controlHandler.onLiftRightLeg.RemoveListener(OnLiftRightLeg);
            controlHandler.onReleaseLeftLeg.RemoveListener(OnReleaseLeftLeg);
            controlHandler.onReleaseRightLeg.RemoveListener(OnReleaseRightLeg);
            controlHandler.onCancelLegGrab.RemoveListener(OnCancelLegGrab);
            controlHandler.onDefaultMode.RemoveListener(OnDefaultMode);
            controlHandler.onMouseMove.RemoveListener(OnMouseMove);
        }
    }

    void Update()
    {
        if (!isLeftFootUp)
        {
            if (movingObjectLeft != null && !catchControlLeft.attchAimPos && !isFreeFoot)
            {
                MoveDown(movingObjectLeft);
            }
        }

        if (!isRightFootUp)
        {
            if (movingObjectRight != null && !catchControlLeft.attchAimPos && !isFreeFoot)
            {
                MoveDown(movingObjectRight);
            }

        }


    }

    void FixedUpdate()
    {
        // ʩ��̧��ʱ�ĵ���Լ��-��
        if (isLeftFootUp && targetObject != null && movingObjectLeft != null)
        {
            ApplySpringForce(movingRbLeft, targetObject.transform.position);
        }

        // ʩ��̧��ʱ�ĵ���Լ��-��
        if (isRightFootUp && targetObject != null && movingObjectRight != null)
        {
            ApplySpringForce(movingRbRight, targetObject.transform.position);
        }
    }

    // �¼�����̧����
    private void OnLiftLeftLeg()
    {
        isLeftFootUp = true;
        catchControlLeft.enabled = false;
        isFreeFoot = false;
    }

    // �¼�����̧����
    private void OnLiftRightLeg()
    {
        isRightFootUp = true;
        catchControlRight.enabled = false;
        isFreeFoot = false;
    }

    // �¼�����������
    private void OnReleaseLeftLeg()
    {
        isLeftFootUp = false;
        catchControlLeft.enabled = true;

    }

    // �¼�����������
    private void OnReleaseRightLeg()
    {
        catchControlRight.enabled = true;
        isRightFootUp = false;
    }

    // �¼�����ȡ���Ȳ�ץȡ
    private void OnCancelLegGrab()
    {
    }

    // �¼��������κΰ����¼�
    private void OnDefaultMode()
    {
    }

    // �¼���������ƶ�
    private void OnMouseMove(Vector2 mouseDelta)
    {
        // ��ȡforwardObject�ĳ��򣬲�ȷ����ƽ����XZƽ��
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // ȷ������ƽ����XZƽ��
        forwardDirection.Normalize();

        // �����ҷ��򣨴�ֱ��forwardDirection��
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // �����λ������ת��Ϊ����forwardObject����ĳ�������
        Vector3 impulseDirection = (rightDirection * mouseDelta.x * mouseSensitivity + forwardDirection * mouseDelta.y * mouseSensitivity).normalized;

        // ����������Ҽ�״̬Ӧ�ó���
        if (isLeftFootUp)
        {
            movingRbLeft.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
        }
        if (isRightFootUp)
        {
            movingRbRight.AddForce(impulseDirection * impulseCoefficient, ForceMode.Impulse);
        }
    }

    void MoveDown(GameObject movingObject)
    {
        // ����Ŀ��λ��
        Vector3 targetPosition = movingObject.transform.position - Vector3.up * fixedHeight;

        // �ƶ�����
        movingObject.transform.position = Vector3.MoveTowards(movingObject.transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if(Vector3.Distance(movingObject.transform.position, targetPosition) < 0.1f)
        {
            isFreeFoot = true;
        }

    }

    // ʩ�ӵ�����
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