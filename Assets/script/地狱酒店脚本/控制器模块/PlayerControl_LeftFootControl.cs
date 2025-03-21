using UnityEngine;

public class PlayerControl_LeftFootControl : PlayerControl_FootControl
{
    protected override void Start()
    {
        base.Start();
        controlHandler.onLiftLeftLeg.AddListener(OnLiftLeftLeg);
        controlHandler.onReleaseLeftLeg.AddListener(OnReleaseLeftLeg);

    }

    protected override void UnsubscribeEvents()
    {
        controlHandler.onLiftLeftLeg.RemoveListener(OnLiftLeftLeg);
        controlHandler.onReleaseLeftLeg.RemoveListener(OnReleaseLeftLeg);
    }


    // 事件处理：抬左腿
    protected virtual void OnLiftLeftLeg() 
    {
        // 处于抬腿状态
        isFootUp = true;

        isCatching = false;

        particleAttachment.enabled = true;

        particleAttachment.BindToTarget(targetObject.transform);
    }

    // 事件处理：放左腿
    protected virtual void OnReleaseLeftLeg()
    {
        // 处于放腿状态
        isFootUp = false;


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

    protected override void Update()
    {
        base.Update();
    }

}
