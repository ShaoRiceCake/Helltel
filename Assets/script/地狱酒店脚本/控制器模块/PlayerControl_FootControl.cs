using Obi;
using UnityEngine;

public abstract class PlayerControl_FootControl : MonoBehaviour
{
    public GameObject targetObject; // 目标点对象
    public GameObject forwardObject; // 用于确定正朝向的对象
    public GameObject footObject; //足部控制对象


    private CatchControl catchControl;
    private ObiParticleAttachment particleAttachment;
    private ObiParticleGroupDragger particleGroupDragger;
    private ObiParticleGroupImpacter particleGroupImpacter;
    private PlayerControlInformationProcess controlHandler; // 控制事件处理器

    // 参数
    public float downForce = 10f;
    public float mouseSensitivity = 1f;
    public float impulseCoefficient = 10f;
    public float springForce = 10f;
    public float damping = 1f;

    protected bool isLeftFootUp = false; // 是否抬起

    void Start()
    {
        // 获取组件
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

        // 订阅事件
        SubscribeEvents();
    }

    protected virtual void OnDestroy()
    {
        // 取消订阅事件
        if (controlHandler != null)
        {
            UnsubscribeEvents();
        }
    }

    // 订阅事件
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

    // 取消订阅事件
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

    // 事件处理：抬左腿
    protected virtual void OnLiftLeftLeg(){}

    // 事件处理：抬右腿
    protected virtual void OnLiftRightLeg(){}

    // 事件处理：放左腿
    protected virtual void OnReleaseLeftLeg(){}

    // 事件处理：放右腿
    protected virtual void OnReleaseRightLeg(){}

    // 事件处理：取消腿部约束
    protected void OnCancelLegGrab()
    {
    }

    // 事件处理：无操作状态
    protected void OnDefaultMode()
    {
    }

    // 事件处理：鼠标移动
    protected void OnMouseMove(Vector2 mouseDelta)
    {
        // 获取forwardObject的朝向，并确保其平行于XZ平面
        Vector3 forwardDirection = forwardObject.transform.forward;
        forwardDirection.y = 0; // 确保朝向平行于XZ平面
        forwardDirection.Normalize();

        // 计算右方向（垂直于forwardDirection）
        Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection).normalized;

        // 将鼠标位移向量转换为基于forwardObject朝向的冲量方向
        Vector3 impulseDirection = (rightDirection * mouseDelta.x * mouseSensitivity + forwardDirection * mouseDelta.y * mouseSensitivity).normalized;
    }

}