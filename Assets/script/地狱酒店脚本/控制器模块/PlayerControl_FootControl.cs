using Obi;
using UnityEngine;

[RequireComponent(typeof(RaycastTool))]
[RequireComponent(typeof(ObiParticleGroupImpacter))]
public abstract class PlayerControl_FootControl : PlayerControl_BaseControl
{
    public GameObject targetObject; // 目标点对象
    public GameObject forwardObject; // 用于确定正朝向的对象

    protected GameObject footObject; //足部控制对象

    protected RaycastTool raycastTool;
    protected ObiParticleGroupImpacter particleGroupImpacter;
    // 参数
    public float mouseSensitivity = 1f;
    public float detectionTime= 0.1f;

    protected float timeCounter;
    protected Vector3 impulseDirection;

    protected bool isFootUp = false; // 是否抬起
    protected bool isCatching = false; // 是否抓取


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

    // 订阅事件
    protected virtual void SubscribeEvents()
    {
        controlHandler.onCancelLegGrab.AddListener(OnCancelLegGrab);
        controlHandler.onDefaultMode.AddListener(OnDefaultMode);
        controlHandler.onMouseMove.AddListener(OnMouseMove);
    }

    // 取消订阅事件
    protected virtual void UnsubscribeEvents()
    {
        controlHandler.onCancelLegGrab.RemoveListener(OnCancelLegGrab);
        controlHandler.onDefaultMode.RemoveListener(OnDefaultMode);
        controlHandler.onMouseMove.RemoveListener(OnMouseMove);
    }


    // 事件处理：取消腿部约束
    protected void OnCancelLegGrab()
    {
    }

    // 事件处理：无操作状态
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
        impulseDirection = (rightDirection * mouseDelta.x * mouseSensitivity + forwardDirection * mouseDelta.y * mouseSensitivity).normalized;

    }


    protected virtual void Update()
    {

        // 根据单位时间施加腿部移动冲量
        if (isFootUp)
        {
            timeCounter += Time.deltaTime;
            if (timeCounter >= detectionTime)
            {
                timeCounter = 0f; // 重置时间计数器
                FootImpact(impulseDirection);
            }
        }

    }

    // 调用粒子组冲量器施加默认冲量
    protected  void FootImpact(Vector3 impulseDirection)
    {
        particleGroupImpacter.TriggerImpulse(impulseDirection);
    }

}