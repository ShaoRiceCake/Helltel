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


    // �¼�����̧����
    protected virtual void OnLiftLeftLeg() 
    {
        // ����̧��״̬
        isFootUp = true;

        isCatching = false;

        particleAttachment.enabled = true;

        particleAttachment.BindToTarget(targetObject.transform);
    }

    // �¼�����������
    protected virtual void OnReleaseLeftLeg()
    {
        // ���ڷ���״̬
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
