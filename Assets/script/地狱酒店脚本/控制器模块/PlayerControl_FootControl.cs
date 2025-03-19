using Obi;
using UnityEngine;

public abstract class PlayerControl_FootControl : MonoBehaviour
{
    public GameObject targetObject; // Ŀ������
    public GameObject forwardObject; // ����ȷ��������Ķ���
    public GameObject footObject; //�㲿���ƶ���


    private CatchControl catchControl;
    private ObiParticleAttachment particleAttachment;
    private ObiParticleGroupDragger particleGroupDragger;
    private ObiParticleGroupImpacter particleGroupImpacter;
    private PlayerControlInformationProcess controlHandler; // �����¼�������

    // ����
    public float downForce = 10f;
    public float mouseSensitivity = 1f;
    public float impulseCoefficient = 10f;
    public float springForce = 10f;
    public float damping = 1f;

    protected bool isLeftFootUp = false; // �Ƿ�̧��

    void Start()
    {
        // ��ȡ���
        controlHandler = GetComponent<PlayerControlInformationProcess>();
        catchControl = footObject.GetComponent<CatchControl>();
        particleAttachment = footObject.GetComponent<ObiParticleAttachment>();
        particleGroupDragger = footObject.GetComponent<ObiParticleGroupDragger>();
        particleGroupImpacter = footObject.GetComponent<ObiParticleGroupImpacter>();

        if (!controlHandler || !footObject || !forwardObject || !catchControl || !particleAttachment || !particleGroupDragger || !particleGroupImpacter)
        {
            Debug.LogError("Some of the PlayerControl_FootControl components are missing!");
            return;
        }

        // �����¼�
        SubscribeEvents();
    }

    protected virtual void OnDestroy()
    {
        // ȡ�������¼�
        if (controlHandler != null)
        {
            UnsubscribeEvents();
        }
    }

    // �����¼�
    protected virtual void SubscribeEvents()
    {
        controlHandler.onLiftLeftLeg.AddListener(OnLiftLeftLeg);
        controlHandler.onLiftRightLeg.AddListener(OnLiftRightLeg);
        controlHandler.onReleaseLeftLeg.AddListener(OnReleaseLeftLeg);
        controlHandler.onReleaseRightLeg.AddListener(OnReleaseRightLeg);
        controlHandler.onCancelLegGrab.AddListener(OnCancelLegGrab);
        controlHandler.onDefaultMode.AddListener(OnDefaultMode);
        controlHandler.onMouseMove.AddListener(OnMouseMove);
    }

    // ȡ�������¼�
    protected virtual void UnsubscribeEvents()
    {
        controlHandler.onLiftLeftLeg.RemoveListener(OnLiftLeftLeg);
        controlHandler.onLiftRightLeg.RemoveListener(OnLiftRightLeg);
        controlHandler.onReleaseLeftLeg.RemoveListener(OnReleaseLeftLeg);
        controlHandler.onReleaseRightLeg.RemoveListener(OnReleaseRightLeg);
        controlHandler.onCancelLegGrab.RemoveListener(OnCancelLegGrab);
        controlHandler.onDefaultMode.RemoveListener(OnDefaultMode);
        controlHandler.onMouseMove.RemoveListener(OnMouseMove);
    }

    // �¼�����̧����
    protected virtual void OnLiftLeftLeg(){}

    // �¼�����̧����
    protected virtual void OnLiftRightLeg(){}

    // �¼�����������
    protected virtual void OnReleaseLeftLeg(){}

    // �¼�����������
    protected virtual void OnReleaseRightLeg(){}

    // �¼�����ȡ���Ȳ�Լ��
    protected void OnCancelLegGrab()
    {
    }

    // �¼������޲���״̬
    protected void OnDefaultMode()
    {
    }

    // �¼���������ƶ�
    protected void OnMouseMove(Vector2 mouseDelta)
    {
        // ��ȡforwardObject�ĳ��򣬲�ȷ����ƽ����XZƽ��
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // ȷ������ƽ����XZƽ��
        forwardDirection.Normalize();

        // �����ҷ��򣨴�ֱ��forwardDirection��
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // �����λ������ת��Ϊ����forwardObject����ĳ�������
        Vector3 impulseDirection = (rightDirection * mouseDelta.x * mouseSensitivity + forwardDirection * mouseDelta.y * mouseSensitivity).normalized;
    }

}