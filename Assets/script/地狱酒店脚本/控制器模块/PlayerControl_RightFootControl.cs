using UnityEngine;

public class PlayerControl_RightFootControl : PlayerControl_FootControl
{
    protected override void Start()
    {
        base.Start();
        controlHandler.onLiftRightLeg.AddListener(OnLiftRightLeg);
        controlHandler.onReleaseRightLeg.AddListener(OnReleaseRightLeg);
    }

    protected override void UnsubscribeEvents()
    {
        controlHandler.onLiftRightLeg.RemoveListener(OnLiftRightLeg);
        controlHandler.onReleaseRightLeg.RemoveListener(OnReleaseRightLeg);
    }

    // �¼�����̧����
    protected virtual void OnLiftRightLeg() 
    {
        isFootUp = true;

        particleAttachment.enabled = true;

        particleAttachment.BindToTarget(targetObject.transform); 
    }

    // �¼�����������
    protected virtual void OnReleaseRightLeg() 
    {
        // ���ڷ���״̬
        isFootUp = false;

        isCatching = false;

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
