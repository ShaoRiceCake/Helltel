using Obi;
using UnityEngine;

[RequireComponent(typeof(RaycastTool))]
[RequireComponent(typeof(ObiParticleGroupImpacter))]
public abstract class PlayerControl_FootControl : PlayerControl_BaseControl
{
    public GameObject targetObject; // Ŀ������
    public GameObject forwardObject; // ����ȷ��������Ķ���

    protected GameObject footObject; //�㲿���ƶ���

    protected RaycastTool raycastTool;
    protected ObiParticleGroupImpacter particleGroupImpacter;
    // ����
    public float mouseSensitivity = 1f;
    public float detectionTime= 0.1f;

    protected float timeCounter;
    protected Vector3 impulseDirection;

    protected bool isFootUp = false; // �Ƿ�̧��
    protected bool isCatching = false; // �Ƿ�ץȡ


    protected override void Start()
    {
        base.Start();

        footObject = this.gameObject;

        particleGroupImpacter = footObject.GetComponent<ObiParticleGroupImpacter>();
        raycastTool = footObject.GetComponent<RaycastTool>();

        if (!CheckRequiredComponents())
        {
            return;
        }

        particleGroupImpacter.obiParticleAttachment = particleAttachment;
    }

    protected bool CheckRequiredComponents()
    {
        string missingComponent =
            !footObject ? nameof(footObject) :
            !forwardObject ? nameof(forwardObject) :
            !particleAttachment ? nameof(particleAttachment) :
            !particleGroupImpacter ? nameof(particleGroupImpacter) :
            !raycastTool ? nameof(raycastTool) : null;

        if (missingComponent != null)
        {
            Debug.LogError($"Missing component: {missingComponent} on {gameObject.name}");
            return false;
        }

        return true;
    }


    protected virtual void OnDestroy()
    {
        if (controlHandler != null)
        {
            UnsubscribeEvents();
        }
    }

    // �����¼�
    protected virtual void SubscribeEvents()
    {
        controlHandler.onCancelLegGrab.AddListener(OnCancelLegGrab);
        controlHandler.onDefaultMode.AddListener(OnDefaultMode);
        controlHandler.onMouseMove.AddListener(OnMouseMove);
    }

    // ȡ�������¼�
    protected virtual void UnsubscribeEvents()
    {
        controlHandler.onCancelLegGrab.RemoveListener(OnCancelLegGrab);
        controlHandler.onDefaultMode.RemoveListener(OnDefaultMode);
        controlHandler.onMouseMove.RemoveListener(OnMouseMove);
    }


    // �¼�����ȡ���Ȳ�Լ��
    protected void OnCancelLegGrab()
    {
    }

    // �¼������޲���״̬
    protected void OnDefaultMode()
    {
        Transform rayTrans = raycastTool.GetHitTrans();
        if (rayTrans != null && !isCatching)
        {
            particleAttachment.BindToTarget(rayTrans);
            isCatching = true;
        }
        else
        {
            particleAttachment.enabled = false;
            isCatching = false;
        }
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
        impulseDirection = (rightDirection * mouseDelta.x * mouseSensitivity + forwardDirection * mouseDelta.y * mouseSensitivity).normalized;

    }


    protected virtual void Update()
    {

        // ���ݵ�λʱ��ʩ���Ȳ��ƶ�����
        if (isFootUp)
        {
            timeCounter += Time.deltaTime;
            if (timeCounter >= detectionTime)
            {
                timeCounter = 0f; // ����ʱ�������
                FootImpact(impulseDirection);
            }
        }

    }

    // ���������������ʩ��Ĭ�ϳ���
    protected  void FootImpact(Vector3 impulseDirection)
    {
        particleGroupImpacter.TriggerImpulse(impulseDirection);
    }

}